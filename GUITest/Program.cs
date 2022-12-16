using System;
using Zene.Graphics;
using Zene.Structs;
using Zene.Windowing;
using Zene.GUI;

namespace GUITest
{
    class Program : Window
    {
        static void Main()
        {
            // Start glfw
            Core.Init();

            Window window = new Program(800, 500, "Work");
            window.Run();

            // End glfw
            Core.Terminate();
        }

        public Program(int width, int height, string title)
            : base(width, height, title, 4.3)
        {
            _em = new ElementManager(this);
            Element p = new Element(new Layout(0d, 0d, 2d, 2d), false);
            _element = new TestElement(new Rectangle(-50, 50, 100, 100))
            {
                CursorStyle = Cursor.Hand,
                Layout = new Layout(new Box(Vector2.Zero, (1d, 1d)))
            };
            p.AddChild(_element);

            _em.AddChild(p);
        }

        private readonly ElementManager _em;
        private readonly TestElement _element;

        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);

            Framebuffer.Clear(BufferBit.Colour | BufferBit.Depth);

            _em.Render();
        }
    }
}