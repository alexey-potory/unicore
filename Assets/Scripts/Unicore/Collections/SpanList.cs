using System;
using System.Collections.Generic;

namespace Unicore.Collections
{
    /// <summary>
    /// Represents mutable list over caller-provided contiguous memory without additional allocations.
    /// </summary>
    /// <typeparam name="T">Type of stored values.</typeparam>
    public ref struct SpanList<T> where T : unmanaged
    {
        private readonly Span<T> _span;
        private readonly int _capacity;

        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpanList{T}" /> struct.
        /// </summary>
        /// <param name="span">A backing storage span that holds list items.</param>
        public SpanList(Span<T> span)
        {
            _span = span;
            _capacity = span.Length;
            _count = 0;
        }

        /// <summary>
        /// Gets number of values currently stored in list.
        /// </summary>
        /// <value>A count of initialized items.</value>
        public int Count => _count;

        /// <summary>
        /// Gets maximum number of values that list can store.
        /// </summary>
        /// <value>A maximum item count supported by backing span.</value>
        public int Capacity => _capacity;

        /// <summary>
        /// Gets or sets value at specified logical index.
        /// </summary>
        /// <param name="index">A zero-based logical index.</param>
        /// <value>A value stored at <paramref name="index" />.</value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is outside used range.</exception>
        public T this[int index]
        {
            get
            {
                EnsureValidIndex(index);
                return _span[index];
            }
            set
            {
                EnsureValidIndex(index);
                _span[index] = value;
            }
        }

        /// <summary>
        /// Appends value to end of list.
        /// </summary>
        /// <param name="item">A value to append.</param>
        /// <exception cref="InvalidOperationException">Backing span does not have enough capacity.</exception>
        public void Add(T item)
        {
            if (!TryAdd(item))
            {
                throw new InvalidOperationException("SpanList does not have enough capacity.");
            }
        }

        /// <summary>
        /// Attempts to append value to end of list.
        /// </summary>
        /// <param name="item">A value to append.</param>
        /// <returns><see langword="true" /> if value was appended; otherwise, <see langword="false" />.</returns>
        public bool TryAdd(T item)
        {
            if (_count >= _capacity)
            {
                return false;
            }

            _span[_count] = item;
            _count++;
            return true;
        }

        /// <summary>
        /// Appends sequence of values to end of list.
        /// </summary>
        /// <param name="items">A span of values to append.</param>
        /// <exception cref="InvalidOperationException">Backing span does not have enough capacity.</exception>
        public void AddRange(ReadOnlySpan<T> items)
        {
            if (items.Length > _capacity - _count)
            {
                throw new InvalidOperationException("SpanList does not have enough capacity.");
            }

            items.CopyTo(_span.Slice(_count));
            _count += items.Length;
        }

        /// <summary>
        /// Removes all values from list.
        /// </summary>
        public void Clear()
        {
            _span.Slice(0, _count).Clear();
            _count = 0;
        }

        /// <summary>
        /// Determines whether list contains specified value.
        /// </summary>
        /// <param name="item">A value to locate.</param>
        /// <returns><see langword="true" /> if value exists in used range; otherwise, <see langword="false" />.</returns>
        public bool Contains(T item) => IndexOf(item) >= 0;

        /// <summary>
        /// Searches for first value that matches specified predicate.
        /// </summary>
        /// <param name="match">A predicate that defines matching condition.</param>
        /// <returns>A first matching value, or <see langword="default" /> when no match is found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is <see langword="null" />.</exception>
        public T Find(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            for (var i = 0; i < _count; i++)
            {
                var item = _span[i];
                if (match(item))
                {
                    return item;
                }
            }

            return default;
        }

        /// <summary>
        /// Searches for logical index of first value that matches specified predicate.
        /// </summary>
        /// <param name="match">A predicate that defines matching condition.</param>
        /// <returns>A zero-based logical index of first matching value, or <c>-1</c> when no match is found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is <see langword="null" />.</exception>
        public int FindIndex(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            for (var i = 0; i < _count; i++)
            {
                if (match(_span[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Attempts to find first value that matches specified predicate.
        /// </summary>
        /// <param name="match">A predicate that defines matching condition.</param>
        /// <param name="value">When this method returns, contains first matching value when match is found; otherwise, <see langword="default" />. This parameter is treated as uninitialized.</param>
        /// <returns><see langword="true" /> if matching value is found; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is <see langword="null" />.</exception>
        public bool TryFind(Predicate<T> match, out T value)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            for (var i = 0; i < _count; i++)
            {
                var item = _span[i];
                if (match(item))
                {
                    value = item;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Searches for specified value and returns its logical index.
        /// </summary>
        /// <param name="item">A value to locate.</param>
        /// <returns>A zero-based logical index of matching value, or <c>-1</c> when value is not found.</returns>
        public int IndexOf(T item)
        {
            var comparer = EqualityComparer<T>.Default;

            for (var i = 0; i < _count; i++)
            {
                if (comparer.Equals(_span[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Inserts value at specified logical index.
        /// </summary>
        /// <param name="index">A zero-based logical index at which value should be inserted.</param>
        /// <param name="item">A value to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is outside insertion range.</exception>
        /// <exception cref="InvalidOperationException">Backing span does not have enough capacity.</exception>
        public void Insert(int index, T item)
        {
            if (!TryInsert(index, item))
            {
                throw new InvalidOperationException("SpanList does not have enough capacity.");
            }
        }

        /// <summary>
        /// Attempts to insert value at specified logical index.
        /// </summary>
        /// <param name="index">A zero-based logical index at which value should be inserted.</param>
        /// <param name="item">A value to insert.</param>
        /// <returns><see langword="true" /> if value was inserted; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is outside insertion range.</exception>
        public bool TryInsert(int index, T item)
        {
            EnsureValidInsertIndex(index);

            if (_count >= _capacity)
            {
                return false;
            }

            if (index < _count)
            {
                _span.Slice(index, _count - index).CopyTo(_span.Slice(index + 1));
            }

            _span[index] = item;
            _count++;
            return true;
        }

        /// <summary>
        /// Removes value at specified logical index and preserves order of remaining values.
        /// </summary>
        /// <param name="index">A zero-based logical index of value to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is outside used range.</exception>
        public void RemoveAt(int index)
        {
            EnsureValidIndex(index);

            var sourceIndex = index + 1;
            if (sourceIndex < _count)
            {
                _span.Slice(sourceIndex, _count - sourceIndex).CopyTo(_span.Slice(index));
            }

            _count--;
            _span[_count] = default;
        }

        /// <summary>
        /// Removes first value that equals specified value.
        /// </summary>
        /// <param name="item">A value to remove.</param>
        /// <returns><see langword="true" /> if matching value was removed; otherwise, <see langword="false" />.</returns>
        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Attempts to remove first value that equals specified value.
        /// </summary>
        /// <param name="item">A value to remove.</param>
        /// <param name="removed">When this method returns, contains removed value when removal succeeds; otherwise, <see langword="default" />. This parameter is treated as uninitialized.</param>
        /// <returns><see langword="true" /> if matching value was removed; otherwise, <see langword="false" />.</returns>
        public bool TryRemove(T item, out T removed)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                removed = default;
                return false;
            }

            removed = _span[index];
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes all values that match specified predicate and preserves order of remaining values.
        /// </summary>
        /// <param name="match">A predicate that defines removal condition.</param>
        /// <returns>A number of removed values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is <see langword="null" />.</exception>
        public int RemoveAll(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var writeIndex = 0;
            for (var readIndex = 0; readIndex < _count; readIndex++)
            {
                var item = _span[readIndex];
                if (!match(item))
                {
                    _span[writeIndex] = item;
                    writeIndex++;
                }
            }

            var removedCount = _count - writeIndex;
            if (removedCount > 0)
            {
                _span.Slice(writeIndex, removedCount).Clear();
                _count = writeIndex;
            }

            return removedCount;
        }

        /// <summary>
        /// Removes and returns last value in list.
        /// </summary>
        /// <returns>A value removed from end of list.</returns>
        /// <exception cref="InvalidOperationException">List is empty.</exception>
        public T RemoveLast()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("SpanList is empty.");
            }

            var lastIndex = _count - 1;
            var item = _span[lastIndex];
            _span[lastIndex] = default;
            _count = lastIndex;
            return item;
        }

        /// <summary>
        /// Returns span over initialized prefix of backing storage.
        /// </summary>
        /// <returns>A span that covers current list contents.</returns>
        public Span<T> AsSpan() => _span.Slice(0, _count);

        /// <summary>
        /// Copies current list contents into destination span.
        /// </summary>
        /// <param name="destination">A destination span that receives current values.</param>
        /// <exception cref="ArgumentException"><paramref name="destination" /> is shorter than current list contents.</exception>
        public void CopyTo(Span<T> destination)
        {
            if (destination.Length < _count)
            {
                throw new ArgumentException("Destination span is shorter than SpanList contents.", nameof(destination));
            }

            AsSpan().CopyTo(destination);
        }

        /// <summary>
        /// Creates array copy of current list contents.
        /// </summary>
        /// <returns>An array that contains current list contents.</returns>
        public T[] ToArray()
        {
            var result = new T[_count];
            AsSpan().CopyTo(result);
            return result;
        }

        /// <summary>
        /// Returns enumerator that iterates current list contents.
        /// </summary>
        /// <returns>An enumerator over initialized values.</returns>
        public Enumerator GetEnumerator() => new Enumerator(AsSpan());

        private void EnsureValidIndex(int index)
        {
            if ((uint)index >= (uint)_count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index was outside the bounds of SpanList.");
            }
        }

        private void EnsureValidInsertIndex(int index)
        {
            if ((uint)index > (uint)_count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index was outside the insertion bounds of SpanList.");
            }
        }

        /// <summary>
        /// Enumerates values stored in <see cref="SpanList{T}" />.
        /// </summary>
        public ref struct Enumerator
        {
            private readonly Span<T> _span;
            private int _index;

            internal Enumerator(Span<T> span)
            {
                _span = span;
                _index = -1;
            }

            /// <summary>
            /// Gets value at current enumerator position.
            /// </summary>
            /// <value>A value at current position.</value>
            public T Current => _span[_index];

            /// <summary>
            /// Advances enumerator to next value.
            /// </summary>
            /// <returns><see langword="true" /> if next value is available; otherwise, <see langword="false" />.</returns>
            public bool MoveNext()
            {
                var nextIndex = _index + 1;
                if (nextIndex >= _span.Length)
                {
                    return false;
                }

                _index = nextIndex;
                return true;
            }
        }
    }
}
