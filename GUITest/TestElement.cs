using System;
using Zene.Graphics;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace GUITest
{
    class TestElement : ParentElement<FixedLayout>
    {
        public TestElement()
            //: base(new Layout(0d, 0d, 1d, 1d))
            : base(new FixedLayout(0d, 0d, 400d, 250d))
        {
            _g = new Renderer(this);
        }

        private readonly Renderer _g;
        public override GraphicsManager Graphics => _g;

        private Colour _c;
        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);

            _c = new Colour(166, 188, 199);

            if (MouseSelect)
            {
                _c = new Colour(255, 244, 233);
                Layout.Size = (410d, 260d);
            }
            else if (MouseHover)
            {
                _c = new Colour(199, 144, 202);
                Layout.Size = (410d, 260d);
            }
            else
            {
                Layout.Size = (400d, 250d);
            }
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);

            if (this[Mods.Control])
            {
                _g._radius += e.DeltaY * 0.01;
                _g._radius = Math.Clamp(_g._radius, 0d, 0.5);
                return;
            }

            if (this[Mods.Shift])
            {
                ViewPan += (e.DeltaY * -10d, 0d);
                return;
            }

            if (this[Mods.Alt])
            {
                ViewScale += ViewScale * e.DeltaY * 0.1;
                return;
            }

            _g._borderWidth += e.DeltaY;
        }

        private class Renderer : GraphicsManager<TestElement>
        {
            public Renderer(TestElement source)
                : base(source)
            {
            }

            private readonly BorderShader _shader = BorderShader.GetInstance();

            private readonly Font f = SampleFont.GetInstance();

            public double _radius = 0.2;
            public double _borderWidth = 10;

            public override void OnRender(DrawManager context)
            {
                Colour bc = new Colour(100, 200, 97);

                if (Source.Focused)
                {
                    bc = new Colour(200, 100, 97);
                }

                context.Shader = _shader;

                // Set uniforms for Shader
                _shader.Size = Source.Size;
                _shader.BorderColour = bc;
                _shader.Radius = _radius;
                _shader.BorderWidth = _borderWidth;
                Size = Source.Size + _shader.BorderWidth;
                _shader.ColourSource = ColourSource.UniformColour;
                _shader.Colour = Source._c;

                context.Model = Matrix4.CreateScale(Source.Bounds.Size);
                context.Draw(Shapes.Square);

                TextRenderer.Colour = new ColourF(1f, 1f, 1f);
                TextRenderer.Model = Matrix4.CreateScale(10);
                //TextRenderer.DrawCentred(context, $"R:{_radius:N2}, B:{_borderWidth}", f, 0, 0);
                TextRenderer.DrawCentred(context, $"Hover:{Source.Hande.Hover}\nFocus:{Source.Hande.Focus}", f, 0, 10);
                //TextRenderer.DrawCentred(context, $"{Source.MouseLocation.ToString("N3")}", f, 0, 0);
            }
        }
    }
}
