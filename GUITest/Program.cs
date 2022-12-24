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
            //window.Run();

            // End glfw
            Core.Terminate();
        }

        public Program(int width, int height, string title)
            : base(width, height, title, 4.3, true)
        {
            _em = new RootElement(this);
            Container c = new Container(new Layout(0d, 0d, 2d, 2d));
            _element = new TestElement()
            {
                CursorStyle = Cursor.Hand
            };
            c.AddChild(_element);
            c.AddChild(new Button(new Layout(0.7, 0.7, 0.25, 0.25))
            {
                Text = "BEANS!"
            });
            c.AddChild(new TextInput(new TextLayout(2d, 2d, 0d, 0.7))
            {
                TextSize = 25
            });

            Label l;
            _em.AddChild(l = new Label(new TextLayout(5d, 5d, -0.7, 0.7))
            {
                TextSize = 15,
                Text = "What's in his Shoe?",
                CursorStyle = Cursor.IBeam
            });
            l.Click += (_, _) =>
            {
                if (l.Text == "What's in his Shoe?")
                {
                    l.Text = "Not beans.";
                    return;
                }
                if (l.Text == "Not beans.")
                {
                    l.Text = "feet (Tim)";
                    return;
                }

                l.Text = "What's in his Shoe?";
            };

            _em.AddChild(c);
        }

        private readonly RootElement _em;
        private readonly TestElement _element;

        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);

            Framebuffer.Clear(BufferBit.Colour | BufferBit.Depth);

            _em.Render();
        }
    }
}