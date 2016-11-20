using System.Collections.Generic;
using System.Linq;

namespace AES.ExtensionFunctions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> enumerable, int numberOfChunkElements)
        {
            IList<IEnumerable<T>> aNewEnumerable = new List<IEnumerable<T>>();
            for (int i = 0; i < enumerable.Count() / numberOfChunkElements; i++)
            {
                IEnumerable<T> afterSkip = enumerable.Skip(i * numberOfChunkElements);
                IEnumerable<T> afterTake = afterSkip.Take(numberOfChunkElements);
                aNewEnumerable.Add(afterTake);
            }

            return aNewEnumerable;
        }
    }
}
