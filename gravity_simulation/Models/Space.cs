using gravity_simulation.Constants;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace gravity_simulation.Models
{
    internal class Space
    {
        // Properties

        public List<Body> bodies;

        // Constructor
        
        public Space(int numBodies)
        {
            bodies = new List<Body>(numBodies);
        }

        // Methods

        public void AddBody(Body Body)
        {
            bodies.Add(Body);
        }

        public void Update(double dt, double targetWidthRatio, double targetHeightRatio)
        {
            double hypotenuse = Math.Sqrt(Math.Pow(targetWidthRatio, 2) + Math.Pow(targetHeightRatio, 2));

            for (int i = 0; i < bodies.Count; i++)
            {
                for (int j = 0; j < bodies.Count; j++)
                {
                    if (i != j)
                    {
                        double distance = Math.Max(bodies[i].DistanceTo(bodies[j]) * hypotenuse, bodies[i].Size + bodies[j].Size);
                        double force = (MathConstants.G * bodies[i].Mass * bodies[j].Mass) / (distance * distance + MathConstants.EPSILON_SQUARED);
                        Vector2 direction = (bodies[j].Position - bodies[i].Position).Normalize();
                        bodies[i].Velocity += direction * force / bodies[i].Mass;
                    }
                }
            }

            foreach (Body body in bodies)
            {
                body.Position += body.Velocity * dt;

                //Debug.WriteLine($"v: ({body.Velocity.X}, {body.Velocity.Y}) p: ({body.Position.X}, {body.Position.Y}) dt: {dt} a: ({body.Acceleration.X}, {body.Acceleration.Y})");
            }
        }
    }
}
