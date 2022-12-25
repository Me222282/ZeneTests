using System;
using Zene.Graphics;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace GUITest
{
    class TestElement : Element
    {
        public TestElement()
            //: base(new Layout(0d, 0d, 1d, 1d))
            : base(new Box((0d, 0d), (400d, 250d)))
        {
            _shader = BorderShader.GetInstance();
        }

        private BorderShader _shader;

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
                Size = (410d, 260d);
            }
            else if (MouseHover)
            {
                c = new Colour(199, 144, 202);
                Size = (410d, 260d);
            }
            else
            {
                Size = (400d, 250d);
            }

            // Set uniforms for Shader
            _shader.Size = Size;
            _shader.BorderColour = new Colour(100, 200, 97);
            _shader.Radius = _radius;
            _shader.BorderWidth = _borderWidth;
            DrawingBoundOffset = _shader.BorderWidth;
            _shader.ColourSource = ColourSource.UniformColour;
            _shader.Colour = c;
            _shader.Matrix1 = Matrix4.CreateScale(Bounds.Size);
            _shader.Matrix2 = Matrix4.Identity;
            _shader.Matrix3 = Projection;

            Shapes.Square.Draw();

            TextRenderer.Model = Matrix4.CreateScale(10);
            TextRenderer.DrawCentred($"R:{_radius:N2}, B:{_borderWidth}", f, 0, 0);
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

            if (this[Mods.Shift])
            {
                ViewPan += (e.DeltaY * -10d, 0d);
                return;
            }

            _borderWidth += e.DeltaY;
        }
    }
}
