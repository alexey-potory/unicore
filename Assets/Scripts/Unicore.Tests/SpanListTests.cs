using System;
using NUnit.Framework;
using Unicore.Collections;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class SpanListTests
    {
        [Test]
        public void Constructor_InitializesEmptyListWithProvidedCapacity()
        {
            Span<int> buffer = stackalloc int[3];
            var list = new SpanList<int>(buffer);

            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(list.Capacity, Is.EqualTo(3));
            Assert.That(list.AsSpan().Length, Is.EqualTo(0));
        }

        [Test]
        public void Add_AppendsValueAndIncrementsCount()
        {
            Span<int> buffer = stackalloc int[3];
            var list = new SpanList<int>(buffer);

            list.Add(10);
            list.Add(20);

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(10));
            Assert.That(list[1], Is.EqualTo(20));
        }

        [Test]
        public void TryAdd_WhenFull_ReturnsFalseAndDoesNotMutateList()
        {
            Span<int> buffer = stackalloc int[2];
            var list = new SpanList<int>(buffer);

            Assert.That(list.TryAdd(1), Is.True);
            Assert.That(list.TryAdd(2), Is.True);
            Assert.That(list.TryAdd(3), Is.False);

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(CopyToArray(list), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Add_WhenFull_ThrowsInvalidOperationException()
        {
            var exception = CaptureAddWhenFullException();

            Assert.That(exception, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void AddRange_AppendsItemsInOrder()
        {
            Span<int> buffer = stackalloc int[5];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 2, 3, 4 };

            list.Add(1);
            list.AddRange(values);

            Assert.That(list.Count, Is.EqualTo(4));
            Assert.That(CopyToArray(list), Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void AddRange_WhenInsufficientCapacity_ThrowsAndLeavesListUnchanged()
        {
            var exception = CaptureAddRangeOverflowException(out var values, out var count);

            Assert.That(exception, Is.TypeOf<InvalidOperationException>());
            Assert.That(count, Is.EqualTo(1));
            Assert.That(values, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            Span<int> buffer = stackalloc int[3];
            var list = new SpanList<int>(buffer);

            list.Add(5);
            list.Add(6);
            list.Clear();

            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(list.AsSpan().Length, Is.EqualTo(0));
            Assert.That(list.TryAdd(7), Is.True);
            Assert.That(list[0], Is.EqualTo(7));
        }

        [Test]
        public void Indexer_GetAndSet_UseLogicalIndex()
        {
            Span<int> buffer = stackalloc int[3];
            var list = new SpanList<int>(buffer);

            list.Add(1);
            list.Add(2);
            list[1] = 9;

            Assert.That(list[1], Is.EqualTo(9));
        }

        [Test]
        public void RemoveAt_RemovesItemAndCompactsTail()
        {
            Span<int> buffer = stackalloc int[5];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 1, 2, 3, 4 };

            list.AddRange(values);
            list.RemoveAt(1);

            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(CopyToArray(list), Is.EqualTo(new[] { 1, 3, 4 }));
        }

        [Test]
        public void RemoveLast_ReturnsLastItemAndShrinksCount()
        {
            Span<int> buffer = stackalloc int[3];
            var list = new SpanList<int>(buffer);

            list.Add(4);
            list.Add(5);

            var removed = list.RemoveLast();

            Assert.That(removed, Is.EqualTo(5));
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(4));
        }

        [Test]
        public void ContainsAndIndexOf_UseLogicalRangeOnly()
        {
            Span<int> buffer = stackalloc int[] { 1, 2, 99 };
            var list = new SpanList<int>(buffer);

            list.Add(1);
            list.Add(2);

            Assert.That(list.Contains(2), Is.True);
            Assert.That(list.IndexOf(2), Is.EqualTo(1));
            Assert.That(list.Contains(99), Is.False);
            Assert.That(list.IndexOf(99), Is.EqualTo(-1));
        }

        [Test]
        public void AsSpan_ReturnsUsedPrefix()
        {
            Span<int> buffer = stackalloc int[4];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 7, 8, 9 };

            list.AddRange(values);

            var span = list.AsSpan();

            Assert.That(span.Length, Is.EqualTo(3));
            Assert.That(span[0], Is.EqualTo(7));
            Assert.That(span[1], Is.EqualTo(8));
            Assert.That(span[2], Is.EqualTo(9));
        }

        [Test]
        public void Enumerator_IteratesUsedValuesInOrder()
        {
            Span<int> buffer = stackalloc int[4];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 3, 1, 4 };

            list.AddRange(values);

            Assert.That(CopyToArrayWithEnumerator(list), Is.EqualTo(new[] { 3, 1, 4 }));
        }

        [Test]
        public void Find_ReturnsFirstMatchingValue()
        {
            Span<int> buffer = stackalloc int[5];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 3, 7, 10, 12 };

            list.AddRange(values);

            var found = list.Find(value => value % 2 == 0);

            Assert.That(found, Is.EqualTo(10));
        }

        [Test]
        public void Find_WhenNoMatch_ReturnsDefaultValue()
        {
            Span<int> buffer = stackalloc int[3];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 1, 3, 5 };

            list.AddRange(values);

            var found = list.Find(value => value % 2 == 0);

            Assert.That(found, Is.EqualTo(default(int)));
        }

        [Test]
        public void Find_WithNullPredicate_ThrowsArgumentNullException()
        {
            var exception = CaptureFindNullPredicateException();

            Assert.That(exception!.ParamName, Is.EqualTo("match"));
        }

        [Test]
        public void FindIndex_ReturnsIndexOfFirstMatchingValue()
        {
            Span<int> buffer = stackalloc int[5];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 4, 7, 10, 13 };

            list.AddRange(values);

            Assert.That(list.FindIndex(value => value > 8), Is.EqualTo(2));
            Assert.That(list.FindIndex(value => value < 0), Is.EqualTo(-1));
        }

        [Test]
        public void TryFind_ReturnsBooleanAndFoundValue()
        {
            Span<int> buffer = stackalloc int[4];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 2, 5, 8 };

            list.AddRange(values);

            var found = list.TryFind(value => value > 4, out var match);
            var missing = list.TryFind(value => value > 10, out var missingMatch);

            Assert.That(found, Is.True);
            Assert.That(match, Is.EqualTo(5));
            Assert.That(missing, Is.False);
            Assert.That(missingMatch, Is.EqualTo(default(int)));
        }

        [Test]
        public void ToArray_ReturnsCopyOfLogicalContents()
        {
            Span<int> buffer = stackalloc int[4];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 6, 7, 8 };

            list.AddRange(values);

            var result = list.ToArray();
            result[0] = 42;

            Assert.That(result, Is.EqualTo(new[] { 42, 7, 8 }));
            Assert.That(list[0], Is.EqualTo(6));
        }

        [Test]
        public void CopyTo_CopiesContentsIntoDestinationSpan()
        {
            Span<int> buffer = stackalloc int[4];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 9, 8, 7 };
            Span<int> destination = stackalloc int[5];

            list.AddRange(values);
            list.CopyTo(destination);

            Assert.That(destination[0], Is.EqualTo(9));
            Assert.That(destination[1], Is.EqualTo(8));
            Assert.That(destination[2], Is.EqualTo(7));
        }

        [Test]
        public void CopyTo_WhenDestinationTooSmall_ThrowsArgumentException()
        {
            var exception = CaptureCopyToTooSmallException();

            Assert.That(exception, Is.Not.Null);
        }

        [Test]
        public void Remove_RemovesFirstMatchingValue()
        {
            Span<int> buffer = stackalloc int[5];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 1, 2, 2, 3 };

            list.AddRange(values);

            var removed = list.Remove(2);

            Assert.That(removed, Is.True);
            Assert.That(CopyToArray(list), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void TryRemove_ReturnsRemovedValueWhenPresent()
        {
            Span<int> buffer = stackalloc int[4];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 4, 5, 6 };

            list.AddRange(values);

            var removed = list.TryRemove(5, out var item);
            var missing = list.TryRemove(9, out var missingItem);

            Assert.That(removed, Is.True);
            Assert.That(item, Is.EqualTo(5));
            Assert.That(missing, Is.False);
            Assert.That(missingItem, Is.EqualTo(default(int)));
            Assert.That(CopyToArray(list), Is.EqualTo(new[] { 4, 6 }));
        }

        [Test]
        public void RemoveAll_RemovesMatchingValuesAndReturnsRemovedCount()
        {
            Span<int> buffer = stackalloc int[6];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 1, 2, 3, 4, 5, 6 };

            list.AddRange(values);

            var removedCount = list.RemoveAll(value => value % 2 == 0);

            Assert.That(removedCount, Is.EqualTo(3));
            Assert.That(CopyToArray(list), Is.EqualTo(new[] { 1, 3, 5 }));
        }

        [Test]
        public void Insert_InsertsValueAtSpecifiedIndex()
        {
            Span<int> buffer = stackalloc int[5];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 1, 3, 4 };

            list.AddRange(values);
            list.Insert(1, 2);

            Assert.That(CopyToArray(list), Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void TryInsert_WhenFull_ReturnsFalseAndLeavesListUnchanged()
        {
            Span<int> buffer = stackalloc int[3];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 1, 2, 3 };

            list.AddRange(values);

            var inserted = list.TryInsert(1, 9);

            Assert.That(inserted, Is.False);
            Assert.That(CopyToArray(list), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        private static Exception CaptureAddWhenFullException()
        {
            Span<int> buffer = stackalloc int[1];
            var list = new SpanList<int>(buffer);
            list.Add(1);

            try
            {
                list.Add(2);
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static ArgumentNullException CaptureFindNullPredicateException()
        {
            Span<int> buffer = stackalloc int[1];
            var list = new SpanList<int>(buffer);

            try
            {
                list.Find(null);
                return null;
            }
            catch (ArgumentNullException exception)
            {
                return exception;
            }
        }

        private static Exception CaptureAddRangeOverflowException(out int[] values, out int count)
        {
            Span<int> buffer = stackalloc int[2];
            var list = new SpanList<int>(buffer);
            Span<int> overflow = stackalloc int[] { 2, 3 };

            list.Add(1);

            try
            {
                list.AddRange(overflow);
                values = CopyToArray(list);
                count = list.Count;
                return null;
            }
            catch (Exception exception)
            {
                values = CopyToArray(list);
                count = list.Count;
                return exception;
            }
        }

        private static ArgumentException CaptureCopyToTooSmallException()
        {
            Span<int> buffer = stackalloc int[3];
            var list = new SpanList<int>(buffer);
            Span<int> values = stackalloc int[] { 1, 2, 3 };
            Span<int> destination = stackalloc int[2];

            list.AddRange(values);

            try
            {
                list.CopyTo(destination);
                return null;
            }
            catch (ArgumentException exception)
            {
                return exception;
            }
        }

        private static int[] CopyToArray(SpanList<int> list)
        {
            var result = new int[list.Count];
            var span = list.AsSpan();

            for (var i = 0; i < span.Length; i++)
            {
                result[i] = span[i];
            }

            return result;
        }

        private static int[] CopyToArrayWithEnumerator(SpanList<int> list)
        {
            var result = new int[list.Count];
            var index = 0;

            foreach (var item in list)
            {
                result[index++] = item;
            }

            return result;
        }
    }
}
