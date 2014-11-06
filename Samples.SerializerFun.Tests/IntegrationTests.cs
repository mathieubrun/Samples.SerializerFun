namespace Samples.SerializerFun.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IntegrationTests
    {
        public class SomeStructHolder<T>
        {
            public T theStruct;
        }

        public struct TestStruct
        {
            public string Val { get; set; }
        }

        [TestMethod]
        public void Serialize_Struct()
        {
            var original = new TestStruct
            {
                Val = "Test"
            };
            var ms = new MemoryStream();

            Serializer.Serialize(original, ms);

            ms.Position = 0;

            var read = Serializer.Deserialize<TestStruct>(ms);

            Assert.AreEqual(original.Val, read.Val);
        }

        public class TestNested<T>
        {
            public class Nested
            {
                public T Str { get; set; }
            }

            public Nested Inner { get; set; }
        }

        [TestMethod]
        public void Serialize_Nested()
        {
            var t1 = new TestNested<string>()
            {
                Inner = new TestNested<string>.Nested { Str = "Test" }
            };

            using (var ms = new MemoryStream())
            {
                var bf = new ReflectionSerializer();
                bf.Serialize(ms, t1);

                ms.Position = 0;

                var r = bf.Deserialize(ms, typeof(TestNested<string>)) as TestNested<string>;

                Assert.AreEqual(t1.Inner.Str, r.Inner.Str);
            }
        }

        public class TestReference
        {
            public TestReference Reference { get; set; }

            public string Str { get; set; }
        }

        [TestMethod]
        public void Serialize_Cyclic_Reference()
        {
            var t1 = new TestReference() { Str = "1" };
            var t2 = new TestReference() { Str = "2" };

            t1.Reference = t2;
            t2.Reference = t1;

            using (var ms = new MemoryStream())
            {
                var bf = new ReflectionSerializer();
                bf.Serialize(ms, t1);

                ms.Position = 0;

                var r = bf.Deserialize(ms, typeof(TestReference)) as TestReference;

                Assert.ReferenceEquals(r, r.Reference.Reference);
                Assert.ReferenceEquals(r.Reference, r.Reference.Reference.Reference);
            }
        }

        [TestMethod]
        public void Serialize_Same_Reference_Array()
        {
            var instance = new TestReference() { Str = "3" };
            var array = new TestReference[] { instance, instance, instance, instance };

            using (var ms = new MemoryStream())
            {
                var bf = new ReflectionSerializer();
                bf.Serialize(ms, array);

                ms.Position = 0;

                var r = bf.Deserialize(ms, typeof(TestReference[])) as TestReference[];

                Assert.ReferenceEquals(array[0], array[1]);
                Assert.ReferenceEquals(array[1], array[2]);
                Assert.ReferenceEquals(array[2], array[3]);
            }
        }

        public interface ITestInterface
        {
            ITestInterface Inner { get; set; }

            int IntValue { get; set; }
        }

        public class TestInterface : ITestInterface
        {
            public ITestInterface Inner { get; set; }

            public int IntValue { get; set; }
        }

        [TestMethod]
        public void Serialize_Interface()
        {
            var original = new TestInterface
            {
                IntValue = int.MaxValue,
                Inner = new TestInterface { IntValue = int.MinValue }
            };

            var ms = new MemoryStream();

            Serializer.Serialize(original, ms);

            ms.Position = 0;

            var read = Serializer.Deserialize<TestInterface>(ms);

            Assert.AreEqual(original.IntValue, read.IntValue);
            Assert.AreEqual(original.Inner.IntValue, read.Inner.IntValue);
        }

        [TestMethod]
        public void Serialize_Dictionary()
        {
            var original = new Dictionary<int, string>{
                        {11111, "11111"},
            };

            var ms = new MemoryStream();

            Serializer.Serialize(original, ms);

            ms.Position = 0;

            var read = Serializer.Deserialize<Dictionary<int, string>>(ms);

            foreach (var v in read)
            {
                Assert.AreEqual(v.Key, 11111);
                Assert.AreEqual(v.Value, "11111");
            }
        }

        public class TestList
        {
            public List<TestList> InnerList { get; set; }

            public int IntValue { get; set; }
        }

        [TestMethod]
        public void Serialize_List()
        {
            var original = new TestList
            {
                IntValue = int.MinValue,
                InnerList =
                    new List<TestList>
                    {
                        new TestList
                        {
                            IntValue = int.MinValue
                        },
                        new TestList
                        {
                            IntValue = 0
                        },
                        new TestList
                        {
                            IntValue = int.MaxValue
                        },
                    }
            };

            var ms = new MemoryStream();

            Serializer.Serialize(original, ms);

            ms.Position = 0;

            var read = Serializer.Deserialize<TestList>(ms);

            Assert.AreEqual(original.IntValue, read.IntValue);
            Assert.AreEqual(original.InnerList[0].IntValue, read.InnerList[0].IntValue);
            Assert.AreEqual(original.InnerList[1].IntValue, read.InnerList[1].IntValue);
            Assert.AreEqual(original.InnerList[2].IntValue, read.InnerList[2].IntValue);
        }

        public class TestMultiDimensionnalArray
        {
            public TestMultiDimensionnalArray[, ,] InnerArray { get; set; }

            public int IntValue { get; set; }
        }

        [TestMethod]
        public void Serialize_Multidimensionnal_Array()
        {
            var original = new TestMultiDimensionnalArray
            {
                IntValue = int.MinValue,
                InnerArray =
                new TestMultiDimensionnalArray[,,]
                {
                    {
                        {
                            new TestMultiDimensionnalArray
                            {
                                IntValue = int.MinValue
                            },
                            null,
                            null
                        }
                    },
                    {
                        {
                            null,
                            new TestMultiDimensionnalArray
                            {
                                IntValue = 0
                            },
                            null
                        }
                    },
                    {
                        {
                            null,
                            null,
                            new TestMultiDimensionnalArray
                            {
                                IntValue = int.MaxValue
                            },
                        }
                    },
                }
            };

            var ms = new MemoryStream();

            Serializer.Serialize(original, ms);

            ms.Position = 0;

            var read = Serializer.Deserialize<TestMultiDimensionnalArray>(ms);

            Assert.AreEqual(original.IntValue, read.IntValue);
            Assert.AreEqual(original.InnerArray[0, 0, 0].IntValue, read.InnerArray[0, 0, 0].IntValue);
            Assert.AreEqual(original.InnerArray[1, 0, 1].IntValue, read.InnerArray[1, 0, 1].IntValue);
            Assert.AreEqual(original.InnerArray[2, 0, 2].IntValue, read.InnerArray[2, 0, 2].IntValue);
        }

        public class TestArray
        {
            public TestArray[] InnerArray { get; set; }

            public int IntValue { get; set; }
        }

        [TestMethod]
        public void Serialize_Array()
        {
            var original = new TestArray
            {
                IntValue = int.MinValue,
                InnerArray =
                new TestArray[]
                {
                    new TestArray
                    {
                        IntValue = int.MinValue
                    },
                    new TestArray
                    {
                        IntValue = 0
                    },
                    new TestArray
                    {
                        IntValue = int.MaxValue
                    },
                }
            };

            var ms = new MemoryStream();

            Serializer.Serialize(original, ms);

            ms.Position = 0;

            var read = Serializer.Deserialize<TestArray>(ms);

            Assert.AreEqual(original.IntValue, read.IntValue);
            Assert.AreEqual(original.InnerArray[0].IntValue, read.InnerArray[0].IntValue);
            Assert.AreEqual(original.InnerArray[1].IntValue, read.InnerArray[1].IntValue);
            Assert.AreEqual(original.InnerArray[2].IntValue, read.InnerArray[2].IntValue);
        }

        public class TestArrayOfArray
        {
            public TestArrayOfArray[][] InnerArray { get; set; }

            public int IntValue { get; set; }
        }

        [TestMethod]
        public void Serialize_Array_Of_Array()
        {
            var original = new TestArrayOfArray
            {
                IntValue = int.MinValue,
                InnerArray =
                new TestArrayOfArray[][]
                {
                    new TestArrayOfArray[]
                    {
                        new TestArrayOfArray
                        {
                            IntValue = int.MinValue
                        },
                        new TestArrayOfArray
                        {
                            IntValue = 0
                        },
                    },
                    new TestArrayOfArray[]
                    {
                        new TestArrayOfArray
                        {
                            IntValue = int.MaxValue
                        },
                    }
                }
            };

            var ms = new MemoryStream();

            Serializer.Serialize(original, ms);

            ms.Position = 0;

            var read = Serializer.Deserialize<TestArrayOfArray>(ms);

            Assert.AreEqual(original.IntValue, read.IntValue);
            Assert.AreEqual(original.InnerArray[0][0].IntValue, read.InnerArray[0][0].IntValue);
            Assert.AreEqual(original.InnerArray[0][1].IntValue, read.InnerArray[0][1].IntValue);
            Assert.AreEqual(original.InnerArray[1][0].IntValue, read.InnerArray[1][0].IntValue);
        }

        public class SimpleObject
        {
            public double DoubleValue { get; set; }

            public float? FloatValue { get; set; }

            public SimpleObject Inner { get; set; }

            public int IntValue { get; set; }

            public string StringValue { get; set; }
        }

        [TestMethod]
        public void Serialize_Simple_Object()
        {
            var original = new SimpleObject
            {
                IntValue = int.MinValue,
                DoubleValue = double.MinValue,
                FloatValue = float.MinValue,
                StringValue = "1234567890",
                Inner = new SimpleObject
                {
                    IntValue = int.MaxValue,
                    DoubleValue = double.MaxValue,
                    FloatValue = null,
                    StringValue = "0987654321",
                }
            };

            var ms = new MemoryStream();

            Serializer.Serialize(original, ms);

            ms.Position = 0;

            var read = Serializer.Deserialize<SimpleObject>(ms);

            Assert.AreEqual(original.IntValue, read.IntValue);
            Assert.AreEqual(original.DoubleValue, read.DoubleValue);
            Assert.AreEqual(original.FloatValue, read.FloatValue);
            Assert.AreEqual(original.StringValue, read.StringValue);

            Assert.AreEqual(original.Inner.IntValue, read.Inner.IntValue);
            Assert.AreEqual(original.Inner.DoubleValue, read.Inner.DoubleValue);
            Assert.AreEqual(original.Inner.FloatValue, read.Inner.FloatValue);
            Assert.AreEqual(original.Inner.StringValue, read.Inner.StringValue);
        }
    }
}