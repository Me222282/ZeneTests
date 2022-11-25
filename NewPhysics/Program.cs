using Zene.Graphics;
using Zene.Windowing;
using Zene.Structs;
using System;
using Zene.Physics;

namespace NewPhysics
{
    public class Program : Window
    {
        public static bool MultiThread { get; } = true;

        public static void Main()
        {
            Core.Init();

            Program window = new Program(800, 500, "Work");

            window.Run();

            Core.Terminate();
        }

        public Program(int width, int height, string title)
            : base(width, height, title)
        {
            _shader = new BasicShader();

            _drawable = new DrawObject<float, byte>(stackalloc float[]
            {
                0.5f, 0.5f,
                -0.5f, 0.5f,
                -0.5f, -0.5f,
                0.5f, -0.5f
            }, stackalloc byte[] { 0, 1, 2, 2, 3, 0 }, 2, 0, AttributeSize.D2, BufferUsage.DrawFrequent);

            // Enabling transparency
            State.Blending = true;
            Zene.Graphics.Base.GL.BlendFunc(Zene.Graphics.Base.GLEnum.SrcAlpha, Zene.Graphics.Base.GLEnum.OneMinusSrcAlpha);

            OnSizeChange(new SizeChangeEventArgs(width, height));

            _physics.Objects.Add(new PhysicsBox(0d, (5d, 5d), 1d));
            _physics.Objects.Add(new PhysicsBox((0d, -200), (500d, 50d), 1d, true));
            _physics.GlobalForces.Add(new GravityForce(9.8));
        }

        private readonly PhysicsManager<PhysicsBox, PhysicsBounds> _physics = new PhysicsManager<PhysicsBox, PhysicsBounds>(new BoxCollisions());

        private readonly BasicShader _shader;
        private readonly DrawObject<float, byte> _drawable;

        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);

            Framebuffer.Clear(new Colour(245, 245, 245));

            _physics.ManageFrame(1d / 60d);

            _shader.Bind();

            _shader.ColourSource = ColourSource.UniformColour;
            _shader.Colour = new ColourF(1f, 0f, 0f);
            
            foreach (PhysicsBox obj in _physics.Objects)
            {
                DrawObject(obj);
            }
        }

        private void DrawObject(PhysicsBox obj)
        {
            _shader.Matrix1 = Matrix4.CreateBox(obj.BoundingBox);

            _drawable.Draw();
        }

        protected override void OnSizeChange(SizeChangeEventArgs e)
        {
            base.OnSizeChange(e);

            _shader.Matrix3 = Matrix4.CreateOrthographic(e.Width, e.Height, -1d, 1d);
        }
    }
}
