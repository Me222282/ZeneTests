using System;
using Zene.Graphics;
using Zene.GUI;
using Zene.Structs;

namespace GUITest
{
    class TestElement : Element
    {
        public TestElement(IBox bounds)
            : base(bounds)
        {
            
        }

        private Font f = new SampleFont();

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);

            Colour c = new Colour(255, 244, 233);

            if (MouseSelect)
            {
                c = new Colour(199, 144, 202);
            }
            else if (MouseHover)
            {
                c = new Colour(166, 188, 199);
            }

            e.Framebuffer.Clear(c);

            TextRenderer.Model = Matrix4.CreateScale(100);
            TextRenderer.DrawCentred("Test", f, 0, 0);
        }
    }
}
