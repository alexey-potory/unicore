using NUnit.Framework;
using UnityEngine;
using Unicore.Math;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class Bounds2dTests
    {
        [Test]
        public void Constructor_WithCenterAndSize_ExposesExpectedGeometry()
        {
            var bounds = new Bounds2d(4f, 6f, 8f, 10f);

            Assert.That(bounds.Center, Is.EqualTo(new Vector2(4f, 6f)));
            Assert.That(bounds.Size, Is.EqualTo(new Vector2(8f, 10f)));
            Assert.That(bounds.Extents, Is.EqualTo(new Vector2(4f, 5f)));
            Assert.That(bounds.Min, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(bounds.Max, Is.EqualTo(new Vector2(8f, 11f)));
            Assert.That(bounds.Left, Is.EqualTo(0f));
            Assert.That(bounds.Right, Is.EqualTo(8f));
            Assert.That(bounds.Bottom, Is.EqualTo(1f));
            Assert.That(bounds.Top, Is.EqualTo(11f));
        }

        [Test]
        public void FromMinMax_CreatesBoundsFromCorners()
        {
            var bounds = Bounds2d.FromMinMax(new Vector2(-2f, 3f), new Vector2(6f, 9f));

            Assert.That(bounds.Center, Is.EqualTo(new Vector2(2f, 6f)));
            Assert.That(bounds.Size, Is.EqualTo(new Vector2(8f, 6f)));
            Assert.That(bounds.Min, Is.EqualTo(new Vector2(-2f, 3f)));
            Assert.That(bounds.Max, Is.EqualTo(new Vector2(6f, 9f)));
        }

        [Test]
        public void Contains_PointOnEdgeAndInside_ReturnsTrue()
        {
            var bounds = Bounds2d.FromMinMax(new Vector2(-2f, -1f), new Vector2(3f, 4f));

            Assert.That(bounds.Contains(new Vector2(-2f, 2f)), Is.True);
            Assert.That(bounds.Contains(new Vector2(1f, 0f)), Is.True);
            Assert.That(bounds.Contains(new Vector2(3.1f, 0f)), Is.False);
        }

        [Test]
        public void Contains_WhenOtherBoundsFullyInside_ReturnsTrue()
        {
            var outer = Bounds2d.FromMinMax(new Vector2(-5f, -5f), new Vector2(5f, 5f));
            var inner = Bounds2d.FromMinMax(new Vector2(-2f, -1f), new Vector2(3f, 4f));
            var outside = Bounds2d.FromMinMax(new Vector2(4f, 4f), new Vector2(6f, 7f));

            Assert.That(outer.Contains(inner), Is.True);
            Assert.That(outer.Contains(outside), Is.False);
        }

        [Test]
        public void EncapsulatePoint_ExpandsBoundsToIncludePoint()
        {
            var bounds = Bounds2d.FromMinMax(new Vector2(-1f, -1f), new Vector2(1f, 1f));

            var expanded = bounds.Encapsulate(new Vector2(4f, -3f));

            AssertBounds(expanded, new Vector2(-1f, -3f), new Vector2(4f, 1f));
        }

        [Test]
        public void EncapsulateBounds_ExpandsBoundsToIncludeOtherBounds()
        {
            var left = Bounds2d.FromMinMax(new Vector2(-3f, -1f), new Vector2(1f, 2f));
            var right = Bounds2d.FromMinMax(new Vector2(0f, -4f), new Vector2(5f, 3f));

            var union = left.Encapsulate(right);

            AssertBounds(union, new Vector2(-3f, -4f), new Vector2(5f, 3f));
        }

        [Test]
        public void Union_ReturnsCombinedBounds()
        {
            var a = Bounds2d.FromMinMax(new Vector2(-4f, 0f), new Vector2(-1f, 3f));
            var b = Bounds2d.FromMinMax(new Vector2(2f, -2f), new Vector2(4f, 1f));

            var union = a.Union(b);

            AssertBounds(union, new Vector2(-4f, -2f), new Vector2(4f, 3f));
        }

        [Test]
        public void Overlaps_ReturnsTrueWhenBoundsShareAreaOrEdge()
        {
            var source = Bounds2d.FromMinMax(new Vector2(0f, 0f), new Vector2(4f, 4f));
            var overlap = Bounds2d.FromMinMax(new Vector2(3f, 1f), new Vector2(6f, 5f));
            var edge = Bounds2d.FromMinMax(new Vector2(4f, -1f), new Vector2(6f, 1f));
            var separate = Bounds2d.FromMinMax(new Vector2(5f, 5f), new Vector2(7f, 7f));

            Assert.That(source.Overlaps(overlap), Is.True);
            Assert.That(source.Overlaps(edge), Is.True);
            Assert.That(source.Overlaps(separate), Is.False);
        }

        [Test]
        public void Intersection_WhenBoundsOverlap_ReturnsSharedRegion()
        {
            var a = Bounds2d.FromMinMax(new Vector2(-2f, -2f), new Vector2(3f, 4f));
            var b = Bounds2d.FromMinMax(new Vector2(1f, -4f), new Vector2(5f, 1f));

            var intersection = a.Intersection(b);

            Assert.That(intersection.HasValue, Is.True);
            AssertBounds(intersection.Value, new Vector2(1f, -2f), new Vector2(3f, 1f));
        }

        [Test]
        public void Intersection_WhenBoundsDoNotOverlap_ReturnsNull()
        {
            var a = Bounds2d.FromMinMax(new Vector2(-3f, -3f), new Vector2(-1f, -1f));
            var b = Bounds2d.FromMinMax(new Vector2(1f, 1f), new Vector2(3f, 3f));

            var intersection = a.Intersection(b);

            Assert.That(intersection.HasValue, Is.False);
        }

        [Test]
        public void ClampPoint_ReturnsNearestPointInsideBounds()
        {
            var bounds = Bounds2d.FromMinMax(new Vector2(-2f, 0f), new Vector2(3f, 4f));

            var clamped = bounds.ClampPoint(new Vector2(10f, -5f));

            Assert.That(clamped, Is.EqualTo(new Vector2(3f, 0f)));
        }

        [Test]
        public void Expand_IncreasesSizeWithoutMovingCenter()
        {
            var bounds = new Bounds2d(new Vector2(3f, -2f), new Vector2(4f, 6f));

            var expanded = bounds.Expand(new Vector2(2f, 8f));

            Assert.That(expanded.Center, Is.EqualTo(new Vector2(3f, -2f)));
            Assert.That(expanded.Size, Is.EqualTo(new Vector2(6f, 14f)));
        }

        [Test]
        public void Equality_UsesCenterAndSize()
        {
            var a = new Bounds2d(new Vector2(1f, 2f), new Vector2(3f, 4f));
            var b = Bounds2d.FromMinMax(new Vector2(-0.5f, 0f), new Vector2(2.5f, 4f));

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
            Assert.That(a != b, Is.False);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        private static void AssertBounds(Bounds2d bounds, Vector2 expectedMin, Vector2 expectedMax)
        {
            Assert.That(bounds.Min, Is.EqualTo(expectedMin));
            Assert.That(bounds.Max, Is.EqualTo(expectedMax));
        }
    }
}
