using System;

namespace gravity_simulation.Models
{
    internal class Body
    {
        // Properties

        public double Mass { get; set; }
        public double Size { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        // Constructor
        
        public Body(double _mass, double _size, Vector2 _position)
        {
            Mass = _mass;
            Size = _size;
            Position = _position;
            Velocity = Vector2.Zero;
        }

        // Methods

        public double DistanceTo(Body other)
        {
            return Math.Sqrt(Math.Pow(other.Position.X - this.Position.X, 2) + Math.Pow(other.Position.Y - this.Position.Y, 2));
        }
    }
}
