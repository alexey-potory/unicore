using NUnit.Framework;
using Unicore.Math;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class RangeTests
    {
        [Test]
        public void Constructor_AutoNormalizesMinAndMax()
        {
            var range = new Range(5f, 1f);

            Assert.That(range.Min, Is.EqualTo(1f));
            Assert.That(range.Max, Is.EqualTo(5f));
            Assert.That(range.Length, Is.EqualTo(4f));
            Assert.That(range.Center, Is.EqualTo(3f));
        }

        [Test]
        public void IsPoint_WhenMinEqualsMax_ReturnsTrue()
        {
            var range = new Range(2f, 2f);

            Assert.That(range.IsPoint, Is.True);
            Assert.That(range.Length, Is.EqualTo(0f));
        }

        [Test]
        public void Contains_WithValue_UsesInclusiveBounds()
        {
            var range = new Range(-2f, 3f);

            Assert.That(range.Contains(-2f), Is.True);
            Assert.That(range.Contains(0f), Is.True);
            Assert.That(range.Contains(3f), Is.True);
            Assert.That(range.Contains(3.1f), Is.False);
        }

        [Test]
        public void Contains_WithRange_ReturnsTrueOnlyWhenFullyInside()
        {
            var outer = new Range(-5f, 5f);
            var inner = new Range(-1f, 2f);
            var outside = new Range(4f, 6f);

            Assert.That(outer.Contains(inner), Is.True);
            Assert.That(outer.Contains(outside), Is.False);
        }

        [Test]
        public void Overlaps_WhenRangesShareEdge_ReturnsTrue()
        {
            var left = new Range(-3f, 1f);
            var edge = new Range(1f, 4f);
            var separate = new Range(1.1f, 5f);

            Assert.That(left.Overlaps(edge), Is.True);
            Assert.That(left.Overlaps(separate), Is.False);
        }

        [Test]
        public void Clamp_ReturnsNearestValueInsideRange()
        {
            var range = new Range(2f, 6f);

            Assert.That(range.Clamp(-1f), Is.EqualTo(2f));
            Assert.That(range.Clamp(4f), Is.EqualTo(4f));
            Assert.That(range.Clamp(9f), Is.EqualTo(6f));
        }

        [Test]
        public void Lerp_MapsTIntoRangeWithoutClamping()
        {
            var range = new Range(10f, 20f);

            Assert.That(range.Lerp(0f), Is.EqualTo(10f));
            Assert.That(range.Lerp(0.5f), Is.EqualTo(15f));
            Assert.That(range.Lerp(1.5f), Is.EqualTo(25f));
        }

        [Test]
        public void InverseLerp_MapsValueIntoNormalizedPosition()
        {
            var range = new Range(10f, 20f);

            Assert.That(range.InverseLerp(10f), Is.EqualTo(0f));
            Assert.That(range.InverseLerp(15f), Is.EqualTo(0.5f));
            Assert.That(range.InverseLerp(25f), Is.EqualTo(1.5f));
        }

        [Test]
        public void InverseLerp_WhenPointRange_ReturnsZero()
        {
            var range = new Range(5f, 5f);

            Assert.That(range.InverseLerp(5f), Is.EqualTo(0f));
            Assert.That(range.InverseLerp(8f), Is.EqualTo(0f));
        }

        [Test]
        public void Expand_GrowsRangeOnBothSides()
        {
            var range = new Range(3f, 7f);

            var expanded = range.Expand(2f);

            Assert.That(expanded.Min, Is.EqualTo(1f));
            Assert.That(expanded.Max, Is.EqualTo(9f));
        }

        [Test]
        public void Expand_WhenShrunkPastCenter_ClampsToPointRange()
        {
            var range = new Range(2f, 6f);

            var shrunk = range.Expand(-3f);

            Assert.That(shrunk.Min, Is.EqualTo(4f));
            Assert.That(shrunk.Max, Is.EqualTo(4f));
            Assert.That(shrunk.IsPoint, Is.True);
        }

        [Test]
        public void Union_ReturnsSmallestRangeContainingBothRanges()
        {
            var left = new Range(-4f, -1f);
            var right = new Range(2f, 5f);

            var union = left.Union(right);

            Assert.That(union.Min, Is.EqualTo(-4f));
            Assert.That(union.Max, Is.EqualTo(5f));
        }

        [Test]
        public void Intersection_WhenRangesOverlap_ReturnsSharedSegment()
        {
            var a = new Range(-2f, 4f);
            var b = new Range(1f, 7f);

            var intersection = a.Intersection(b);

            Assert.That(intersection.HasValue, Is.True);
            Assert.That(intersection.Value.Min, Is.EqualTo(1f));
            Assert.That(intersection.Value.Max, Is.EqualTo(4f));
        }

        [Test]
        public void Intersection_WhenRangesDoNotOverlap_ReturnsNull()
        {
            var a = new Range(-3f, -1f);
            var b = new Range(1f, 3f);

            var intersection = a.Intersection(b);

            Assert.That(intersection.HasValue, Is.False);
        }

        [Test]
        public void Equality_UsesNormalizedBounds()
        {
            var a = new Range(1f, 5f);
            var b = new Range(5f, 1f);

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
            Assert.That(a != b, Is.False);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }
    }
}
