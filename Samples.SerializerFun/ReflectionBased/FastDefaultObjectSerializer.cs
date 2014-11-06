namespace Samples.SerializerFun.ReflectionBased
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;

    public class FastDefaultObjectSerializer : SubSerializerBase
    {
        private ObjectIDGenerator idGenerator = new ObjectIDGenerator();

        private Dictionary<long, object> deserializedInstanceCache = new Dictionary<long, object>();

        /// <summary>
        /// For each type, cache of Func to get all fields of type instance
        /// </summary>
        private ConcurrentDictionary<Type, List<Tuple<Type, Func<object, object>>>> getters = new ConcurrentDictionary<Type, List<Tuple<Type, Func<object, object>>>>();

        /// <summary>
        /// For each type, cache of Func to set all fields of type instance
        /// </summary>
        private ConcurrentDictionary<Type, Func<ExtendedBinaryReader, object, object>> setters = new ConcurrentDictionary<Type, Func<ExtendedBinaryReader, object, object>>();

        public FastDefaultObjectSerializer(RootSerializer root)
            : base(root)
        {
        }

        public override void Done()
        {
            base.Done();

            this.idGenerator = new ObjectIDGenerator();
            this.deserializedInstanceCache.Clear();
        }

        public override bool CanApply(Type type)
        {
            return Type.GetTypeCode(type) == TypeCode.Object;
        }

        public override void Serialize(ExtendedBinaryWriter writer, object source, Type sourceType)
        {
            bool firstTime = true;

            // no need to store key for value types
            if (!sourceType.IsValueType)
            {
                // generate unique id for object, in order not to save same object multiple times
                var key = this.idGenerator.GetId(source, out firstTime);

                writer.Write(firstTime);
                writer.Write(key);
            }

            if (firstTime)
            {
                List<Tuple<Type, Func<object, object>>> gettersForType;
                if (!this.getters.TryGetValue(sourceType, out gettersForType))
                {
                    gettersForType = new List<Tuple<Type, Func<object, object>>>();

                    // create getter list from fields
                    foreach (var prop in sourceType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                    {
                        gettersForType.Add(Tuple.Create(prop.FieldType, CreateGetter(prop)));
                    }

                    this.getters.TryAdd(sourceType, gettersForType);
                }

                // serialize each field using the previously generated getters
                foreach (var s in gettersForType)
                {
                    this.SerializeBase(s.Item1, s.Item2(source), writer);
                }
            }
        }

        public override object Deserialize(ExtendedBinaryReader source, object target, Type type)
        {
            // Create setters for type being deserialized
            Func<ExtendedBinaryReader, object, object> settersForType;
            if (!this.setters.TryGetValue(type, out settersForType))
            {
                settersForType = this.CreateSetters(type);
                this.setters.TryAdd(type, settersForType);
            }

            // fast return for value types
            if (type.IsValueType)
            {
                return settersForType(source, null);
            }

            // is it the first time this object is present in the stream ?
            var firstTime = source.ReadBoolean();

            // what is the object id ?
            var key = source.ReadInt64();

            if (!firstTime)
            {
                return this.deserializedInstanceCache[key];
            }
            else
            {
                var destination = Activator.CreateInstance(type);

                // add instance to cache before deserializing properties, to untangle eventual cyclic dependencies
                this.deserializedInstanceCache.Add(key, destination);

                // apply setters on created object
                settersForType(source, destination);

                return destination;
            }
        }

        private static Func<object, object> CreateGetter(FieldInfo field)
        {
            var fieldType = field.DeclaringType;

            // the input parameter for the lambda
            var sourceParameter = Expression.Parameter(typeof(object), "source");

            // as the parameter is of object type, a cast or conversion may be required
            var castedSource = GetExpressionAsTypeIfNeeded(sourceParameter, fieldType);

            // get field value
            var fieldExpression = Expression.Field(castedSource, field);

            // as the return parameter is of type object, a cast or conversion may be required
            var castedField = Expression.TypeAs(fieldExpression, typeof(object));

            // the lambda expression for accessing a field on an object
            var expr = Expression.Lambda<Func<object, object>>(castedField, sourceParameter);
            return expr.Compile();
        }

        private static Expression GetExpressionAsTypeIfNeeded(Expression expression, Type targetType)
        {
            Type expressionType = expression.Type;

            // if no cast is required; the expression is returned as-is
            if (targetType.IsAssignableFrom(expressionType))
            {
                return expression;
            }

            // if the type is a value type and not nullable, a convert expression is returned
            if (targetType.IsValueType && !(targetType.IsGenericType && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))))
            {
                return Expression.Convert(expression, targetType);
            }

            // otherwise, a cast expression is returned
            return Expression.TypeAs(expression, targetType);
        }

        private Func<ExtendedBinaryReader, object, object> CreateSetters(Type type)
        {
            // the input parameters of the generated lambda : the destination instance on which the setters will be applied
            var destinationParameter = Expression.Parameter(typeof(object), "destination");

            // the BinaryReader from which to get the data
            var binaryReaderParameter = Expression.Parameter(typeof(ExtendedBinaryReader), "source");

            // a variable to hold the destination instance
            var deserializedType = Expression.Variable(type, "destination");

            var expressionBlock = new List<Expression>();

            if (!type.IsValueType)
            {
                // if the type is not a value type the instance given as a parameter is used, or a new instance is created
                var coalesce = Expression.Coalesce(GetExpressionAsTypeIfNeeded(destinationParameter, type), Expression.New(type));

                // the first "line" of the lambda is to assign the destination variable
                expressionBlock.Add(Expression.Assign(deserializedType, coalesce));
            }
            else
            {
                // for a value type, a "new" instance is created
                expressionBlock.Add(Expression.Assign(deserializedType, Expression.New(type)));
            }

            var thisAsMethodTarget = Expression.Constant(this);

            var methodToCall = typeof(FastDefaultObjectSerializer).GetMethod("DeserializeBase");
            var deserializedTypeAsObject = Expression.TypeAs(deserializedType, typeof(object));

            foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
            {
                // access to the field on the instance being deserialized
                var fieldExp = Expression.Field(deserializedType, field);

                var fieldType = Expression.Constant(field.FieldType);

                // a methood call expression
                var call = Expression.Call(
                    thisAsMethodTarget,
                    methodToCall,
                    fieldType,
                    deserializedTypeAsObject,
                    binaryReaderParameter);

                // the result of the method call is converted to the field type if needed ...
                var callResultAsFieldType = GetExpressionAsTypeIfNeeded(call, field.FieldType);

                // ... and is assigned to the field
                var assignToField = Expression.Assign(fieldExp, callResultAsFieldType);

                expressionBlock.Add(assignToField);
            }

            // the return part of the lambda
            var returnTarget = Expression.Label(typeof(object));
            var returnExpression = Expression.Return(returnTarget, deserializedTypeAsObject, typeof(object));
            var returnLabel = Expression.Label(returnTarget, deserializedTypeAsObject);

            expressionBlock.Add(returnExpression);
            expressionBlock.Add(returnLabel);

            var block = Expression.Block(new ParameterExpression[] { deserializedType }, expressionBlock);

            var lambda = Expression.Lambda<Func<ExtendedBinaryReader, object, object>>(block, binaryReaderParameter, destinationParameter);

            return lambda.Compile();
        }
    }
}