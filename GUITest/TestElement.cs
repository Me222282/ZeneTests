using System;
using Zene.Graphics;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace GUITest
{
    class TestElement : Element
    {
        public TestElement(IBox bounds)
            : base(bounds)
        {
            Shader = new BorderShader
            {
                Radius = 0.2,
                BorderWidth = 10,
                BorderColour = new Colour(100, 200, 97)
            };
        }

        public override BorderShader Shader { get; }

        private readonly Font f = new SampleFont();

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

            TextRenderer.Model = Matrix4.CreateScale(10);
            TextRenderer.DrawCentred($"R:{Shader.Radius:N2}, B:{Shader.BorderWidth}", f, 0, 0);
        }

        protected override void OnSizeChange(SizeChangeEventArgs e)
        {
            base.OnSizeChange(e);

            Actions.Push(() =>
            {
                Shader.Size = e.Size;
            });
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);

            if (this[Mods.Control])
            {
                Actions.Push(() =>
                {
                    Shader.Radius += e.DeltaY * 0.01;
                });
                return;
            }

            Actions.Push(() =>
            {
                Shader.BorderWidth += e.DeltaY;
            });
        }
    }
}
