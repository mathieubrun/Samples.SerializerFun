namespace Samples.SerializerFun.ReflectionBased
{
    using System;
    using System.Linq;

    public class NullableSerializer : SubSerializerBase
    {
        public NullableSerializer(RootSerializer root)
            : base(root)
        {
        }

        public override bool CanApply(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public override void Serialize(ExtendedBinaryWriter writer, object source, Type sourceType)
        {
            this.SerializeBase(sourceType.GetGenericArguments().First(), source, writer);
        }

        public override object Deserialize(ExtendedBinaryReader source, object target, Type type)
        {
            return this.DeserializeBase(type.GetGenericArguments().First(), null, source);
        }
    }
}