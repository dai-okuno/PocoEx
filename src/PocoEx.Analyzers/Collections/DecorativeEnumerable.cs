using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoEx.Collections
{
    public abstract class DecorativeEnumerable<T>
            : IEnumerable<T>
    {
        public DecorativeEnumerable(IEnumerable<T> original)
        {
            Original = original;
        }

        protected IEnumerable<T> Original { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            var result = GetDecorativeEnumerator();
            result.Original = Original.GetEnumerator();
            return result;
        }

        protected abstract Enumerator GetDecorativeEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        protected abstract class Enumerator
            : IEnumerator<T>
        {
            internal IEnumerator<T> Original { get; set; }

            public virtual T Current
                => Original.Current;

            object IEnumerator.Current
                => Current;

            public virtual void Dispose()
                => Original.Dispose();

            public abstract bool MoveNext();

            public virtual void Reset()
                => Original.Reset();

        }
    }
}
