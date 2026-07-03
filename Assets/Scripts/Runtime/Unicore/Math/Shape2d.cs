using System;
using UnityEngine;

namespace Unicore.Math
{
    /// <summary>
    /// Represents 2D polygon shape. Supports concave outlines.
    /// </summary>
    public readonly struct Shape2d : IEquatable<Shape2d>
    {
        private readonly Vector2[] _points;

        /// <summary>
        /// Initializes shape from ordered polygon points.
        /// </summary>
        /// <param name="points">Polygon points in winding order.</param>
        public Shape2d(Vector2[] points) : this(points, true)
        {
        }

        private Shape2d(Vector2[] points, bool copy)
        {
            if (points == null || points.Length == 0)
            {
                _points = Array.Empty<Vector2>();
                return;
            }

            _points = copy
                ? (Vector2[])points.Clone()
                : points;
        }

        /// <summary>
        /// Creates shape from polygon points.
        /// </summary>
        /// <param name="points">Polygon points in winding order.</param>
        /// <returns>Shape built from given points.</returns>
        public static Shape2d FromPoints(params Vector2[] points) => new(points);

        /// <summary>
        /// Gets number of points in shape.
        /// </summary>
        public int Count => _points?.Length ?? 0;

        /// <summary>
        /// Gets whether shape has no points.
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Gets point at specified index.
        /// </summary>
        /// <param name="index">Point index.</param>
        public Vector2 this[int index] => _points[index];

        /// <summary>
        /// Gets copy of polygon points.
        /// </summary>
        public Vector2[] Points => ToArray();

        /// <summary>
        /// Gets axis-aligned bounds of shape.
        /// </summary>
        public Bounds2d Bounds
        {
            get
            {
                if (IsEmpty)
                {
                    return default;
                }

                Vector2 min = _points[0];
                Vector2 max = _points[0];

                for (int i = 1; i < Count; i++)
                {
                    Vector2 point = _points[i];
                    min = Vector2.Min(min, point);
                    max = Vector2.Max(max, point);
                }

                return Bounds2d.FromMinMax(min, max);
            }
        }

        /// <summary>
        /// Gets signed polygon area. Counter-clockwise winding is positive.
        /// </summary>
        public float SignedArea => GetSignedArea();

        /// <summary>
        /// Gets absolute polygon area.
        /// </summary>
        public float Area => Mathf.Abs(SignedArea);

        /// <summary>
        /// Gets whether polygon winding is clockwise.
        /// </summary>
        public bool IsClockwise => SignedArea < 0f;

        /// <summary>
        /// Gets total outline length.
        /// </summary>
        public float Perimeter => GetPerimeter();

        /// <summary>
        /// Gets polygon centroid.
        /// </summary>
        public Vector2 Centroid => GetCentroid();

        /// <summary>
        /// Returns copy of polygon points.
        /// </summary>
        /// <returns>Array copy of shape points.</returns>
        public Vector2[] ToArray()
        {
            if (IsEmpty)
            {
                return Array.Empty<Vector2>();
            }

            return (Vector2[])_points.Clone();
        }

        /// <summary>
        /// Determines whether point lies inside shape or on its boundary.
        /// </summary>
        /// <param name="point">Point to test.</param>
        /// <returns><see langword="true" /> if point is inside or on edge; otherwise, <see langword="false" />.</returns>
        public bool Contains(Vector2 point)
        {
            switch (Count)
            {
                case 0:
                    return false;
                case 1:
                    return Approximately(_points[0], point);
                case 2:
                    return IsPointOnSegment(point, _points[0], _points[1]);
            }

            bool contains = false;
            Vector2 previous = _points[Count - 1];

            for (int i = 0; i < Count; i++)
            {
                Vector2 current = _points[i];
                if (IsPointOnSegment(point, previous, current))
                {
                    return true;
                }

                bool crossesScanline = (current.y > point.y) != (previous.y > point.y);
                if (crossesScanline)
                {
                    float intersectionX = (previous.x - current.x) * (point.y - current.y) / (previous.y - current.y) + current.x;
                    if (point.x < intersectionX)
                    {
                        contains = !contains;
                    }
                }

                previous = current;
            }

            return contains;
        }

        /// <summary>
        /// Returns total outline length.
        /// </summary>
        /// <returns>Closed-loop perimeter, or degenerate equivalent for lower point counts.</returns>
        public float GetPerimeter()
        {
            switch (Count)
            {
                case 0:
                case 1:
                    return 0f;
                case 2:
                    return Vector2.Distance(_points[0], _points[1]) * 2f;
            }

            float perimeter = 0f;
            Vector2 previous = _points[Count - 1];

            for (int i = 0; i < Count; i++)
            {
                Vector2 current = _points[i];
                perimeter += Vector2.Distance(previous, current);
                previous = current;
            }

            return perimeter;
        }

        /// <summary>
        /// Returns polygon centroid. Degenerate shapes fall back to arithmetic mean of points.
        /// </summary>
        /// <returns>Centroid or average point for degenerate shapes.</returns>
        public Vector2 GetCentroid()
        {
            switch (Count)
            {
                case 0:
                    return Vector2.zero;
                case 1:
                    return _points[0];
                case 2:
                    return (_points[0] + _points[1]) * 0.5f;
            }

            float twiceSignedArea = 0f;
            float centroidX = 0f;
            float centroidY = 0f;
            Vector2 previous = _points[Count - 1];

            for (int i = 0; i < Count; i++)
            {
                Vector2 current = _points[i];
                float cross = previous.x * current.y - current.x * previous.y;
                twiceSignedArea += cross;
                centroidX += (previous.x + current.x) * cross;
                centroidY += (previous.y + current.y) * cross;
                previous = current;
            }

            if (Mathf.Abs(twiceSignedArea) <= Mathf.Epsilon)
            {
                return GetAveragePoint();
            }

            float factor = 1f / (3f * twiceSignedArea);
            return new Vector2(centroidX * factor, centroidY * factor);
        }

        /// <summary>
        /// Returns translated copy of shape.
        /// </summary>
        /// <param name="offset">Translation offset.</param>
        /// <returns>Translated shape.</returns>
        public Shape2d Translate(Vector2 offset) => Transform(Matrix2d.Translate(offset));

        /// <summary>
        /// Returns scaled copy of shape around origin.
        /// </summary>
        /// <param name="scale">Per-axis scale.</param>
        /// <returns>Scaled shape.</returns>
        public Shape2d Scale(Vector2 scale) => Transform(Matrix2d.Scale(scale));

        /// <summary>
        /// Returns transformed copy of shape.
        /// </summary>
        /// <param name="matrix">Affine transform applied to each point.</param>
        /// <returns>Transformed shape.</returns>
        public Shape2d Transform(Matrix2d matrix)
        {
            if (IsEmpty)
            {
                return new Shape2d(Array.Empty<Vector2>(), false);
            }

            Vector2[] transformed = new Vector2[Count];
            for (int i = 0; i < Count; i++)
            {
                transformed[i] = matrix.TransformPoint(_points[i]);
            }

            return new Shape2d(transformed, false);
        }

        /// <summary>
        /// Returns shape with reversed point order.
        /// </summary>
        /// <returns>Shape with opposite winding.</returns>
        public Shape2d ReverseWinding()
        {
            if (Count <= 1)
            {
                return new Shape2d(ToArray(), false);
            }

            Vector2[] reversed = new Vector2[Count];
            for (int i = 0; i < Count; i++)
            {
                reversed[i] = _points[Count - 1 - i];
            }

            return new Shape2d(reversed, false);
        }

        /// <inheritdoc />
        public bool Equals(Shape2d other)
        {
            if (Count != other.Count)
            {
                return false;
            }

            for (int i = 0; i < Count; i++)
            {
                if (_points[i] != other._points[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Shape2d other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            for (int i = 0; i < Count; i++)
            {
                hash.Add(_points[i]);
            }

            return hash.ToHashCode();
        }

        /// <summary>
        /// Returns formatted shape string.
        /// </summary>
        public override string ToString() => $"Count: {Count}, Area: {Area}";

        public static bool operator ==(Shape2d left, Shape2d right) => left.Equals(right);

        public static bool operator !=(Shape2d left, Shape2d right) => !left.Equals(right);

        private float GetSignedArea()
        {
            if (Count < 3)
            {
                return 0f;
            }

            float sum = 0f;
            Vector2 previous = _points[Count - 1];

            for (int i = 0; i < Count; i++)
            {
                Vector2 current = _points[i];
                sum += previous.x * current.y - current.x * previous.y;
                previous = current;
            }

            return sum * 0.5f;
        }

        private Vector2 GetAveragePoint()
        {
            if (IsEmpty)
            {
                return Vector2.zero;
            }

            Vector2 sum = Vector2.zero;
            for (int i = 0; i < Count; i++)
            {
                sum += _points[i];
            }

            return sum / Count;
        }

        private static bool Approximately(Vector2 left, Vector2 right) => (left - right).sqrMagnitude <= Mathf.Epsilon * Mathf.Epsilon;

        private static bool IsPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            Vector2 toPoint = point - start;
            float cross = segment.x * toPoint.y - segment.y * toPoint.x;
            if (Mathf.Abs(cross) > Mathf.Epsilon)
            {
                return false;
            }

            float dot = Vector2.Dot(toPoint, segment);
            if (dot < -Mathf.Epsilon)
            {
                return false;
            }

            float squaredLength = segment.sqrMagnitude;
            return dot - squaredLength <= Mathf.Epsilon;
        }
    }
}
