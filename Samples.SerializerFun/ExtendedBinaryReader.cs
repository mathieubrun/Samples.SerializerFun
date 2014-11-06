namespace Samples.SerializerFun
{
    using System;
    using System.IO;

    public class ExtendedBinaryReader : BinaryReader
    {
        public ExtendedBinaryReader(Stream input)
            : base(input)
        {
        }

        public DateTime ReadDateTime()
        {
            return new DateTime(this.ReadInt64());
        }

        public Type ReadType()
        {
            var t = this.ReadString();
            return Type.GetType(t);
        }

        public override string ReadString()
        {
            if (this.ReadBoolean())
            {
                return base.ReadString();
            }

            return null;
        }
    }
}