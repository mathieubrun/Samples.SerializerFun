namespace Samples.SerializerFun.ReflectionBased
{
    using System;

    public abstract class SubSerializerBase : RootSerializer, ISubSerializer
    {
        protected SubSerializerBase(RootSerializer root)
            : base(root)
        {
        }

        public abstract bool CanApply(Type type);

        public abstract void Serialize(ExtendedBinaryWriter writer, object source, Type sourceType);

        public abstract object Deserialize(ExtendedBinaryReader source, object target, Type type);
    }
}