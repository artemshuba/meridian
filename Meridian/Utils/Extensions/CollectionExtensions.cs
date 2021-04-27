using System.Collections.Generic;
using System.Linq;

namespace Meridian.Utils.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Splits an array into several smaller arrays.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">The array to split.</param>
        /// <param name="size">The size of the smaller arrays.</param>
        /// <returns>An array containing smaller arrays.</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> soure, int size)
        {
            for (var i = 0; i < (float)soure.Count() / size; i++)
            {
                yield return soure.Skip(i * size).Take(size);
            }
        }
    }
}
