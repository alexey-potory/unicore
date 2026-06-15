using System;
using UnityEngine;

namespace Unicore.Math
{
    /// <summary>
    /// Represents normalized inclusive float range.
    /// </summary>
    public readonly struct Range : IEquatable<Range>
    {
        private readonly float _min;
        private readonly float _max;

        /// <summary>
        /// Initializes range and auto-normalizes endpoints.
        /// </summary>
        /// <param name="min">One range endpoint.</param>
        /// <param name="max">Other range endpoint.</param>
        public Range(float min, float max)
        {
            _min = Mathf.Min(min, max);
            _max = Mathf.Max(min, max);
        }

        /// <summary>
        /// Gets lower inclusive bound.
        /// </summary>
        public float Min => _min;

        /// <summary>
        /// Gets upper inclusive bound.
        /// </summary>
        public float Max => _max;

        /// <summary>
        /// Gets range length.
        /// </summary>
        public float Length => _max - _min;

        /// <summary>
        /// Gets midpoint between bounds.
        /// </summary>
        public float Center => (_min + _max) * 0.5f;

        /// <summary>
        /// Gets whether range collapsed into single point.
        /// </summary>
        public bool IsPoint => _min.Equals(_max);

        /// <summary>
        /// Determines whether range contains value.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <returns><see langword="true" /> if value lies inside or on bounds.</returns>
        public bool Contains(float value) => value >= _min && value <= _max;

        /// <summary>
        /// Determines whether range fully contains another range.
        /// </summary>
        /// <param name="other">Range to test.</param>
        /// <returns><see langword="true" /> if other range lies fully inside current range.</returns>
        public bool Contains(Range other) => other._min >= _min && other._max <= _max;

        /// <summary>
        /// Determines whether ranges overlap.
        /// </summary>
        /// <param name="other">Range to test.</param>
        /// <returns><see langword="true" /> if ranges share segment or edge.</returns>
        public bool Overlaps(Range other) => _min <= other._max && _max >= other._min;

        /// <summary>
        /// Clamps value into current range.
        /// </summary>
        /// <param name="value">Value to clamp.</param>
        /// <returns>Nearest value inside current range.</returns>
        public float Clamp(float value) => Mathf.Clamp(value, _min, _max);

        /// <summary>
        /// Maps normalized position into current range.
        /// </summary>
        /// <param name="t">Interpolation parameter.</param>
        /// <returns>Interpolated value.</returns>
        public float Lerp(float t) => Mathf.LerpUnclamped(_min, _max, t);

        /// <summary>
        /// Maps value into normalized position within current range.
        /// </summary>
        /// <param name="value">Value to map.</param>
        /// <returns>Relative position inside current range.</returns>
        public float InverseLerp(float value)
        {
            return Length <= 0f
                ? 0f
                : Mathf.InverseLerp(_min, _max, value) + (value < _min || value > _max
                    ? (value - Clamp(value)) / Length
                    : 0f);
        }

        /// <summary>
        /// Expands or shrinks range symmetrically around center.
        /// </summary>
        /// <param name="amount">Amount added to each side. Negative shrinks range.</param>
        /// <returns>Expanded or shrunk range.</returns>
        public Range Expand(float amount)
        {
            float center = Center;
            float halfLength = Length * 0.5f + amount;

            if (halfLength <= 0f)
            {
                return new Range(center, center);
            }

            return new Range(center - halfLength, center + halfLength);
        }

        /// <summary>
        /// Returns smallest range that contains current range and other range.
        /// </summary>
        /// <param name="other">Range to include.</param>
        /// <returns>Union range.</returns>
        public Range Union(Range other) => new(Mathf.Min(_min, other._min), Mathf.Max(_max, other._max));

        /// <summary>
        /// Returns shared overlap with another range when it exists.
        /// </summary>
        /// <param name="other">Range to intersect with.</param>
        /// <returns>Intersection range, or <see langword="null" /> when ranges do not overlap.</returns>
        public Range? Intersection(Range other)
        {
            return TryIntersection(other, out Range intersection)
                ? intersection
                : null;
        }

        /// <summary>
        /// Attempts to compute shared overlap with another range.
        /// </summary>
        /// <param name="other">Range to intersect with.</param>
        /// <param name="intersection">Receives shared overlap when it exists.</param>
        /// <returns><see langword="true" /> if overlap exists; otherwise, <see langword="false" />.</returns>
        public bool TryIntersection(Range other, out Range intersection)
        {
            float min = Mathf.Max(_min, other._min);
            float max = Mathf.Min(_max, other._max);

            if (min > max)
            {
                intersection = default;
                return false;
            }

            intersection = new Range(min, max);
            return true;
        }

        /// <inheritdoc />
        public bool Equals(Range other) => _min.Equals(other._min) && _max.Equals(other._max);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Range other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(_min, _max);

        /// <summary>
        /// Returns formatted range string.
        /// </summary>
        public override string ToString() => $"[{_min}, {_max}]";

        public static bool operator ==(Range left, Range right) => left.Equals(right);

        public static bool operator !=(Range left, Range right) => !left.Equals(right);
    }
}
