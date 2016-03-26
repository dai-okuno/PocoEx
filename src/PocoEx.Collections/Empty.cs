using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PocoEx.Collections
{
    public sealed class Empty
    {
        public static T[] Array<T>()
            => EmptyList<T>.Array;

        public static ICollection<T> Collection<T>()
            => EmptyList<T>.Array;

        public static IDictionary<TKey, TValue> Dictionary<TKey, TValue>()
            => EmptyDictionary<TKey, TValue>.Instance;

        public static IEnumerable<T> Enumerable<T>()
            => EmptyList<T>.Array;

        public static IList<T> List<T>()
            => EmptyList<T>.Array;

        public static IReadOnlyCollection<T> ReadOnlyCollection<T>()
            => EmptyList<T>.Array;

        public static IReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary<TKey, TValue>()
            => EmptyDictionary<TKey, TValue>.Instance;

        public static IReadOnlyList<T> ReadOnlyList<T>()
            => EmptyList<T>.Array;

        public static ISet<T> Set<T>()
            => EmptySet<T>.Instance;

        private sealed class EmptyList<T>
        {
            public static readonly T[] Array = new T[0];
        }

        class EmptyEnumerable<T>
            : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
                => Enumerator.Instance;

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            private sealed class Enumerator
                : IEnumerator<T>
            {

                public static readonly Enumerator Instance = new Enumerator();

                public T Current
                    => default(T);

                object IEnumerator.Current
                    => Current;

                public void Dispose()
                { }

                public bool MoveNext()
                    => false;

                void System.Collections.IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }
            }
        }

        sealed class EmptyDictionary<TKey, TValue>
            : EmptyEnumerable<KeyValuePair<TKey, TValue>>
            , IReadOnlyDictionary<TKey, TValue>
            , IDictionary<TKey, TValue>
        {
            public static readonly EmptyDictionary<TKey, TValue> Instance = new EmptyDictionary<TKey, TValue>();

            private EmptyDictionary()
            { }

            public TValue this[TKey key]
            {
                get { throw new KeyNotFoundException(); }
                set { throw new NotSupportedException(); }
            }

            public int Count
                => 0;

            public bool IsReadOnly
                => true;

            ICollection<TKey> IDictionary<TKey, TValue>.Keys
                => List<TKey>();

            ICollection<TValue> IDictionary<TKey, TValue>.Values
                => List<TValue>();

            public IEnumerable<TKey> Keys
                => Enumerable<TKey>();

            public IEnumerable<TValue> Values
                => Enumerable<TValue>();

            void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
            {
                throw new NotSupportedException();
            }

            void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            { }

            public bool Contains(KeyValuePair<TKey, TValue> item)
                => false;

            public bool ContainsKey(TKey key)
                => false;

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                if (array == null) throw new ArgumentNullException(nameof(array));
                if (arrayIndex < 0 || array.Length <= arrayIndex) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
                => false;

            bool IDictionary<TKey, TValue>.Remove(TKey key)
               => false;

            public bool TryGetValue(TKey key, out TValue value)
            {
                value = default(TValue);
                return false;
            }
        }

        sealed class EmptySet<T>
            : EmptyEnumerable<T>
            , ISet<T>
        {

            public static readonly EmptySet<T> Instance = new EmptySet<T>();

            private EmptySet()
            { }

            public int Count
                => 0;

            public bool IsReadOnly
                => true;

            bool ISet<T>.Add(T item)
            {
                throw new NotSupportedException();
            }

            void ICollection<T>.Clear()
            { }

            public bool Contains(T item)
                => false;

            public void CopyTo(T[] array, int arrayIndex)
            {
                if (array == null) throw new ArgumentNullException(nameof(array));
                if (arrayIndex < 0 || array.Length <= arrayIndex) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            public void ExceptWith(IEnumerable<T> other)
            {
                if (other == null) throw new ArgumentNullException(nameof(other));
            }

            public void IntersectWith(IEnumerable<T> other)
            {
                if (other == null) throw new ArgumentNullException(nameof(other));
            }

            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                if (other == null) throw new ArgumentNullException(nameof(other));
                return true;
            }

            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                if (other == null) throw new ArgumentNullException(nameof(other));
                return false;
            }

            public bool IsSubsetOf(IEnumerable<T> other)
            {
                if (other == null) throw new ArgumentNullException(nameof(other));
                return true;
            }

            public bool IsSupersetOf(IEnumerable<T> other)
            {
                if (other == null) throw new ArgumentNullException(nameof(other));
                return false;
            }

            public bool Overlaps(IEnumerable<T> other)
            {
                if (other == null) throw new ArgumentNullException(nameof(other));
                return false;
            }

            public bool Remove(T item)
                => false;

            public bool SetEquals(IEnumerable<T> other)
            {
                if (other == null) throw new ArgumentNullException(nameof(other));
                return !other.Any();
            }

            void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            void ISet<T>.UnionWith(IEnumerable<T> other)
            {
                throw new NotSupportedException();
            }

            void ICollection<T>.Add(T item)
            {
                throw new NotSupportedException();
            }

        }

    }
}
