using System;
using Zene.Graphics;
using Zene.Graphics.Base;
using Zene.Windowing;
using Zene.Structs;

namespace CSGL
{
    public class WindowTest : Window
    {
        public WindowTest(int width, int height, string title)
            : base(width, height, title)
        {
            SetUp();
        }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                _shader.Dispose();

                _drawObject.Dispose();
                _texture.Dispose();
            }
        }

        private readonly float[] vertData = new float[]
        {
            /*Vertex*/ 0f, 5f, 5f,      /*Tex Coord*/ 0f, 0f,
            /*Vertex*/ 0f, 5f, -5f,     /*Tex Coord*/ 1f, 0f,
            /*Vertex*/ 0f, -5f, -5f,    /*Tex Coord*/ 1f, 1f,
            /*Vertex*/ 0f, -5f, 5f,     /*Tex Coord*/ 0f, 1f
        };

        private readonly byte[] indices = new byte[]
        {
            0, 1, 2,
            2, 3, 0
        };

        private DrawObject<float, byte> _drawObject;

        private Texture2D _texture;

        private BasicShader _shader;

        private Room _room;

        protected virtual void SetUp()
        {
            _shader = new BasicShader();

            _texture = Texture2D.Create("Resources/CharacterLeftN.png", WrapStyle.EdgeClamp, TextureSampling.Nearest, false);

            _drawObject = new DrawObject<float, byte>(vertData, indices, 5, 0, AttributeSize.D3, BufferUsage.DrawFrequent);
            _drawObject.AddAttribute(ShaderLocation.TextureCoords, 3, AttributeSize.D2); // Texture Coordinates

            State.Blending = true;
            State.SourceScaleBlending = BlendFunction.SourceAlpha;
            State.DestinationScaleBlending = BlendFunction.OneMinusSourceAlpha;
            State.DepthTesting = true;

            _room = new Room();
            BaseFramebuffer.ClearColour = new ColourF(1f, 1f, 1f);
        }

        private Matrix3 rotationMatrix;

        private readonly double moveSpeed = 0.5;

        private Vector3 CameraPos = Vector3.Zero;

        private double pDir = 0;

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);

            rotationMatrix = Matrix3.CreateRotationY(rotateY);

            Vector3 cameraMove = new Vector3(0, 0, 0);

            if (_left) 
            {
                pDir = 0;
                cameraMove.X += moveSpeed;
            }
            if (_right) 
            {
                pDir = 0.5;
                cameraMove.X -= moveSpeed;
            }
            if (_forward)   { cameraMove.Z += moveSpeed; }
            if (_backward)  { cameraMove.Z -= moveSpeed; }
            if (_up)        { cameraMove.Y += moveSpeed; }
            if (_down)      { cameraMove.Y -= moveSpeed; }

            if (_lShift)    { cameraMove *= 2; }
            if (_lAltGoFast) { cameraMove *= 4; }

            CameraPos += cameraMove * rotationMatrix;

            _shader.Bind();

            _shader.Matrix2 = Matrix4.CreateTranslation(CameraPos) * Matrix4.CreateRotationY(rotateY) *
                Matrix4.CreateRotationX(Radian.Percent(-0.125 + 0.5));

            BaseFramebuffer.Clear(BufferBit.Colour | BufferBit.Depth);

            _shader.Matrix1 = Matrix.Identity;
            _shader.ColourSource = ColourSource.Texture;
            _shader.TextureSlot = Room.TexTexSlot;
            e.Context.Shader = _shader;
            e.Context.Render(_room);

            _shader.Matrix1 = Matrix4.CreateScale(0.25) * Matrix4.CreateRotationY(Radian.Percent(pDir)) * Matrix4.CreateRotationZ(-0.125) *
                Matrix4.CreateTranslation(-CameraPos + new Vector3(-5, 5, 0));
            _shader.ColourSource = ColourSource.Texture;
            _shader.TextureSlot = 0;

            _texture.Bind(0);
            e.Context.Draw(_drawObject);
            _texture.Unbind();
        }

        private bool _left;
        private bool _right;
        private bool _up;
        private bool _down;
        private bool _forward;
        private bool _backward;
        private bool _lShift;
        private bool _lAltGoFast;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e[Keys.LeftShift])
            {
                _lShift = true;
                return;
            }
            if (e[Keys.LeftAlt])
            {
                _lAltGoFast = true;
                return;
            }
            if (e[Keys.LeftControl])
            {
                _down = true;
                return;
            }
            if (e[Keys.S])
            {
                _backward = true;
                return;
            }
            if (e[Keys.W])
            {
                _forward = true;
                return;
            }
            if (e[Keys.A])
            {
                _left = true;
                return;
            }
            if (e[Keys.D])
            {
                _right = true;
                return;
            }
            if (e[Keys.Space])
            {
                _up = true;
                return;
            }
            if (e[Keys.Escape])
            {
                Close();
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
                _lShift = false;
                return;
            }
            if (e[Keys.LeftAlt])
            {
                _lAltGoFast = false;
                return;
            }
            if (e[Keys.LeftControl])
            {
                _down = false;
                return;
            }
            if (e[Keys.S])
            {
                _backward = false;
                return;
            }
            if (e[Keys.W])
            {
                _forward = false;
                return;
            }
            if (e[Keys.A])
            {
                _left = false;
                return;
            }
            if (e[Keys.D])
            {
                _right = false;
                return;
            }
            if (e[Keys.Space])
            {
                _up = false;
                return;
            }
        }

        protected override void OnSizeChange(VectorIEventArgs e)
        {
            base.OnSizeChange(e);

            _shader.Matrix3 = Matrix4.CreatePerspectiveFieldOfView(Radian.Degrees(65), (double)e.X / e.Y, 0.1, 1000);
        }

        private Vector2 _mouseLocation = Vector2.Zero;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (CursorMode != CursorMode.Disabled) { return; }

            if (new Vector2(e.X, e.Y) == _mouseLocation) { return; }

            double distanceX = e.X - _mouseLocation.X;
            double distanceY = _mouseLocation.Y - e.Y;

            _mouseLocation = new Vector2(e.X, e.Y);

            rotateY += Radian.Degrees(distanceX * 0.1);
            //rotateX += Radian.Degrees(distanceY * 0.1);
        }

        private Radian rotateY = 0;
    }
}
