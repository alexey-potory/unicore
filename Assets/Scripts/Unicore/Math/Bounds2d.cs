using System;
using UnityEngine;

namespace Unicore.Math
{
    // ReSharper disable once InconsistentNaming
    public readonly struct Bounds2d : IEquatable<Bounds2d>
    {
        private readonly float _centerX;
        private readonly float _centerY;
        private readonly float _width;
        private readonly float _height;

        /// <summary>
        /// Initializes bounds from center coordinates and size.
        /// </summary>
        /// <param name="x">X coordinate of bounds center.</param>
        /// <param name="y">Y coordinate of bounds center.</param>
        /// <param name="width">Bounds width.</param>
        /// <param name="height">Bounds height.</param>
        public Bounds2d(float x, float y, float width, float height)
        {
            _centerX = x;
            _centerY = y;
            _width = Mathf.Abs(width);
            _height = Mathf.Abs(height);
        }

        /// <summary>
        /// Initializes bounds from center point and size.
        /// </summary>
        /// <param name="center">Bounds center.</param>
        /// <param name="size">Bounds size.</param>
        public Bounds2d(Vector2 center, Vector2 size) : this(center.x, center.y, size.x, size.y)
        {
        }

        /// <summary>
        /// Gets X coordinate of bounds center.
        /// </summary>
        public float X => _centerX;

        /// <summary>
        /// Gets Y coordinate of bounds center.
        /// </summary>
        public float Y => _centerY;

        /// <summary>
        /// Gets bounds width.
        /// </summary>
        public float Width => _width;

        /// <summary>
        /// Gets bounds height.
        /// </summary>
        public float Height => _height;

        /// <summary>
        /// Gets center point.
        /// </summary>
        public Vector2 Center => new Vector2(_centerX, _centerY);

        /// <summary>
        /// Gets bounds size.
        /// </summary>
        public Vector2 Size => new Vector2(_width, _height);

        /// <summary>
        /// Gets half-size of bounds.
        /// </summary>
        public Vector2 Extents => Size * 0.5f;

        /// <summary>
        /// Gets lower-left corner.
        /// </summary>
        public Vector2 Min => Center - Extents;

        /// <summary>
        /// Gets upper-right corner.
        /// </summary>
        public Vector2 Max => Center + Extents;

        /// <summary>
        /// Gets left X boundary.
        /// </summary>
        public float Left => Min.x;

        /// <summary>
        /// Gets right X boundary.
        /// </summary>
        public float Right => Max.x;

        /// <summary>
        /// Gets bottom Y boundary.
        /// </summary>
        public float Bottom => Min.y;

        /// <summary>
        /// Gets top Y boundary.
        /// </summary>
        public float Top => Max.y;

        /// <summary>
        /// Creates bounds from corner points.
        /// </summary>
        /// <param name="min">One corner of bounds.</param>
        /// <param name="max">Opposite corner of bounds.</param>
        /// <returns>Bounds that span between normalized corners.</returns>
        public static Bounds2d FromMinMax(Vector2 min, Vector2 max)
        {
            var minX = Mathf.Min(min.x, max.x);
            var minY = Mathf.Min(min.y, max.y);
            var maxX = Mathf.Max(min.x, max.x);
            var maxY = Mathf.Max(min.y, max.y);

            return new Bounds2d(
                (minX + maxX) * 0.5f,
                (minY + maxY) * 0.5f,
                maxX - minX,
                maxY - minY);
        }

        /// <summary>
        /// Determines whether bounds contain specified point.
        /// </summary>
        /// <param name="point">Point to test.</param>
        /// <returns><see langword="true" /> if point lies inside or on edge; otherwise, <see langword="false" />.</returns>
        public bool Contains(Vector2 point)
        {
            var min = Min;
            var max = Max;

            return point.x >= min.x &&
                   point.x <= max.x &&
                   point.y >= min.y &&
                   point.y <= max.y;
        }

        /// <summary>
        /// Determines whether bounds fully contain other bounds.
        /// </summary>
        /// <param name="other">Bounds to test.</param>
        /// <returns><see langword="true" /> if all corners of <paramref name="other" /> lie inside current bounds.</returns>
        public bool Contains(Bounds2d other)
        {
            var otherMin = other.Min;
            var otherMax = other.Max;
            return Contains(otherMin) && Contains(otherMax);
        }

        /// <summary>
        /// Determines whether bounds overlap another bounds.
        /// </summary>
        /// <param name="other">Bounds to test.</param>
        /// <returns><see langword="true" /> when bounds share area or edge; otherwise, <see langword="false" />.</returns>
        public bool Overlaps(Bounds2d other)
        {
            var min = Min;
            var max = Max;
            var otherMin = other.Min;
            var otherMax = other.Max;

            return min.x <= otherMax.x &&
                   max.x >= otherMin.x &&
                   min.y <= otherMax.y &&
                   max.y >= otherMin.y;
        }

        /// <summary>
        /// Returns bounds expanded to include specified point.
        /// </summary>
        /// <param name="point">Point to include.</param>
        /// <returns>New bounds that contain both current bounds and <paramref name="point" />.</returns>
        public Bounds2d Encapsulate(Vector2 point)
        {
            var min = Min;
            var max = Max;

            return FromMinMax(
                new Vector2(Mathf.Min(min.x, point.x), Mathf.Min(min.y, point.y)),
                new Vector2(Mathf.Max(max.x, point.x), Mathf.Max(max.y, point.y)));
        }

        /// <summary>
        /// Returns bounds expanded to include another bounds.
        /// </summary>
        /// <param name="other">Bounds to include.</param>
        /// <returns>New bounds that contain both current bounds and <paramref name="other" />.</returns>
        public Bounds2d Encapsulate(Bounds2d other)
        {
            var min = Min;
            var max = Max;
            var otherMin = other.Min;
            var otherMax = other.Max;

            return FromMinMax(
                new Vector2(Mathf.Min(min.x, otherMin.x), Mathf.Min(min.y, otherMin.y)),
                new Vector2(Mathf.Max(max.x, otherMax.x), Mathf.Max(max.y, otherMax.y)));
        }

        /// <summary>
        /// Returns combined bounds of current bounds and another bounds.
        /// </summary>
        /// <param name="other">Bounds to include.</param>
        /// <returns>Union of both bounds.</returns>
        public Bounds2d Union(Bounds2d other) => Encapsulate(other);

        /// <summary>
        /// Returns overlap region with another bounds when it exists.
        /// </summary>
        /// <param name="other">Bounds to intersect with.</param>
        /// <returns>Intersection bounds, or <see langword="null" /> when bounds are disjoint.</returns>
        public Bounds2d? Intersection(Bounds2d other)
        {
            return TryIntersection(other, out var intersection)
                ? intersection
                : (Bounds2d?)null;
        }

        /// <summary>
        /// Attempts to compute overlap region with another bounds.
        /// </summary>
        /// <param name="other">Bounds to intersect with.</param>
        /// <param name="intersection">Receives intersection bounds when overlap exists.</param>
        /// <returns><see langword="true" /> when overlap exists; otherwise, <see langword="false" />.</returns>
        public bool TryIntersection(Bounds2d other, out Bounds2d intersection)
        {
            var min = Min;
            var max = Max;
            var otherMin = other.Min;
            var otherMax = other.Max;

            var left = Mathf.Max(min.x, otherMin.x);
            var bottom = Mathf.Max(min.y, otherMin.y);
            var right = Mathf.Min(max.x, otherMax.x);
            var top = Mathf.Min(max.y, otherMax.y);

            if (left > right || bottom > top)
            {
                intersection = default;
                return false;
            }

            intersection = FromMinMax(new Vector2(left, bottom), new Vector2(right, top));
            return true;
        }

        /// <summary>
        /// Returns point clamped to current bounds.
        /// </summary>
        /// <param name="point">Point to clamp.</param>
        /// <returns>Point moved into bounds if necessary.</returns>
        public Vector2 ClampPoint(Vector2 point)
        {
            var min = Min;
            var max = Max;

            return new Vector2(
                Mathf.Clamp(point.x, min.x, max.x),
                Mathf.Clamp(point.y, min.y, max.y));
        }

        /// <summary>
        /// Returns bounds expanded equally on both axes.
        /// </summary>
        /// <param name="amount">Total amount added to width and height.</param>
        /// <returns>Expanded bounds.</returns>
        public Bounds2d Expand(float amount) => Expand(new Vector2(amount, amount));

        /// <summary>
        /// Returns bounds expanded independently on each axis.
        /// </summary>
        /// <param name="amount">Total amount added to size on each axis.</param>
        /// <returns>Expanded bounds.</returns>
        public Bounds2d Expand(Vector2 amount)
        {
            return new Bounds2d(
                Center,
                new Vector2(
                    Mathf.Max(0f, _width + amount.x),
                    Mathf.Max(0f, _height + amount.y)));
        }

        /// <inheritdoc />
        public bool Equals(Bounds2d other)
        {
            return _centerX.Equals(other._centerX) &&
                   _centerY.Equals(other._centerY) &&
                   _width.Equals(other._width) &&
                   _height.Equals(other._height);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Bounds2d other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(_centerX, _centerY, _width, _height);

        /// <summary>
        /// Returns formatted bounds string.
        /// </summary>
        public override string ToString() => $"Center: {Center}, Size: {Size}";

        public static bool operator ==(Bounds2d left, Bounds2d right) => left.Equals(right);

        public static bool operator !=(Bounds2d left, Bounds2d right) => !left.Equals(right);
    }
}
