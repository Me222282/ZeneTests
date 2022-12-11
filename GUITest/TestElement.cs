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

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);

            if (MouseSelect)
            {
                e.Framebuffer.Clear(new Colour(199, 144, 202));
                return;
            }

            if (MouseHover)
            {
                e.Framebuffer.Clear(new Colour(166, 188, 199));
                return;
            }

            e.Framebuffer.Clear(new Colour(255, 244, 233));
        }
    }
}
