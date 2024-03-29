﻿using Zene.Graphics;
using Zene.Windowing;
using Zene.Structs;
using System;

namespace PhysicsTest
{
    public class Program : Window
    {
        public static bool MultiThread { get; } = true;

        public static void Main()
        {
            Core.Init();

            //Program window = new Program(800, 500, "Work");
            SpaceWindow window = new SpaceWindow(800, 500, "Work");

            window.Run();

            Core.Terminate();
        }

        public Program(int width, int height, string title)
            : base(width, height, title)
        {
            _textDraw = new TextRenderer();
            _font = SampleFont.GetInstance();

            // Enabling transparency
            State.Blending = true;
            State.SourceScaleBlending = BlendFunction.SourceAlpha;
            State.DestinationScaleBlending = BlendFunction.OneMinusSourceAlpha;
        }

        private readonly TextRenderer _textDraw;
        private readonly Font _font;

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                _textDraw.Dispose();
            }
        }

        private double _count = 0;
        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);

            Framebuffer.Clear(BufferBit.Colour);

            _textDraw.DrawLeftBound($"Count:{_count:N3}", _font, 0, 0);
            _count += 0.001;
        }

        protected override void OnSizeChange(VectorIEventArgs e)
        {
            base.OnSizeChange(e);

            _textDraw.Projection = Matrix4.CreateOrthographic(e.X, e.Y, 0, -1);
            _textDraw.Model = Matrix4.CreateScale(30, 30, 1) * Matrix4.CreateTranslation(-e.X * 0.5, e.Y * 0.5, 0);

            Framebuffer.Viewport.Size = e.Value;
        }
    }
}
