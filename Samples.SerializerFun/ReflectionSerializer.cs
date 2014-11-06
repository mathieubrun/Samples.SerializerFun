namespace Samples.SerializerFun
{
    using System;
    using System.IO;
    using Samples.SerializerFun.ReflectionBased;

    public class ReflectionSerializer
    {
        private readonly RootSerializer rootSerializer;

        public ReflectionSerializer()
        {
            this.rootSerializer = new RootSerializer();

            this.rootSerializer.SubSerializers = new SubSerializerBase[]
            {
                new SimpleSerializer(this.rootSerializer),
                new ObjectSerializer(this.rootSerializer)
                {
                    SubSerializers = new SubSerializerBase[]
                    {
                        new InterfaceObjectSerializer(this.rootSerializer),
                        new NullableSerializer(this.rootSerializer),
                        new ArraySerializer(this.rootSerializer),
                        new DefaultObjectSerializer(this.rootSerializer)
                    }
                }
            };
        }

        public void Serialize<T>(Stream destinationStream, T source)
        {
            this.Serialize(destinationStream, source, typeof(T));
        }

        public void Serialize(Stream destinationStream, object source, Type type)
        {
            var writer = new ExtendedBinaryWriter(destinationStream);

            this.rootSerializer.SerializeBase(type, source, writer);

            this.rootSerializer.Done();
        }

        public T Deserialize<T>(Stream source)
        {
            return (T)this.Deserialize(source, typeof(T));
        }

        public object Deserialize(Stream source, Type type)
        {
            var reader = new ExtendedBinaryReader(source);

            var v = this.rootSerializer.DeserializeBase(type, null, reader);

            this.rootSerializer.Done();

            return v;
        }
    }
}