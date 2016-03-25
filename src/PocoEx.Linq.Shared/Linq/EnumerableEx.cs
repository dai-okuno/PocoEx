using PocoEx.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoEx.Linq
{
    public static class EnumerableEx
    {
        public static IEnumerable<T> Remove<T>(this IEnumerable<T> source, T item)
            => Remove(source, item, EqualityComparer<T>.Default);

        public static IEnumerable<T> Remove<T>(this IEnumerable<T> source, T item, IEqualityComparer<T> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            return new RemoveEnumerable<T>(source)
            {
                Removing = item,
                Comparer = comparer,
            };
        }

    }
}
