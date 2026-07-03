using NUnit.Framework;
using UnityEngine;
using Unicore.Math;

namespace Unicore.Tests
{
    [TestFixture]
    public sealed class Matrix2dTests
    {
        [Test]
        public void Identity_LeavesPointAndDirectionUnchanged()
        {
            var matrix = Matrix2d.Identity;
            var point = new Vector2(2f, -3f);
            var direction = new Vector2(-1f, 4f);

            Assert.That(matrix.TransformPoint(point), Is.EqualTo(point));
            Assert.That(matrix.TransformDirection(direction), Is.EqualTo(direction));
            Assert.That(matrix.Determinant, Is.EqualTo(1f));
        }

        [Test]
        public void TRS_AppliesScaleThenRotationThenTranslation()
        {
            var matrix = Matrix2d.TRS(new Vector2(5f, -1f), Mathf.PI * 0.5f, new Vector2(2f, 3f));

            var result = matrix.TransformPoint(new Vector2(1f, 1f));

            AssertVector(result, new Vector2(2f, 1f));
        }

        [Test]
        public void Translate_OffsetsPointButNotDirection()
        {
            var matrix = Matrix2d.Translate(new Vector2(3f, -2f));

            Assert.That(matrix.TransformPoint(new Vector2(1f, 1f)), Is.EqualTo(new Vector2(4f, -1f)));
            Assert.That(matrix.TransformDirection(new Vector2(1f, 1f)), Is.EqualTo(new Vector2(1f, 1f)));
        }

        [Test]
        public void Rotate_RotatesPointAndDirectionAroundOrigin()
        {
            var matrix = Matrix2d.Rotate(Mathf.PI * 0.5f);

            AssertVector(matrix.TransformPoint(new Vector2(2f, 0f)), new Vector2(0f, 2f));
            AssertVector(matrix.TransformDirection(new Vector2(0f, 3f)), new Vector2(-3f, 0f));
        }

        [Test]
        public void Scale_ScalesPointAndDirectionPerAxis()
        {
            var matrix = Matrix2d.Scale(new Vector2(2f, -4f));

            Assert.That(matrix.TransformPoint(new Vector2(3f, -2f)), Is.EqualTo(new Vector2(6f, 8f)));
            Assert.That(matrix.TransformDirection(new Vector2(-1f, 0.5f)), Is.EqualTo(new Vector2(-2f, -2f)));
        }

        [Test]
        public void Multiply_ComposesTransformsInApplicationOrder()
        {
            var translation = Matrix2d.Translate(new Vector2(3f, 0f));
            var rotation = Matrix2d.Rotate(Mathf.PI * 0.5f);
            var combined = translation.Multiply(rotation);

            var result = combined.TransformPoint(new Vector2(1f, 0f));

            AssertVector(result, new Vector2(3f, 1f));
        }

        [Test]
        public void PositionAndBasis_ExposeMatrixColumns()
        {
            var matrix = Matrix2d.TRS(new Vector2(4f, -3f), Mathf.PI * 0.5f, new Vector2(2f, 1f));

            Assert.That(matrix.Position, Is.EqualTo(new Vector2(4f, -3f)));
            AssertVector(matrix.Right, new Vector2(0f, 2f));
            AssertVector(matrix.Up, new Vector2(-1f, 0f));
        }

        [Test]
        public void Inverse_ReversesTransform()
        {
            var matrix = Matrix2d.TRS(new Vector2(4f, -2f), 0.75f, new Vector2(2f, 0.5f));
            var point = new Vector2(-3f, 5f);
            var transformed = matrix.TransformPoint(point);

            var inverse = matrix.Inverse();
            var restored = inverse.TransformPoint(transformed);

            AssertVector(restored, point);
        }

        [Test]
        public void TryInverse_WhenMatrixSingular_ReturnsFalse()
        {
            var matrix = Matrix2d.Scale(new Vector2(0f, 2f));

            var inverted = matrix.TryInverse(out var inverse);

            Assert.That(inverted, Is.False);
            Assert.That(inverse, Is.EqualTo(default(Matrix2d)));
        }

        [Test]
        public void Inverse_WhenMatrixSingular_ThrowsInvalidOperationException()
        {
            var matrix = Matrix2d.Scale(new Vector2(0f, 1f));

            Assert.That(() => matrix.Inverse(), Throws.TypeOf<System.InvalidOperationException>());
        }

        [Test]
        public void Equality_UsesMatrixCoefficients()
        {
            var a = Matrix2d.TRS(new Vector2(1f, 2f), 0.25f, new Vector2(3f, 4f));
            var b = Matrix2d.Translate(new Vector2(1f, 2f))
                .Multiply(Matrix2d.Rotate(0.25f))
                .Multiply(Matrix2d.Scale(new Vector2(3f, 4f)));

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
            Assert.That(a != b, Is.False);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        private static void AssertVector(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        }
    }
}
