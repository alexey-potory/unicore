using System;
using UnityEngine;

namespace Unicore.Math
{
    /// <summary>
    /// Represents 2D affine transform matrix with 2x3 coefficients.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public readonly struct Matrix2d : IEquatable<Matrix2d>
    {
        private readonly float _m00;
        private readonly float _m01;
        private readonly float _m02;
        private readonly float _m10;
        private readonly float _m11;
        private readonly float _m12;

        private Matrix2d(float m00, float m01, float m02, float m10, float m11, float m12)
        {
            _m00 = m00;
            _m01 = m01;
            _m02 = m02;
            _m10 = m10;
            _m11 = m11;
            _m12 = m12;
        }

        /// <summary>
        /// Gets identity transform.
        /// </summary>
        public static Matrix2d Identity => new Matrix2d(1f, 0f, 0f, 0f, 1f, 0f);

        /// <summary>
        /// Gets matrix determinant of linear 2x2 part.
        /// </summary>
        public float Determinant => _m00 * _m11 - _m01 * _m10;

        /// <summary>
        /// Gets translation component.
        /// </summary>
        public Vector2 Position => new Vector2(_m02, _m12);

        /// <summary>
        /// Gets transformed X basis vector.
        /// </summary>
        public Vector2 Right => new Vector2(_m00, _m10);

        /// <summary>
        /// Gets transformed Y basis vector.
        /// </summary>
        public Vector2 Up => new Vector2(_m01, _m11);

        /// <summary>
        /// Creates transform from translation, rotation in radians, and scale.
        /// </summary>
        /// <param name="position">Translation component.</param>
        /// <param name="rotationRadians">Rotation angle in radians.</param>
        /// <param name="scale">Per-axis scale.</param>
        /// <returns>Affine transform that applies scale, then rotation, then translation.</returns>
        public static Matrix2d TRS(Vector2 position, float rotationRadians, Vector2 scale)
        {
            var cosine = Mathf.Cos(rotationRadians);
            var sine = Mathf.Sin(rotationRadians);

            return new Matrix2d(
                cosine * scale.x,
                -sine * scale.y,
                position.x,
                sine * scale.x,
                cosine * scale.y,
                position.y);
        }

        /// <summary>
        /// Creates translation matrix.
        /// </summary>
        /// <param name="offset">Translation offset.</param>
        /// <returns>Translation transform.</returns>
        public static Matrix2d Translate(Vector2 offset) => new Matrix2d(1f, 0f, offset.x, 0f, 1f, offset.y);

        /// <summary>
        /// Creates rotation matrix around origin.
        /// </summary>
        /// <param name="radians">Rotation angle in radians.</param>
        /// <returns>Rotation transform.</returns>
        public static Matrix2d Rotate(float radians)
        {
            var cosine = Mathf.Cos(radians);
            var sine = Mathf.Sin(radians);

            return new Matrix2d(cosine, -sine, 0f, sine, cosine, 0f);
        }

        /// <summary>
        /// Creates scale matrix.
        /// </summary>
        /// <param name="scale">Per-axis scale.</param>
        /// <returns>Scale transform.</returns>
        public static Matrix2d Scale(Vector2 scale) => new Matrix2d(scale.x, 0f, 0f, 0f, scale.y, 0f);

        /// <summary>
        /// Returns matrix composition with another transform.
        /// </summary>
        /// <param name="other">Transform applied after current transform.</param>
        /// <returns>Composed transform.</returns>
        public Matrix2d Multiply(Matrix2d other)
        {
            return new Matrix2d(
                _m00 * other._m00 + _m01 * other._m10,
                _m00 * other._m01 + _m01 * other._m11,
                _m00 * other._m02 + _m01 * other._m12 + _m02,
                _m10 * other._m00 + _m11 * other._m10,
                _m10 * other._m01 + _m11 * other._m11,
                _m10 * other._m02 + _m11 * other._m12 + _m12);
        }

        /// <summary>
        /// Transforms point including translation.
        /// </summary>
        /// <param name="point">Point to transform.</param>
        /// <returns>Transformed point.</returns>
        public Vector2 TransformPoint(Vector2 point)
        {
            return new Vector2(
                _m00 * point.x + _m01 * point.y + _m02,
                _m10 * point.x + _m11 * point.y + _m12);
        }

        /// <summary>
        /// Transforms direction ignoring translation.
        /// </summary>
        /// <param name="direction">Direction to transform.</param>
        /// <returns>Transformed direction.</returns>
        public Vector2 TransformDirection(Vector2 direction)
        {
            return new Vector2(
                _m00 * direction.x + _m01 * direction.y,
                _m10 * direction.x + _m11 * direction.y);
        }

        /// <summary>
        /// Returns inverse transform.
        /// </summary>
        /// <returns>Inverse transform.</returns>
        /// <exception cref="InvalidOperationException">Matrix is singular and cannot be inverted.</exception>
        public Matrix2d Inverse()
        {
            if (!TryInverse(out var inverse))
            {
                throw new InvalidOperationException("Matrix2d is singular and cannot be inverted.");
            }

            return inverse;
        }

        /// <summary>
        /// Attempts to invert matrix.
        /// </summary>
        /// <param name="inverse">Receives inverse matrix when inversion succeeds.</param>
        /// <returns><see langword="true" /> if inversion succeeds; otherwise, <see langword="false" />.</returns>
        public bool TryInverse(out Matrix2d inverse)
        {
            var determinant = Determinant;
            if (Mathf.Approximately(determinant, 0f))
            {
                inverse = default;
                return false;
            }

            var inverseDeterminant = 1f / determinant;
            var m00 = _m11 * inverseDeterminant;
            var m01 = -_m01 * inverseDeterminant;
            var m10 = -_m10 * inverseDeterminant;
            var m11 = _m00 * inverseDeterminant;
            var m02 = -m00 * _m02 - m01 * _m12;
            var m12 = -m10 * _m02 - m11 * _m12;

            inverse = new Matrix2d(m00, m01, m02, m10, m11, m12);
            return true;
        }

        /// <inheritdoc />
        public bool Equals(Matrix2d other)
        {
            return _m00.Equals(other._m00) &&
                   _m01.Equals(other._m01) &&
                   _m02.Equals(other._m02) &&
                   _m10.Equals(other._m10) &&
                   _m11.Equals(other._m11) &&
                   _m12.Equals(other._m12);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Matrix2d other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(_m00, _m01, _m02, _m10, _m11, _m12);

        /// <summary>
        /// Returns formatted matrix string.
        /// </summary>
        public override string ToString() => $"[{_m00}, {_m01}, {_m02}; {_m10}, {_m11}, {_m12}]";

        public static bool operator ==(Matrix2d left, Matrix2d right) => left.Equals(right);

        public static bool operator !=(Matrix2d left, Matrix2d right) => !left.Equals(right);
    }
}
