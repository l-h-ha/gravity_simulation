using System;

namespace gravity_simulation.Models
{
    public class Vector2
    {
        // Properties

        public double X { get; set; }
        public double Y { get; set; }

        // Constructor
        public Vector2(double _x, double _y)
        {
            X = _x;
            Y = _y;
        }

        // Static Properties

        public static Vector2 Zero { get; } = new Vector2(0, 0);
        public static Vector2 One { get; } = new Vector2(1, 1);
        public static Vector2 Up { get; } = new Vector2(0, 1);
        public static Vector2 Down { get; } = new Vector2(0, -1);
        public static Vector2 Left { get; } = new Vector2(-1, 0);
        public static Vector2 Right { get; } = new Vector2(1, 0);

        // Methods

        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public Vector2 Normalize()
        {
            double magnitude = Magnitude();
            if (magnitude == 0)
                throw new DivideByZeroException("Cannot normalize a zero vector.");
            return new Vector2(X / magnitude, Y / magnitude);
        }

        public double Dot(Vector2 other)
        {
            return X * other.X + Y * other.Y;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        // Operators

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(Vector2 a, double scalar)
        {
            return new Vector2(a.X * scalar, a.Y * scalar);
        }

        public static Vector2 operator *(double scalar, Vector2 a)
        {
            return new Vector2(a.X * scalar, a.Y * scalar);
        }

        public static Vector2 operator /(Vector2 a, double scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Cannot divide by zero.");
            return new Vector2(a.X / scalar, a.Y / scalar);
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.X, -a.Y);
        }
    }
}
