namespace Samples.SerializerFun
{
    using System;

    public class ObjectSerializer : SubSerializerBase
    {
        public ObjectSerializer(RootSerializer root)
            : base(root)
        {
        }

        public override bool CanApply(Type type)
        {
            return Type.GetTypeCode(type) == TypeCode.Object;
        }

        public override void Serialize(ExtendedBinaryWriter writer, object source, Type sourceType)
        {
            // always write a boolean indicating if object is null or not
            var hasValue = source != null;
            writer.Write(hasValue);

            if (hasValue)
            {
                this.SerializeBase(sourceType, source, writer);
            }
        }

        public override object Deserialize(ExtendedBinaryReader source, object target, Type type)
        {
            var hasValue = source.ReadBoolean();

            if (hasValue)
            {
                return this.DeserializeBase(type, target, source);
            }

            return null;
        }
    }
}