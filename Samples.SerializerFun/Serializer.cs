namespace Samples.SerializerFun
{
    using System;
    using System.IO;

    public class Serializer
    {
        public static void Serialize<T>(T ob, Stream stream)
        {
            Serialize(ob, stream, typeof(T));
        }

        public static void Serialize(object ob, Stream stream, Type t)
        {
            var s = new FastReflectionSerializer();

            s.Serialize(stream, ob, t);
        }

        public static object Deserialize(Stream s, Type t)
        {
            var ser = new FastReflectionSerializer();

            var x = ser.Deserialize(s, t);

            return x;
        }

        public static T Deserialize<T>(Stream s)
        {
            return (T)Deserialize(s, typeof(T));
        }
    }
}