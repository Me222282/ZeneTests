using System;
using Zene.Graphics.Base;
using Zene.Graphics;
using Zene.Windowing;
using Zene.Structs;

namespace CSGL
{
    public unsafe class Window2D : Window
    {
        public Window2D(int width, int height, string title)
            : base(width, height, title, new WindowInitProperties()
            {
                TransparentFramebuffer = true
            })
        {
            SetUp();
        }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                Shader.Dispose();

                DrawObject.Dispose();
            }
        }

        private readonly float[] vertData = new float[]
        {
            /*Vertex*/ 0.5f, 0.5f, /*-20f, /*Colour*/ 0f, 0f, 1f,
            /*Vertex*/ 0.5f, -0.5f, /*-20f, /*Colour*/ 0f, 1f, 0f,
            //*Vertex*/ 0.5f, -0.5f, /*-20f, /*Colour*/ 0f, 1f, 0f,
            /*Vertex*/ -0.5f, -0.5f, /*-20f, /*Colour*/ 1f, 0f, 0f,
            /*Vertex*/ -0.5f, 0.5f, /*-20f, /*Colour*/ 1f, 1f, 0f
        };

        private readonly byte[] indices = new byte[]
        {
            0, 1, 2,
            2, 3, 0
        };

        private DrawObject<float, byte> DrawObject;

        private BasicShader Shader;

        //private readonly Random r = new Random();

        private IMatrix matrix;

        private float scale;

        private float orthoWidth;
        private float orthoHeight;

        protected virtual void SetUp()
        {
            Shader = new BasicShader();
            
            DrawObject = new DrawObject<float, byte>(vertData, indices, 5, 0, AttributeSize.D2, BufferUsage.DrawFrequent);

            DrawObject.AddAttribute(1, 2, AttributeSize.D3); // Colour attribute

            State.Blending = true;
            State.SourceScaleBlending = BlendFunction.SourceAlpha;
            State.DestinationScaleBlending = BlendFunction.OneMinusSourceAlpha;

            scale = 100;

            matrix = Matrix.Identity;

            //GL.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);
        }

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);

            BaseFramebuffer.ClearColour = new ColourF(0.2f, 0.4f, 0.8f, 0.5f);
            //BaseFramebuffer.ClearColour = new ColourF(0.2f, 0.4f, 0.8f, 1.0f);
            BaseFramebuffer.Clear(BufferBit.Colour);
            
            if (_rightShift || _leftShift)
            {
                scale += 1;
            }
            if (_leftControl || _rightControl)
            {
                scale -= 1;
            }

            e.Context.Model = matrix * Matrix4.CreateScale(scale) * Matrix4.CreateOrthographic(orthoWidth, orthoHeight, 10, 0);
            //Shader.Matrix1 = Matrix4.CreateTranslation(0, 0, scale) * Matrix4.CreatePerspectiveFieldOfView(Radian.Degrees(60), orthoWidth / orthoHeight, 1, 100);

            Shader.ColourSource = ColourSource.AttributeColour;
            e.Context.Shader = Shader;
            e.Context.Draw(DrawObject);

            e.Context.RenderState.PostMatrixMods = false;
            e.Context.DrawTriangle((-0.2, 0.3), 1d, (0.5, -0.6), ColourF.Lime);
        }

        private bool _leftShift;
        private bool _rightShift;
        private bool _leftControl;
        private bool _rightControl;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e[Keys.LeftShift])
            {
                _leftShift = true;
                return;
            }
            if (e[Keys.RightShift])
            {
                _rightShift = true;
                return;
            }
            if (e[Keys.LeftControl])
            {
                _leftControl = true;
                return;
            }
            if (e[Keys.RightControl])
            {
                _rightControl = true;
                return;
            }
            if (e[Keys.Tab])
            {
                FullScreen = !FullScreen;
                return;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e[Keys.LeftShift])
            {
                _leftShift = false;
                return;
            }
            if (e[Keys.RightShift])
            {
                _rightShift = false;
                return;
            }
            if (e[Keys.LeftControl])
            {
                _leftControl = false;
                return;
            }
            if (e[Keys.RightControl])
            {
                _rightControl = false;
                return;
            }
        }

        protected override void OnSizeChange(VectorIEventArgs e)
        {
            base.OnSizeChange(e);

            float mWidth;
            float mHeight;

            if (e.X > e.Y)
            {
                float heightPercent = (float)e.Y / e.X;

                mWidth = 1600;

                mHeight = 1600 * heightPercent;
            }
            else
            {
                float widthPercent = (float)e.X / e.Y;

                mHeight = 900;

                mWidth = 900 * widthPercent;
            }

            orthoWidth = mWidth;
            orthoHeight = mHeight;
        }
    }
}
