using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoEx.Collections
{
    class RemoveEnumerable<T>
        : DecorativeEnumerable<T>
    {
        public RemoveEnumerable(IEnumerable<T> original) : base(original) { }

        internal IEqualityComparer<T> Comparer { get; set; }

        internal T Removing { get; set; }


        protected override DecorativeEnumerable<T>.Enumerator GetDecorativeEnumerator()
        {
            return new Enumerator()
            {
                Enumerable = this
            };
        }
        protected bool IsRemoved(T item)
            => Comparer.Equals(item, Removing);

        new class Enumerator
            : DecorativeEnumerable<T>.Enumerator
        {

            internal RemoveEnumerable<T> Enumerable { get; set; }

            public override bool MoveNext()
            {
                while (Original.MoveNext())
                {
                    if (Enumerable.IsRemoved(Original.Current)) continue;
                    return true;
                }
                return false;
            }
        }
    }
}
