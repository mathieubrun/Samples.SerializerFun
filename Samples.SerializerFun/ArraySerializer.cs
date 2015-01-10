namespace Samples.SerializerFun
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ArraySerializer : SubSerializerBase
    {
        public ArraySerializer(RootSerializer root)
            : base(root)
        {
        }

        public override bool CanApply(Type type)
        {
            return type.IsArray;
        }

        public override void Serialize(ExtendedBinaryWriter writer, object source, Type sourceType)
        {
            var arr = source as Array;

            // get array dimensions
            var dims = new int[arr.Rank];
            writer.Write(arr.Rank);

            // get all dimension lenghts
            for (int i = 0; i < dims.Length; i++)
            {
                dims[i] = arr.GetLength(i);
                writer.Write(dims[i]);
            }

            // get all indices sets
            var indices = GetDimensionsAndLengths(dims);

            // perform cartesian product to get all possible indices
            foreach (var indice in CartesianProduct(indices))
            {
                this.SerializeBase(sourceType.GetElementType(), arr.GetValue(indice.ToArray()), writer);
            }

            return;
        }

        public override object Deserialize(ExtendedBinaryReader source, object target, Type type)
        {
            // get array dimensions
            var rank = source.ReadInt32();
            var dims = new int[rank];

            // get all dimension lenghts
            for (int i = 0; i < rank; i++)
            {
                dims[i] = source.ReadInt32();
            }

            var arr = Array.CreateInstance(type.GetElementType(), dims);

            // get all indices sets
            var indices = GetDimensionsAndLengths(dims);

            // perform cartesian product to get all possible indices
            foreach (var indice in CartesianProduct(indices))
            {
                arr.SetValue(this.DeserializeBase(type.GetElementType(), null, source), indice.ToArray());
            }

            return arr;
        }

        private static IEnumerable<IEnumerable<int>> GetDimensionsAndLengths(params int[] dimensionLenghts)
        {
            for (int i = 0; i < dimensionLenghts.Length; i++)
            {
                yield return GetIndices(dimensionLenghts[i]);
            }
        }

        private static IEnumerable<int> GetIndices(int len)
        {
            for (int j = 0; j < len; j++)
            {
                yield return j;
            }
        }

        private static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

            return sequences.Aggregate(emptyProduct, (accumulator, sequence) => from accseq in accumulator from item in sequence select accseq.Concat(new[] { item }));
        }
    }
}