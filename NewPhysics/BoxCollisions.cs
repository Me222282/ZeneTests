using System;
using System.Collections.Generic;
using Zene.Physics;
using Zene.Structs;

namespace NewPhysics
{
    public class BoxCollisions : ICollisionManager<PhysicsBox, PhysicsBounds>
    {
        public void ManageCollisions(List<PhysicsBox> objs, double frameTime)
        {
            ForEachPair(objs, (a, b) =>
            {
                // Static objects cannot collide
                if (a.Static && b.Static) { return; }

                if (!CheckCollision(
                        a.BoundingBox,
                        a.Velocity * frameTime,
                        b.BoundingBox,
                        b.Velocity * frameTime,
                        out Collision<PhysicsBox, PhysicsBounds> ca,
                        out Collision<PhysicsBox, PhysicsBounds> cb))
                {
                    return;
                }

                Console.WriteLine(ca.PathPercentage * frameTime);

                ca.PathPercentage *= frameTime;
                cb.PathPercentage *= frameTime;

                ca.Object = b;
                cb.Object = a;

                if (!a.Static)
                {
                    PhysicsManager<PhysicsBox, PhysicsBounds>.ResolveCollision(a, ca);
                    //a.Collisions.Add(ca);
                }
                if (!b.Static)
                {
                    PhysicsManager<PhysicsBox, PhysicsBounds>.ResolveCollision(b, cb);
                    //b.Collisions.Add(cb);
                }
            });
        }

        private static void ForEachPair<T>(List<T> list, Action<T, T> a)
        {
            int c = list.Count - 1;
            for (int i = 0; i < c; i++)
            {
                for (int p = i + 1; p < list.Count; p++)
                {
                    a.Invoke(list[i], list[p]);
                }
            }
        }

