using Zene.Windowing;
using Zene.Graphics;
using Zene.Structs;
using System;

namespace ImplicitFunctions
{
    public class Program : Window
    {
        static void Main()
        {
            Core.Init();

            //Game window = new Game(1000, 1000, "Work");
            Program window = new Program(800, 800, "Work");

            window.Run();

            Core.Terminate();
        }

        public Program(int width, int height, string title)
            : base(width, height, title)
        {
            _drawable = new DrawObject<double, byte>(new double[]
            {
                -1, 1,
                -1, -1,
                1, -1,
                1, 1
            }, new byte[] { 0, 1, 2, 2, 3, 0 }, 2, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
            _drawable.AddAttribute(1, 0, AttributeSize.D2);

            _shader = new IFShader();
        }

        private readonly DrawObject<double, byte> _drawable;
        private IFShader _shader;

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                _drawable.Dispose();
                _shader.Dispose();
            }
        }

        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);

            // Clear screen black
            BaseFramebuffer.Clear(BufferBit.Colour);

            // Use shader and render object
            _shader.Bind();
            _drawable.Draw();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e[Keys.Escape])
            {
                Close();
                return;
            }
            if (e[Keys.F5])
            {
                _shader.Dispose();
                _shader = new IFShader();
                return;
            }
        }
    }
}
