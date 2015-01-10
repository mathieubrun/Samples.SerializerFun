namespace Samples.SerializerFun.Tests.Reflection
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ReflectionSerializerTests
    {
        [TestMethod]
        public void Serialize_Deserialize_Simple_Types()
        {
            this.Serialize<bool>(true);
            this.Serialize<bool?>(false);
            this.Serialize<bool?>(null);

            this.Serialize<decimal>(decimal.MinValue);
            this.Serialize<decimal?>(decimal.MaxValue);
            this.Serialize<decimal?>(null);

            this.Serialize<double>(double.MinValue);
            this.Serialize<double?>(double.MaxValue);
            this.Serialize<double?>(null);

            this.Serialize<ushort>(ushort.MinValue);
            this.Serialize<ushort?>(ushort.MaxValue);
            this.Serialize<ushort?>(null);

            this.Serialize<uint>(uint.MinValue);
            this.Serialize<uint?>(uint.MaxValue);
            this.Serialize<uint?>(null);

            this.Serialize<ulong>(ulong.MinValue);
            this.Serialize<ulong?>(ulong.MaxValue);
            this.Serialize<ulong?>(null);

            this.Serialize<short>(short.MinValue);
            this.Serialize<short?>(short.MaxValue);
            this.Serialize<short?>(null);

            this.Serialize<int>(int.MinValue);
            this.Serialize<int?>(int.MaxValue);
            this.Serialize<int?>(null);

            this.Serialize<long>(long.MinValue);
            this.Serialize<long?>(long.MaxValue);
            this.Serialize<long?>(null);

            this.Serialize<char>(char.MinValue);
            this.Serialize<char?>(char.MaxValue);
            this.Serialize<char?>(null);

            this.Serialize<byte>(byte.MinValue);
            this.Serialize<byte?>(byte.MaxValue);
            this.Serialize<byte?>(null);

            this.Serialize<sbyte>(sbyte.MinValue);
            this.Serialize<sbyte?>(sbyte.MaxValue);
            this.Serialize<sbyte?>(null);

            this.Serialize<float>(float.MinValue);
            this.Serialize<float?>(float.MaxValue);
            this.Serialize<float?>(null);

            this.Serialize<DateTime>(DateTime.MinValue);
            this.Serialize<DateTime?>(DateTime.MaxValue);
            this.Serialize<DateTime?>(null);

            this.Serialize<string>("123456");
            this.Serialize<string>("");
            this.Serialize<string>(null);

            this.Serialize(new string[] { "123456" });
            this.Serialize(new string[] { });
            this.Serialize(null);
        }

        private void Serialize<T>(T value)
        {
            T val = value;

            using (var ms = new MemoryStream())
            {
                var serializer = new ReflectionSerializer();
                serializer.Serialize(ms, val);

                ms.Position = 0;

                var read = serializer.Deserialize<T>(ms);
                Assert.AreEqual(val, read);
            }
        }

        private void Serialize(string[] value)
        {
            var val = value;

            using (var ms = new MemoryStream())
            {
                var serializer = new ReflectionSerializer();
                serializer.Serialize(ms, val);

                ms.Position = 0;

                var read = serializer.Deserialize<string[]>(ms);

                if (value != null)
                {
                    for (int i = 0; i < val.Length; i++)
                    {
                        Assert.AreEqual(val.GetValue(i), read.GetValue(i));
                    }
                }
            }
        }
    }
}