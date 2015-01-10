namespace Samples.SerializerFun.Lambda
{
    using System;

    public class FastInterfaceObjectSerializer : DefaultObjectSerializer
    {
        public FastInterfaceObjectSerializer(RootSerializer root)
            : base(root)
        {
        }

        public override bool CanApply(Type type)
        {
            return base.CanApply(type) && type.IsInterface;
        }

        public override void Serialize(ExtendedBinaryWriter writer, object source, Type sourceType)
        {
            var st = source.GetType();

            // write implemented type
            writer.Write(st);

            // continue with implemented type and not interface
            base.Serialize(writer, source, st);
        }

        public override object Deserialize(ExtendedBinaryReader source, object target, Type type)
        {
            var t = source.ReadType();

            return base.Deserialize(source, target, t);
        }
    }
}