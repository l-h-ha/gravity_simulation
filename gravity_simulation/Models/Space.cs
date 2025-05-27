using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static gravity_simulation.Constants.MathConstants;

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

        public Models.Vector2 CalculateNetForce(Body obj, Quadtree node)
        {
            Models.Vector2 netForce = Vector2.Zero;

            double objMass = obj.Mass;
            Models.Vector2 objPosition = obj.Position;
            
            if (node.isLeaf())
            {
                int bodyCount = node.Bodies.Count();

                if (bodyCount == 0)
                {
                    return netForce;
                }

                foreach (Body body in node.Bodies)
                {
                    if (body == obj || body is null) continue;

                    double bodyMass = body.Mass;
                    Models.Vector2 bodyPosition = body.Position;

                    Models.Vector2 dir = bodyPosition - objPosition;

                    double distanceSquared = Math.Max(dir.MagnitudeSquared(), Math.Pow(obj.Radius + body.Radius, 2));
                    double forceScale = (G * objMass * bodyMass) / (distanceSquared + EPSILON_SQUARED); // F = G * m1 * m2 / r^2

                    Models.Vector2 force = dir.Normalize() * forceScale;
                    netForce += force;
                }
            }

            // Accounted node is an internal node, we will check whether to approximate or to go further

            else
            {
                double distanceToNode = (node.Boundary.Center - objPosition).Magnitude();
                double nodeWidth = node.GetWidth();

                // Since node is sufficiently far away, we can approximate it as a single body

                if (nodeWidth / distanceToNode < THETA)
                {
                    double nodeMass = node.GetTotalMass();
                    Models.Vector2 nodeCenterOfMass = node.GetCenterOfMass();
                   
                    Models.Vector2 dir = nodeCenterOfMass - objPosition;

                    double distanceSquared = Math.Max(dir.MagnitudeSquared(), Math.Pow(objMass + nodeMass, 2));
                    double forceScale = (G * objMass * nodeMass) / (distanceSquared + EPSILON_SQUARED); // F = G * m1 * m2 / r^2
                    
                    Models.Vector2 force = dir.Normalize() * forceScale;
                    netForce += force;
                }

                // Node is not sufficiently far away, we will go deeper into the tree

                else
                {
                    netForce += CalculateNetForce(obj, node.Topleft);
                    netForce += CalculateNetForce(obj, node.Topright);
                    netForce += CalculateNetForce(obj, node.Bottomleft);
                    netForce += CalculateNetForce(obj, node.Bottomright);
                }
            }

            return netForce;
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

            // Barnes-Hut solution

            foreach (Body body in Bodies)
            {
                Models.Vector2 netForce = CalculateNetForce(body, Quadtree);
                body.Velocity += netForce / body.Mass * dt; // v += a * dt
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
