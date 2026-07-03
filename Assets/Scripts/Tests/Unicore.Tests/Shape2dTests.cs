using NUnit.Framework;
using UnityEngine;
using Unicore.Math;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class Shape2dTests
    {
        [Test]
        public void Constructor_ClonesPointsAndExposesCount()
        {
            var points = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(2f, 0f),
                new Vector2(1f, 1f)
            };

            var shape = new Shape2d(points);
            points[0] = new Vector2(10f, 10f);

            Assert.That(shape.Count, Is.EqualTo(3));
            Assert.That(shape[0], Is.EqualTo(new Vector2(0f, 0f)));
            Assert.That(shape.ToArray()[0], Is.EqualTo(new Vector2(0f, 0f)));
        }

        [Test]
        public void Bounds_ReturnsMinAndMaxForConcaveShape()
        {
            var shape = CreateConcaveLShape();

            var bounds = shape.Bounds;

            Assert.That(bounds.Min, Is.EqualTo(new Vector2(0f, 0f)));
            Assert.That(bounds.Max, Is.EqualTo(new Vector2(4f, 4f)));
            Assert.That(bounds.Size, Is.EqualTo(new Vector2(4f, 4f)));
        }

        [Test]
        public void Contains_UsesEvenOddRuleForConcaveShape()
        {
            var shape = CreateConcaveLShape();

            Assert.That(shape.Contains(new Vector2(0.5f, 3.5f)), Is.True);
            Assert.That(shape.Contains(new Vector2(2f, 0.5f)), Is.True);
            Assert.That(shape.Contains(new Vector2(2f, 2f)), Is.False);
            Assert.That(shape.Contains(new Vector2(1f, 2f)), Is.True);
        }

        [Test]
        public void SignedArea_AndWinding_ReflectPointOrder()
        {
            var ccw = CreateConcaveLShape();
            var cw = ccw.ReverseWinding();

            Assert.That(ccw.SignedArea, Is.EqualTo(7f).Within(0.0001f));
            Assert.That(ccw.Area, Is.EqualTo(7f).Within(0.0001f));
            Assert.That(ccw.IsClockwise, Is.False);
            Assert.That(cw.SignedArea, Is.EqualTo(-7f).Within(0.0001f));
            Assert.That(cw.Area, Is.EqualTo(7f).Within(0.0001f));
            Assert.That(cw.IsClockwise, Is.True);
        }

        [Test]
        public void GetPerimeter_ReturnsClosedLoopLength()
        {
            var shape = CreateConcaveLShape();

            Assert.That(shape.GetPerimeter(), Is.EqualTo(16f).Within(0.0001f));
            Assert.That(shape.Perimeter, Is.EqualTo(16f).Within(0.0001f));
        }

        [Test]
        public void GetCentroid_ForConcaveShape_ReturnsPolygonCentroid()
        {
            var shape = CreateConcaveLShape();

            var centroid = shape.GetCentroid();

            Assert.That(centroid.x, Is.EqualTo(19f / 14f).Within(0.0001f));
            Assert.That(centroid.y, Is.EqualTo(19f / 14f).Within(0.0001f));
            Assert.That(shape.Centroid, Is.EqualTo(centroid));
        }

        [Test]
        public void Translate_ScaleAndTransform_ReturnExpectedPoints()
        {
            var shape = new Shape2d(new[]
            {
                new Vector2(1f, 1f),
                new Vector2(2f, 1f),
                new Vector2(1f, 3f)
            });

            var translated = shape.Translate(new Vector2(-1f, 2f));
            var scaled = shape.Scale(new Vector2(2f, -1f));
            var transformed = shape.Transform(Matrix2d.Translate(new Vector2(3f, -2f)));

            Assert.That(translated[0], Is.EqualTo(new Vector2(0f, 3f)));
            Assert.That(translated[2], Is.EqualTo(new Vector2(0f, 5f)));
            Assert.That(scaled[0], Is.EqualTo(new Vector2(2f, -1f)));
            Assert.That(scaled[2], Is.EqualTo(new Vector2(2f, -3f)));
            Assert.That(transformed[0], Is.EqualTo(new Vector2(4f, -1f)));
            Assert.That(transformed[2], Is.EqualTo(new Vector2(4f, 1f)));
        }

        [Test]
        public void DegenerateShapes_HandleEmptyPointAndSegmentCases()
        {
            var empty = new Shape2d(null);
            var point = new Shape2d(new[] { new Vector2(2f, 3f) });
            var segment = new Shape2d(new[] { new Vector2(0f, 0f), new Vector2(4f, 0f) });

            Assert.That(empty.IsEmpty, Is.True);
            Assert.That(empty.Bounds, Is.EqualTo(default(Bounds2d)));
            Assert.That(empty.Contains(Vector2.zero), Is.False);
            Assert.That(empty.GetCentroid(), Is.EqualTo(Vector2.zero));
            Assert.That(point.Contains(new Vector2(2f, 3f)), Is.True);
            Assert.That(point.Contains(new Vector2(2f, 4f)), Is.False);
            Assert.That(point.GetCentroid(), Is.EqualTo(new Vector2(2f, 3f)));
            Assert.That(segment.Contains(new Vector2(2f, 0f)), Is.True);
            Assert.That(segment.Contains(new Vector2(2f, 1f)), Is.False);
            Assert.That(segment.GetPerimeter(), Is.EqualTo(8f).Within(0.0001f));
            Assert.That(segment.GetCentroid(), Is.EqualTo(new Vector2(2f, 0f)));
        }

        private static Shape2d CreateConcaveLShape()
        {
            return new Shape2d(new[]
            {
                new Vector2(0f, 0f),
                new Vector2(4f, 0f),
                new Vector2(4f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 4f),
                new Vector2(0f, 4f)
            });
        }
    }
}
