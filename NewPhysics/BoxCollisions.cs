using System;
using System.Collections.Generic;
using Zene.Physics;
using Zene.Structs;

namespace NewPhysics
{
    public class BoxCollisions : ICollisionManager<PhysicsBox, PhysicsBounds>
    {
        public void ManageCollisions(List<PhysicsBox> objs)
        {
            ForEachPair(objs, (a, b) =>
            {
                // Static objects cannot collide
                if (a.Static && b.Static) { return; }

                if (!CheckCollision(
                        a.BoundingBox,
                        a.Velocity,
                        b.BoundingBox,
                        b.Velocity,
                        out Collision ca,
                        out Collision cb))
                {
                    return;
                }

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

        private static bool CheckCollision(IBox a, Vector2 va, IBox b, Vector2 vb, out Collision ca, out Collision cb)
        {
            
        }
    }
}
