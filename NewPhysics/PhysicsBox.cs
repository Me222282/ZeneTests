using System;
using System.Collections.Generic;
using Zene.Physics;
using Zene.Structs;

namespace NewPhysics
{
    public class PhysicsBox : IPhysicsObject<PhysicsBounds>
    {
        public PhysicsBox(Vector2 location, Vector2 size, double mass, bool @static = false)
        {
            Bounds = new PhysicsBounds(0d, size);
            COM = location;
            Mass = mass;
            Static = @static;

            CompressiveStrength = 0.99;
        }

        public PhysicsBounds Bounds { get; }
        public IBox BoundingBox => new PhysicsBounds(Bounds.Location + COM, Bounds.Size);
        public double Mass { get; set; }
        public double CompressiveStrength { get; set; }
        public bool Static { get; set; }

        public Vector2 Acceleration { get; set; }

        public Vector2 COM { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 ResultantForce { get; set; }
        public Radian Rotation { get; set; }

        public List<IForceController> Forces { get; } = new List<IForceController>();
        //public List<Collision> Collisions { get; } = new List<Collision>();

        public void PushForce(Vector2 force)
        {
            ResultantForce += force;
        }
    }
}
