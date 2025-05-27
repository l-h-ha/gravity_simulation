using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace gravity_simulation.Models
{
    internal class AABB
    {
        public Models.Vector2 Center { get; set; }
        public Models.Vector2 HalfSize { get; set; }
        public AABB(Models.Vector2 center, Models.Vector2 halfSize)
        {
            Center = center;
            HalfSize = halfSize;
        }
        public bool Contains(Body point)
        {
            Models.Vector2 position = point.Position;
            return (position.X >= Center.X - HalfSize.X && position.X <= Center.X + HalfSize.X &&
                    position.Y >= Center.Y - HalfSize.Y && position.Y <= Center.Y + HalfSize.Y);
        }

        public bool Intersects(AABB other)
        {
            return (Center.X - HalfSize.X < other.Center.X + other.HalfSize.X &&
                    Center.X + HalfSize.X > other.Center.X - other.HalfSize.X &&
                    Center.Y - HalfSize.Y < other.Center.Y + other.HalfSize.Y &&
                    Center.Y + HalfSize.Y > other.Center.Y - other.HalfSize.Y);
        }
    }

    internal class Quadtree
    {
        const int QT_NODE_CAPACITY = 4;

        public AABB Boundary;
        public List<Body> Bodies;

        public Quadtree Topleft;
        public Quadtree Topright;
        public Quadtree Bottomleft;
        public Quadtree Bottomright;

        public Quadtree(AABB boundary)
        {
            Boundary = boundary;
            Bodies = new List<Body>();
            Topleft = null;
            Topright = null;
            Bottomleft = null;
            Bottomright = null;
        }

        public bool Insert(Body point)
        {
            // If point is not within this node, returns false.

            if (!Boundary.Contains(point)) return false;

            // If this is a leaf node

            if (isLeaf())
            {
                // If this node has not reached capacity, add the point to this node.
                // OR if CurrentDepth is equal or greater to QT_NODE_DEPTH, add the point to this node.

                if (Bodies.Count + 1 < QT_NODE_CAPACITY)
                {
                    Bodies.Add(point);
                    return true;
                }

                // This node has reached capacity and depth has not reached maximum. Subdivide the node.

                Subdivide();
            }

            // Insert the point into the appropriate child node.

            if      (Topleft.Boundary.Contains(point))     return Topleft.Insert(point);
            else if (Topright.Boundary.Contains(point))    return Topright.Insert(point);
            else if (Bottomleft.Boundary.Contains(point))  return Bottomleft.Insert(point);
            else if (Bottomright.Boundary.Contains(point)) return Bottomright.Insert(point);

            // Point cannot be inserted god knows why. Returns false.

            Debug.Print($"Point {point.Position} at boundary of {Boundary.Center} could not be placed in a child.");

            return false;
        }

        public bool Remove(Body body)
        {
            // If point is not within this node, it must not be here. Returns false.

            if (!Boundary.Contains(body)) return false;

            // Point is within this node.

            // Case 1: Node has not subdivided yet. Removes the point from this node.

            if (isLeaf()) return Bodies.Remove(body);

            // Case 2: Node has subdivided. Removes the point from the child nodes.

            bool removed = false;

            if      (Topleft.Boundary.Contains(body)     && Topleft.Remove(body))     removed = true;

            else if (Topright.Boundary.Contains(body)    && Topright.Remove(body))    removed = true;

            else if (Bottomleft.Boundary.Contains(body)  && Bottomleft.Remove(body))  removed = true;

            else if (Bottomright.Boundary.Contains(body) && Bottomright.Remove(body)) removed = true;

            // If this node is empty, merge the child nodes into this node.

            if (removed && Count() < QT_NODE_CAPACITY) Merge();

            return removed;
        }

        public void Subdivide()
        {
            // Creating four quadrants.

            Models.Vector2 halfSize = new Models.Vector2(Boundary.HalfSize.X / 2, Boundary.HalfSize.Y / 2);
            Topleft = new Quadtree(new AABB(new Models.Vector2(Boundary.Center.X - halfSize.X, Boundary.Center.Y - halfSize.Y), halfSize));
            Topright = new Quadtree(new AABB(new Models.Vector2(Boundary.Center.X + halfSize.X, Boundary.Center.Y - halfSize.Y), halfSize));
            Bottomleft = new Quadtree(new AABB(new Models.Vector2(Boundary.Center.X - halfSize.X, Boundary.Center.Y + halfSize.Y), halfSize));
            Bottomright = new Quadtree(new AABB(new Models.Vector2(Boundary.Center.X + halfSize.X, Boundary.Center.Y + halfSize.Y), halfSize));

            // Move all bodies from this node to the child nodes.

            foreach (Body body in Bodies)
            {
                if (Topleft.Insert(body)) continue;
                if (Topright.Insert(body)) continue;
                if (Bottomleft.Insert(body)) continue;
                if (Bottomright.Insert(body)) continue;
            }

            // Clear the bodies from this node.

            Bodies.Clear();
        }

        public void Merge()
        {
            // If this node is a leaf node, return.

            if (isLeaf()) return;

            // This node is not a leaf node. Checking body count.
            // If body count does not exceed capacity, merge the child nodes into this node.

            if ( Count() < QT_NODE_CAPACITY )
            {
                // Clear the bodies from this node. Just in case.

                Bodies.Clear();

                // Move all bodies from the child nodes to this node.

                Topleft.AddBodyToList(Topleft, Bodies);
                Topright.AddBodyToList(Topright, Bodies);
                Bottomleft.AddBodyToList(Bottomleft, Bodies);
                Bottomright.AddBodyToList(Bottomright, Bodies);

                // Clear the child nodes.

                Topleft = null;
                Topright = null;
                Bottomleft = null;
                Bottomright = null;
            }
        }

        public void AddBodyToList(Quadtree node, List<Body> targetList)
        {
            // If node is a leaf node, add the bodies to the target list.

            if (node.isLeaf())
            {
                targetList.AddRange(node.Bodies);
                return;
            }

            // This node is not a leaf node. Recursively add the bodies to the target list.

            AddBodyToList(node.Topleft, targetList);
            AddBodyToList(node.Topright, targetList);
            AddBodyToList(node.Bottomleft, targetList);
            AddBodyToList(node.Bottomright, targetList);
        }

        public int Count()
        {
            // If this node is a leaf node, return the body count.

            if (isLeaf()) return Bodies.Count;

            // This node is not a leaf node. Return the sum of the body counts of the child nodes.

            int sum = 0;

            // The redundant checks (is not null) are here is to make sure 100% we count the leaves only.

            sum += Topleft.Count();
            sum += Topright.Count();
            sum += Bottomleft.Count();
            sum += Bottomright.Count();

            return sum;
        }

        public bool isLeaf()
        {
            return (Topleft is null && Topright is null && Bottomleft is null && Bottomright is null);
        }

        //

        public double GetWidth()
        {
            return Boundary.HalfSize.X * 2;
        }

        public double GetTotalMass()
        {
            List<Body> bodies = new List<Body>();
            AddBodyToList(this, bodies);

            double totalMass = 0;

            foreach (Body body in bodies)
            {
                totalMass += body.Mass;
            }

            return totalMass;
        }

        public Models.Vector2 GetCenterOfMass()
        {
            List<Body> bodies = new List<Body>();
            AddBodyToList(this, bodies);

            if (bodies.Count == 0) return Boundary.Center;

            double totalMass = 0;
            Models.Vector2 centerOfMass = new Models.Vector2(0, 0);

            foreach (Body body in bodies)
            {
                centerOfMass += body.Position * body.Mass;
                totalMass += body.Mass;
            }

            // A body of no mass exerts no gravitational force
             
            if (totalMass == 0) return Boundary.Center;

            centerOfMass /= totalMass;
            return centerOfMass;
        }
    }
}
