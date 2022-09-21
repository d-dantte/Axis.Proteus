using System.Collections.Generic;

namespace Axis.Proteus
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<(T1 first, T2 second)> PairWith<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
        {
            using var enumerator1 = first.GetEnumerator();
            using var enumerator2 = second.GetEnumerator();
            while(enumerator1.MoveNext() && enumerator2.MoveNext())
                yield return (enumerator1.Current, enumerator2.Current);
        }
    }
}
