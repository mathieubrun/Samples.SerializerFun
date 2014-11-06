namespace Samples.SerializerFun.ReflectionBased
{
    using System;

    public interface ISubSerializer
    {
        bool CanApply(Type type);

        void Serialize(ExtendedBinaryWriter writer, object source, Type sourceType);

        object Deserialize(ExtendedBinaryReader source, object target, Type type);

        void Done();
    }
}