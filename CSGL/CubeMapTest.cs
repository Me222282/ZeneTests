using Zene.Graphics;
using Zene.Windowing;
using Zene.Structs;
using System;

namespace CSGL
{
    public class CubeMapTest : Window
    {
        public CubeMapTest(int width, int height, string title)
            : base(width, height, title)
        {
            _shader = new SkyBoxShader();

            _drawObject = new DrawObject<float, byte>(
                new float[]
                {
                    -1.0f, 1.0f, 1.0f,     // 0
                    1.0f, 1.0f, 1.0f,      // 1
                    1.0f, -1.0f, 1.0f,     // 2
                    -1.0f, -1.0f, 1.0f,    // 3

                    -1.0f, 1.0f, -1.0f,    // 4
                    1.0f, 1.0f, -1.0f,     // 5
                    1.0f, -1.0f, -1.0f,    // 6
                    -1.0f, -1.0f, -1.0f    // 7
                },
                new byte[]
                {
                    // Front
                    0, 3, 2,
                    2, 1, 0,

                    // Back
                    5, 6, 7,
                    7, 4, 5,

                    // Left
                    4, 7, 3,
                    3, 0, 4,

                    // Right
                    1, 2, 6,
                    6, 5, 1,

                    // Top
                    4, 0, 1,
                    1, 5, 4,

                    // Bottom
                    2, 3, 7,
                    7, 6, 2
                }, 3, 0, AttributeSize.D3, BufferUsage.DrawFrequent);

            Bitmap.AutoFlipTextures = false;

            /*
            Bitmap skyBox = new Bitmap("Resources/StandardCubeMap.jpg");
            int faceSize = skyBox.Width / 4;
            _cubeMap = CubeMap.Create(new Bitmap[]
            {
                skyBox.SubBitmap(faceSize * 2, faceSize, faceSize, faceSize),   // Right
                skyBox.SubBitmap(0, faceSize, faceSize, faceSize),              // Left
                skyBox.SubBitmap(faceSize, 0, faceSize, faceSize),              // Top
                skyBox.SubBitmap(faceSize, faceSize * 2, faceSize, faceSize),   // Bottom
                skyBox.SubBitmap(faceSize, faceSize, faceSize, faceSize),       // Front
                skyBox.SubBitmap(faceSize * 3, faceSize, faceSize, faceSize)    // Back
            }, WrapStyle.EdgeClamp, TextureSampling.Nearest, false);
            *//*
            Bitmap skyBox = new Bitmap("Resources/CubeMapHD.png");
            Console.WriteLine("Texture Decoded.");
            Console.WriteLine(test[0, 0]);

            _cubeMap = CubeMap.Create(new Bitmap[]
            {
                skyBox.SubBitmap(faceSize * 1, 0, faceSize, faceSize),      // Right
                skyBox.SubBitmap(faceSize * 3, 0, faceSize, faceSize),      // Left
                skyBox.SubBitmap(faceSize * 4, 0, faceSize, faceSize),      // Top
                skyBox.SubBitmap(faceSize * 5, 0, faceSize, faceSize),      // Bottom
                skyBox.SubBitmap(faceSize * 0, 0, faceSize, faceSize),      // Front
                skyBox.SubBitmap(faceSize * 2, 0, faceSize, faceSize)       // Back
            }, WrapStyle.EdgeClamp, TextureSampling.Blend, false);
            Console.WriteLine("Texture Loaded.");*/
            /*
            int faceSize = 2048;
            _cubeMap = CubeMap.LoadSync("Resources/CubeMapHD.png", new RectangleI[]
            {
                new RectangleI(faceSize * 1, faceSize, faceSize, faceSize),    // Right
                new RectangleI(faceSize * 3, faceSize, faceSize, faceSize),    // Left
                new RectangleI(faceSize * 4, faceSize, faceSize, faceSize),    // Top
                new RectangleI(faceSize * 5, faceSize, faceSize, faceSize),    // Bottom
                new RectangleI(faceSize * 0, faceSize, faceSize, faceSize),    // Front
                new RectangleI(faceSize * 2, faceSize, faceSize, faceSize),    // Back
            }, WrapStyle.EdgeClamp, TextureSampling.Blend, false);*/

            
            _cubeMap = CubeMap.LoadAsync(new string[]
            {
                "Resources/cubeMaps/Storforsen4/posx.jpg",  // Right
                "Resources/cubeMaps/Storforsen4/negx.jpg",  // Left
                "Resources/cubeMaps/Storforsen4/posy.jpg",  // Top
                "Resources/cubeMaps/Storforsen4/negy.jpg",  // Bottom
                "Resources/cubeMaps/Storforsen4/posz.jpg",  // Front
                "Resources/cubeMaps/Storforsen4/negz.jpg"   // Back
            }, 2048, WrapStyle.EdgeClamp, TextureSampling.Blend, true);

            State.SeamlessCubeMaps = true;
            State.DepthTesting = true;

            // Hide mouse
            CursorMode = CursorMode.Disabled;
        }

