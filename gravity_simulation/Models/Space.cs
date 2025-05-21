using gravity_simulation.Constants;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace gravity_simulation.Models
{
    internal class Space
    {
        // Properties

        public List<Body> Bodies;
        public Quadtree Quadtree;

        public Models.Vector2 Viewport;

        // Constructor
        
        public Space(int numBodies, Models.Vector2 viewport)
        {
            Bodies = new List<Body>(numBodies);
            Viewport = viewport;
            Quadtree = new Quadtree(new AABB(viewport / 2, viewport / 2));
        }

        // Methods

        public void AddBody(Body Body)
        {
            Bodies.Add(Body);
            Quadtree.Insert(Body);
        }

        public void Update(double dt)
        {
            // Udate the quadtree with the new positions of the bodies

            AABB boundary = Quadtree.Boundary;
            Quadtree = new Quadtree(boundary);

            foreach (Body body in Bodies)
            {
                Quadtree.Insert(body);
            }

            // Update the positions and velocities of the bodies based on gravitational forces

            for (int i = 0; i < Bodies.Count; i++)
            {
                for (int j = 0; j < Bodies.Count; j++)
                {
                    if (i != j)
                    {
                        double distance = Math.Max(Bodies[i].DistanceTo(Bodies[j]), Bodies[i].Radius + Bodies[j].Radius) / 10;
                        double force = (MathConstants.G * Bodies[i].Mass * Bodies[j].Mass) / (distance * distance + MathConstants.EPSILON_SQUARED);
                        Vector2 direction = (Bodies[j].Position - Bodies[i].Position).Normalize();
                        Bodies[i].Velocity += (direction * force / Bodies[i].Mass) * dt; // v += a * dt
                    }
                }
            }

            foreach (Body body in Bodies)
            {
                body.Position += body.Velocity * dt; // p += v * dt
            }

            // Wrap the body within the viewport

            foreach (Body body in Bodies)
            {
                if (body.Position.X < 0) body.Position.X = Viewport.X;
                if (body.Position.Y < 0) body.Position.Y = Viewport.Y;
                if (body.Position.X > Viewport.X) body.Position.X = 0;
                if (body.Position.Y > Viewport.Y) body.Position.Y = 0;
            }
        }
    }
}
