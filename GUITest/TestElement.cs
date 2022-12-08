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
            Shader = new BasicShader();
        }

        public override IBasicShader Shader { get; }

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);

            if (MouseHover)
            {
                e.Framebuffer.Clear(new Colour(166, 188, 199));
                return;
            }

            e.Framebuffer.Clear(new Colour(255, 244, 233));
        }
    }
}