        private readonly DrawObject<float, byte> _drawObject;
        private readonly CubeMap _cubeMap;
        private readonly SkyBoxShader _shader;

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                _cubeMap.Dispose();
                _shader.Dispose();
                _drawObject.Dispose();
            }
        }

        private Vector3 _offset = Vector3.Zero;
        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);

            Framebuffer.Clear(BufferBit.Colour | BufferBit.Depth);

            Vector3 cameraMove = new Vector3(0, 0, 0);

            if (_w)
            {
                cameraMove.Z -= 0.025f;
            }
            if (_s)
            {
                cameraMove.Z += 0.025f;
            }
            if (_a)
            {
                cameraMove.X += 0.025f;
            }
            if (_d)
            {
                cameraMove.X -= 0.025f;
            }
            if (_space)
            {
                cameraMove.Y -= 0.025f;
            }
            if (_ctrl)
            {
                cameraMove.Y += 0.025f;
            }

            IMatrix rotationMatrix = Matrix3.CreateRotationY(rotateY) * Matrix3.CreateRotationX(rotateX);

            _offset += cameraMove * rotationMatrix;

            _shader.Bind();
            _shader.Texture = _cubeMap;

            e.Context.Shader = _shader;
            e.Context.View = Matrix4.CreateTranslation(_offset) * Matrix4.CreateRotationY(rotateY) * Matrix4.CreateRotationX(rotateX);
            e.Context.Draw(_drawObject);
        }

        private float _zoom = 60;
        private const float _near = 0.00001f;
        private const float _far = 10;
        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);

            _zoom -= e.DeltaY;

            if (_zoom >= 179)
            {
                _zoom = 179;
            }

            else if (_zoom <= 1)
            {
                _zoom = 1;
            }

            DrawContext.Projection = Matrix4.CreatePerspectiveFieldOfView(Radian.Degrees(_zoom), (float)Width / Height, _near, _far);
        }

        protected override void OnSizeChange(VectorIEventArgs e)
        {
            base.OnSizeChange(e);

            DrawContext.Projection = Matrix4.CreatePerspectiveFieldOfView(Radian.Degrees(_zoom), (float)e.X / e.Y, _near, _far);
        }
        protected override void OnSizePixelChange(VectorIEventArgs e)
        {
            base.OnSizePixelChange(e);

            // Invalide size
            if (e.X <= 0 || e.Y <= 0) { return; }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

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

            if (e[Keys.M])
            {
                if (CursorMode != CursorMode.Normal)
                {
                    CursorMode = CursorMode.Normal;
                    return;
                }

                CursorMode = CursorMode.Disabled;
                return;
            }

            if (e[Keys.Enter])
            {
                _offset = Vector3.Zero;
                return;
            }

            if (e[Keys.W])
            {
                _w = true;
                return;
            }
            if (e[Keys.S])
            {
                _s = true;
                return;
            }
            if (e[Keys.A])
            {
                _a = true;
                return;
            }
            if (e[Keys.D])
            {
                _d = true;
                return;
            }
            if (e[Keys.Space])
            {
                _space = true;
                return;
            }
            if (e[Keys.LeftControl])
            {
                _ctrl = true;
                return;
            }
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e[Keys.W])
            {
                _w = false;
                return;
            }
            if (e[Keys.S])
            {
                _s = false;
                return;
            }
            if (e[Keys.A])
            {
                _a = false;
                return;
            }
            if (e[Keys.D])
            {
                _d = false;
                return;
            }
            if (e[Keys.Space])
            {
                _space = false;
                return;
            }
            if (e[Keys.LeftControl])
            {
                _ctrl = false;
                return;
            }
        }
        private bool _w;
        private bool _s;
        private bool _a;
        private bool _d;
        private bool _space;
        private bool _ctrl;

        private Radian rotateX = 0;
        private Radian rotateY = 0;

        private Vector2 _mouseLocation = Vector2.Zero;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (CursorMode != CursorMode.Disabled) { return; }

            if (new Vector2(e.X, e.Y) == _mouseLocation) { return; }

            float distanceX = e.X - _mouseLocation.X;
            float distanceY = e.Y - _mouseLocation.Y;

            _mouseLocation = new Vector2(e.X, e.Y);

            rotateY -= Radian.Degrees(distanceX * 0.1f);
            rotateX += Radian.Degrees(distanceY * 0.1f);
        }
    }
}
