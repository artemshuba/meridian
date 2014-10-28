using System;
using System.Collections;
using System.Collections.Generic;

namespace Neptune.Extensions
{
    public static class CommonExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (action == null)
                throw new ArgumentNullException("action");

            foreach (var item in source)
            {
                action(item);
            }
        }

        public static bool IsNullOrEmpty(this IList source)
        {
            return source == null || source.Count == 0;
        }
    }
}
