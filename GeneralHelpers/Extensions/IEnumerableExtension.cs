using System;
using System.Collections.Generic;

namespace Extensions
{
    public static class IEnumerableExtension
    {
        //https://stackoverflow.com/questions/1300088/distinct-with-lambda
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                    yield return element;
            }
        }
        public static IEnumerable<TSource> DistinctBy<TSource>
            (this IEnumerable<TSource> source, Func<TSource, int> keySelector1, Func<TSource, int> keySelector2)
        {
            HashSet<long> knownKeys = new HashSet<long>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector1(element).CantorPair(keySelector2(element))))
                    yield return element;
            }
        }
    }
}
