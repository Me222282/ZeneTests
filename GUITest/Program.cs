using System;
using Zene.Graphics;
using Zene.Structs;
using Zene.Windowing;
using Zene.GUI;
using System.IO;

namespace GUITest
{
    class Program : Window
    {
        static void Main()
        {
            // Start glfw
            Core.Init();

            Window window = new Program(800, 500, "Work", File.ReadAllText("GUI.xml"));
            window.RunMultithread();
            //window.Run();

            // End glfw
            Core.Terminate();
        }

        public Program(int width, int height, string title, string xml)
            : base(width, height, title, 4.3, true)
        {
            Xml decoder = new Xml();
            _em = decoder.LoadGUI(this, xml);

            State.Blending = true;
            State.SourceScaleBlending = BlendFunction.SourceAlpha;
            State.DestinationScaleBlending = BlendFunction.OneMinusSourceAlpha;
        }

        private static void LabelClick(object sender, MouseEventArgs e)
        {
            if (sender is not Label l) { return; }

            l.Text = l.Text switch
            {
                "What's in his Shoe?" => "Not beans.",
                "Not beans." => "feet (Tim)",
                _ => "What's in his Shoe?"
            };
        }

        private readonly RootElement _em;

        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);

            Framebuffer.Clear(BufferBit.Colour | BufferBit.Depth);

            _em.Render();
        }
    }
}