namespace Samples.SerializerFun.ReflectionBased
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    public class DefaultObjectSerializer : SubSerializerBase
    {
        private ObjectIDGenerator idGenerator = new ObjectIDGenerator();

        private Dictionary<long, object> deserializedInstanceCache = new Dictionary<long, object>();

        public DefaultObjectSerializer(RootSerializer root)
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
            bool firstTime;

            // generate unique id for object, in order not to save same object multiple times
            var key = this.idGenerator.GetId(source, out firstTime);

            writer.Write(firstTime);
            writer.Write(key);

            if (firstTime)
            {
                // inspect object
                foreach (var prop in sourceType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).OrderBy(x => x.Name))
                {
                    this.SerializeBase(prop.FieldType, prop.GetValue(source), writer);
                }
            }
        }

        public override object Deserialize(ExtendedBinaryReader source, object target, Type type)
        {
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

                // inspect object
                foreach (var prop in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).OrderBy(x => x.Name))
                {
                    var v = this.DeserializeBase(prop.FieldType, destination, source);

                    prop.SetValue(destination, v);
                }

                return destination;
            }
        }
    }
}