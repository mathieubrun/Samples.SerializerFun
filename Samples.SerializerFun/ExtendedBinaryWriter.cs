namespace Samples.SerializerFun
{
    using System;
    using System.IO;

    public class ExtendedBinaryWriter : BinaryWriter
    {
        public ExtendedBinaryWriter(Stream s)
            : base(s)
        {
        }

        public void Write(DateTime value)
        {
            this.Write(value.Ticks);
        }

        public void Write(Type t)
        {
            this.Write(t.FullName + ", " + t.Assembly.FullName);
        }

        public override void Write(string value)
        {
            var hasValue = value != null;

            base.Write(hasValue);

            if (hasValue)
            {
                base.Write(value);
            }
        }
    }
}