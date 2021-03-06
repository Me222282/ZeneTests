using System;
using Zene.Graphics.Base;
using Zene.Graphics;
using Zene.Windowing;
using Zene.Graphics.Shaders;
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

            OnSizeChange(new SizeChangeEventArgs(width, height));
        }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                Shader.Dispose();

                DrawObject.Dispose();
                PointObject.Dispose();
            }
        }

        private readonly float[] vertData = new float[]
        {
            /*Vertex*/ 0.5f, 0.5f, /*-20f, /*Colour*/ 0.0f, 0.0f, 1.0f,
            /*Vertex*/ 0.5f, -0.5f, /*-20f, /*Colour*/ 0.0f, 1.0f, 0.0f,
            //*Vertex*/ 0.5f, -0.5f, /*-20f, /*Colour*/ 0.0f, 1.0f, 0.0f,
            /*Vertex*/ -0.5f, -0.5f, /*-20f, /*Colour*/ 1.0f, 0.0f, 0.0f,
            /*Vertex*/ -0.5f, 0.5f, /*-20f, /*Colour*/ 1.0f, 1.0f, 0.0f
        };

        private readonly byte[] indices = new byte[]
        {
            0, 1, 2,
            2, 3, 0
        };

        private DrawObject<float, byte> DrawObject;

        private DrawingTexture<Vector2> PointObject;

        private BasicShader Shader;

        //private readonly Random r = new Random();

        private Matrix4 matrix;

        private double scale;

        private double orthoWidth;
        private double orthoHeight;

        protected virtual void SetUp()
        {
            Shader = new BasicShader();
            
            DrawObject = new DrawObject<float, byte>(vertData, indices, 5, 0, AttributeSize.D2, BufferUsage.DrawFrequent);

            DrawObject.AddAttribute(1, 2, AttributeSize.D3); // Colour attribute

            State.Blending = true;
            GL.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            PointObject = new DrawingTexture<Vector2>(new Vector2[] 
            {
                new Vector2(-0.628, -0.75),
                new Vector2(-0.75, -0.872),
                new Vector2(-0.872, -0.75),
                new Vector2(-0.75, -0.628)
            }, 2, new Bitmap("Resources/CharacterLeftN.png"), WrapStyle.EdgeClamp, TextureSampling.Nearest, BufferUsage.DrawFrequent, false);

            scale = 100;

            matrix = Matrix4.Identity;

            //GL.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);
        }

        protected override void OnUpdate(EventArgs e)
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

            Shader.Bind();

            Shader.Matrix1 = matrix * Matrix4.CreateScale(scale) * Matrix4.CreateOrthographic(orthoWidth, orthoHeight, 10, 0);
            //Shader.Matrix1 = Matrix4.CreateTranslation(0, 0, scale) * Matrix4.CreatePerspectiveFieldOfView(Radian.Degrees(60), orthoWidth / orthoHeight, 1, 100);

            Shader.SetColourSource(ColourSource.AttributeColour);

            DrawObject.Draw();

            Shader.SetColourSource(ColourSource.Texture);
            Shader.SetTextureSlot(0);

            PointObject.Draw();
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

        protected override void OnSizeChange(SizeChangeEventArgs e)
        {
            base.OnSizeChange(e);

            double mWidth;
            double mHeight;

            if (e.Width > e.Height)
            {
                double heightPercent = (double)e.Height / e.Width;

                mWidth = 1600;

                mHeight = 1600 * heightPercent;
            }
            else
            {
                double widthPercent = (double)e.Width / e.Height;

                mHeight = 900;

                mWidth = 900 * widthPercent;
            }

            orthoWidth = mWidth;
            orthoHeight = mHeight;
        }

        protected override void OnSizePixelChange(SizeChangeEventArgs e)
        {
            base.OnSizePixelChange(e);

            BaseFramebuffer.ViewSize = new Vector2I(e.Width, e.Height);
        }
    }
}
