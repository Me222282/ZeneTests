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
            window.RunMultithread();

            // End glfw
            Core.Terminate();
        }

        public Program(int width, int height, string title)
            : base(width, height, title, 4.3, true)
        {
            _em = new ElementManager(this);
            Container c = new Container(new Layout(0d, 0d, 2d, 2d));
            _element = new TestElement(new Layout(0d, 0d, 1d, 1d))
            {
                CursorStyle = Cursor.Hand
            };
            c.AddChild(_element);
            c.AddChild(new Button(new Layout(0.7, 0.7, 0.25, 0.25))
            {
                Font = new SampleFont(),
                Text = "Beans?"
            });

            _em.AddChild(c);
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