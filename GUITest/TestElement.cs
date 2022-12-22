using System;
using Zene.Graphics;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace GUITest
{
    class TestElement : Element
    {
        public TestElement(ILayout layout)
            : base(layout)
        {
            Shader = BorderShader.GetInstance();
        }

        public override BorderShader Shader { get; }

        private readonly Font f = SampleFont.GetInstance();

        private double _radius = 0.2;
        private double _borderWidth = 10;

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);

            Colour c = new Colour(166, 188, 199);

            if (MouseSelect)
            {
                c = new Colour(255, 244, 233);
            }
            else if (MouseHover)
            {
                c = new Colour(199, 144, 202);
            }

            e.Framebuffer.Clear(c);

            TextRenderer.Model = Matrix4.CreateScale(10);
            TextRenderer.DrawCentred($"R:{Shader.Radius:N2}, B:{Shader.BorderWidth}", f, 0, 0);

            // Set uniforms for Shader
            Shader.Size = Size;
            Shader.BorderColour = new Colour(100, 200, 97);
            Shader.Radius = _radius;
            Shader.BorderWidth = _borderWidth;
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);

            if (this[Mods.Control])
            {
                _radius += e.DeltaY * 0.01;
                _radius = Math.Clamp(_radius, 0d, 0.5);
                return;
            }

            _borderWidth += e.DeltaY;
        }
    }
}