        private static bool CheckCollision<T, F>(IBox a, Vector2 va, IBox bx, Vector2 vb, out Collision<T, F> ca, out Collision<T, F> cb)
            where T : IPhysicsObject<F>
            where F : IPhysicsBounds
        {
            /*
            double l = Math.Min(a.Left, a.Left + va.X);
            double r = Math.Max(a.Right, a.Right + va.X);
            double t = Math.Min(a.Top, a.Top + va.Y);
            double b = Math.Max(a.Bottom, a.Bottom + va.Y);

            double left = a.Left;
            double right = a.Right;
            double top = a.Top;
            double bottom = a.Bottom;

            ca = new Collision<T, F>();
            cb = new Collision<T, F>();

            bool movingX = va.X != 0;
            bool movingY = va.Y != 0;

            bool PositiveVX = va.X > 0;
            bool PositiveVY = va.Y > 0;
            bool NegativeVX = va.X < 0;
            bool NegativeVY = va.Y < 0;

            IBox rect = bx;

            double rL = rect.Left;
            double rR = rect.Right;
            double rT = rect.Top;
            double rB = rect.Bottom;

            bool inX;
            bool inY;

            double distance;

            if (!movingX || !movingY)
            {
                if (PositiveVX)
                {
                    inX = (rR >= left) && (rL < right);
                }
                else if (NegativeVX)
                {
                    inX = (rR > left) && (rL <= right);
                }
                else
                {
                    inX = (rR >= left) && (rL <= right);
                }
                if (PositiveVY)
                {
                    inY = (rT > bottom) && (rB <= top);
                }
                else if (NegativeVY)
                {
                    inY = (rT >= bottom) && (rB < top);
                }
                else
                {
                    inY = (rT >= bottom) && (rB <= top);
                }

                if (inX && inY)
                {
                    //continue;
                    return false;
                }

                bool outX;
                bool outY;

                if (movingX)
                {
                    outX = (rR < l) || (rL > r);
                    outY = (rT >= b) || (rB <= t);
                }
                else
                {
                    outX = (rR <= l) || (rL >= r);
                    outY = (rT > b) || (rB < t);
                }

                // Return if outside the minimum bounding box of the cast.
                if (outX || outY)
                {
                    //continue;
                    return false;
                }

                double p1;
                double p2;

                if (movingX)
                {
                    distance = va.X;

                    if (PositiveVX)
                    {
                        p1 = right;
                        p2 = rL;
                    }
                    else
                    {
                        p1 = left;
                        p2 = rR;
                    }
                }
                else
                {
                    distance = va.Y;

                    if (va.Y > 0)
                    {
                        p1 = bottom;
                        p2 = rT;
                    }
                    else
                    {
                        p1 = top;
                        p2 = rB;
                    }
                }

                double percent = (p2 - p1) / distance;

                ca.PathPercentage = percent;

                return true;
            }

            Vector2 tLineP1;
            Vector2 tLineP2;

            Vector2 bLineP1;
            Vector2 bLineP2;

            double xModifer = 0;
            double yModifer = 0;

            if (NegativeVX)
            {
                xModifer = a.Width;
            }
            if (!NegativeVY)
            {
                bLineP1 = new Vector2(a.Left + xModifer, a.Bottom + yModifer);
                bLineP2 = new Vector2(a.Left + va.X + xModifer, a.Bottom + va.Y + yModifer);

                tLineP1 = new Vector2(a.Right - xModifer, a.Top - yModifer);
                tLineP2 = new Vector2(a.Right + va.X - xModifer, a.Top + va.Y - yModifer);
            }
            else
            {
                yModifer = a.Height;

                tLineP1 = new Vector2(a.Left + xModifer, a.Bottom + yModifer);
                tLineP2 = new Vector2(a.Left + va.X + xModifer, a.Bottom + va.Y + yModifer);

                bLineP1 = new Vector2(a.Right - xModifer, a.Top - yModifer);
                bLineP2 = new Vector2(a.Right + va.X - xModifer, a.Top + va.Y - yModifer);
            }

            Vector2 mLine = new Vector2(a.Right - xModifer, a.Bottom + yModifer);

            double gradient = va.Y / va.X;

            double yCeptT = tLineP1.Y - (gradient * tLineP1.X);
            double yCeptB = bLineP1.Y - (gradient * bLineP1.X);

            double findTopY(double x)
            {
                return (gradient * x) + yCeptT;
            }
            double findBottomY(double x)
            {
                return (gradient * x) + yCeptB;
            }

            double findTopX(double y)
            {
                return (y - yCeptT) / gradient;
            }
            double findBottomX(double y)
            {
                return (y - yCeptB) / gradient;
            }

            double yCeptM = mLine.Y - (gradient * mLine.X);
            double findMidY(double x)
            {
                return (gradient * x) + yCeptM;
            }

            distance = Math.Sqrt(Math.Pow(va.X, 2) + Math.Pow(va.Y, 2));

            if (PositiveVX)
            {
                inX = (rR >= left) && (rL < right);
            }
            else if (NegativeVX)
            {
                inX = (rR > left) && (rL <= right);
            }
            else
            {
                inX = (rR >= left) && (rL <= right);
            }
            if (PositiveVY)
            {
                inY = (rT > bottom) && (rB <= top);
            }
            else if (NegativeVY)
            {
                inY = (rT >= bottom) && (rB < top);
            }
            else
            {
                inY = (rT >= bottom) && (rB <= top);
            }

            if (inX && inY)
            {
                return false;
            }

            // Return if outside the minimum bounding box of the cast.
            if ((rR < l) || (rL > r) || (rT < b) || (rB > t))
            {
                return false;
            }

            if ((findTopY(rL) >= rB || findTopY(rR) >= rB) && (findBottomY(rL) <= rT || findBottomY(rR) <= rT))
            {
                double thisX;
                double thisY;

                double sX;
                double sY;

                double cPointX;

                if (NegativeVX)
                {
                    cPointX = rR;
                }
                else
                {
                    cPointX = rL;
                }
                if (NegativeVY)
                {
                    if (findMidY(cPointX) > rB)
                    {
                        thisX = cPointX;
                        thisY = findBottomY(cPointX);

                        sX = bLineP1.X;
                        sY = bLineP1.Y;
                    }
                    else
                    {
                        thisX = findTopX(rB);
                        thisY = rB;

                        sX = tLineP1.X;
                        sY = tLineP1.Y;
                    }
                }
                else
                {
                    if (findMidY(cPointX) < rT)
                    {
                        thisX = cPointX;
                        thisY = findTopY(cPointX);

                        sX = tLineP1.X;
                        sY = tLineP1.Y;
                    }
                    else
                    {
                        thisX = findBottomX(rT);
                        thisY = rT;

                        sX = bLineP1.X;
                        sY = bLineP1.Y;
                    }
                }

                double ba = sX - thisX;
                double hi = sY - thisY;

                double tDistance = Math.Sqrt((ba * ba) + (hi * hi));

                double percent = tDistance / distance;

                ca.PathPercentage = percent;

                return true;
            }

            return false;*/
            
            cb = new Collision<T, F>();

            if (a.Bottom > bx.Top &&
                a.Bottom + va.Y < bx.Top)
            {
                ca = new Collision<T, F>(default, (a.Bottom - bx.Top) / va.Y);
                return true;
            }
            if (a.Top < bx.Bottom &&
                a.Top + va.Y > bx.Bottom)
            {
                ca = new Collision<T, F>(default, (a.Top - bx.Bottom) / va.Y);
                return true;
            }

            if (a.Left > bx.Right &&
                a.Left + va.X < bx.Right)
            {
                ca = new Collision<T, F>(default, (a.Left - bx.Right) / va.X);
                return true;
            }
            if (a.Right < bx.Left &&
                a.Right + va.X > bx.Left)
            {
                ca = new Collision<T, F>(default, (a.Right - bx.Left) / va.X);
                return true;
            }

            ca = new Collision<T, F>();
            return false;
        }
    }
}
