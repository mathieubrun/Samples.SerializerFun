namespace Samples.SerializerFun.ReflectionBased
{
    using System;

    public class SimpleSerializer : SubSerializerBase
    {
        public SimpleSerializer(RootSerializer root)
            : base(root)
        {
        }

        public override bool CanApply(Type type)
        {
            return Type.GetTypeCode(type) != TypeCode.Object;
        }

        public override void Serialize(ExtendedBinaryWriter writer, object source, Type sourceType)
        {
            var typeCode = Type.GetTypeCode(sourceType);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    writer.Write((bool)source);
                    break;

                case TypeCode.Decimal:
                    writer.Write((decimal)source);
                    break;

                case TypeCode.Byte:
                    writer.Write((byte)source);
                    break;

                case TypeCode.SByte:
                    writer.Write((sbyte)source);
                    break;

                case TypeCode.Char:
                    writer.Write((char)source);
                    break;

                case TypeCode.DateTime:
                    writer.Write((DateTime)source);
                    break;

                case TypeCode.Double:
                    writer.Write((double)source);
                    break;

                case TypeCode.Int16:
                    writer.Write((short)source);
                    break;

                case TypeCode.Int32:
                    writer.Write((int)source);
                    break;

                case TypeCode.Int64:
                    writer.Write((long)source);
                    break;

                case TypeCode.UInt16:
                    writer.Write((ushort)source);
                    break;

                case TypeCode.UInt32:
                    writer.Write((uint)source);
                    break;

                case TypeCode.UInt64:
                    writer.Write((ulong)source);
                    break;

                case TypeCode.Single:
                    writer.Write((float)source);
                    break;

                case TypeCode.String:
                    writer.Write((string)source);
                    break;
            }
        }

        public override object Deserialize(ExtendedBinaryReader source, object target, Type type)
        {
            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return source.ReadBoolean();
                case TypeCode.Decimal:
                    return source.ReadDecimal();
                case TypeCode.Byte:
                    return source.ReadByte();
                case TypeCode.SByte:
                    return source.ReadSByte();
                case TypeCode.Char:
                    return source.ReadChar();
                case TypeCode.Double:
                    return source.ReadDouble();
                case TypeCode.Int16:
                    return source.ReadInt16();
                case TypeCode.Int32:
                    return source.ReadInt32();
                case TypeCode.Int64:
                    return source.ReadInt64();
                case TypeCode.UInt16:
                    return source.ReadUInt16();
                case TypeCode.UInt32:
                    return source.ReadUInt32();
                case TypeCode.UInt64:
                    return source.ReadUInt64();
                case TypeCode.Single:
                    return source.ReadSingle();
                case TypeCode.DateTime:
                    return source.ReadDateTime();
                case TypeCode.String:
                    return source.ReadString();
            }

            throw new ArgumentException();
        }
    }
}