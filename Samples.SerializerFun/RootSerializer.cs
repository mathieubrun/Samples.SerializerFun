namespace Samples.SerializerFun
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RootSerializer
    {
        public RootSerializer()
        {
        }

        protected RootSerializer(RootSerializer root)
        {
            this.Root = root;
        }

        public virtual IEnumerable<SubSerializerBase> SubSerializers { get; set; }

        protected RootSerializer Root { get; private set; }

        public void SerializeBase(Type sourceType, object source, ExtendedBinaryWriter writer)
        {
            var serializers = this.SubSerializers ?? this.Root.SubSerializers;
            foreach (var s in serializers)
            {
                if (s.CanApply(sourceType))
                {
                    s.Serialize(writer, source, sourceType);
                    return;
                }
            }
        }

        public object DeserializeBase(Type type, object target, ExtendedBinaryReader source)
        {
            var serializers = this.SubSerializers ?? this.Root.SubSerializers;
            foreach (var s in serializers)
            {
                if (s.CanApply(type))
                {
                    return s.Deserialize(source, target, type);
                }
            }

            throw new ArgumentException();
        }

        public virtual void Done()
        {
            var serializers = this.SubSerializers ?? Enumerable.Empty<SubSerializerBase>();
            foreach (var s in serializers)
            {
                s.Done();
            }
        }
    }
}