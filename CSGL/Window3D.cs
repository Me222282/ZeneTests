using System;
using Zene.Graphics.Z3D;
using Zene.Graphics;
using Zene.Windowing;
using Zene.Structs;
using System.Collections.Generic;

namespace CSGL
{
    public unsafe class Window3D : Window
    {
        public Window3D(int width, int height, string title)
            : base(width, height, title, true)
        {
            _context = new PostProcessing(width, height)
            {
                //Pixelated = true,
                //UseKernel = true,
                Kernel = PostShader.SharpenKernel,
                KernelOffset = 200
            };
            _perspective = new PerspectiveMatrix(Radian.Degrees(60), (float)width / height, 0.1f, 3000);
            _context.Projection = _perspective;

            AddWindowFollower(_perspective);
            AddWindowFollower(_context);

            SetUp();

            //CursorMode = CursorMode.Disabled;

            State.OutputDebug = false;
        }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                _shader.Dispose();
                Framebuffer.Dispose();

                _drawObject.Dispose();

                _floor.Dispose();
                _floorTexture.Dispose();
                _floorNormalMap.Dispose();

                _lightObject.Dispose();

                _farLightObject.Dispose();

                _room.Dispose();

                _loadObject.Dispose();
            }
        }

        private readonly List<string> _runTimeLog = new List<string>(new string[] { $"{DateTime.Now}\n" });

        protected override void GLError(uint type, string message)
        {
            base.GLError(type, message);

            if (type == Zene.Graphics.Base.GLEnum.DebugTypeError && message != null)
            {
                _runTimeLog.Add($"GL Output: {message}");
            }
        }

        protected override void OnStart(EventArgs e)
        {
            base.OnStart(e);

            Timer = 0d;
        }
        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);

            State.ClearErrors();
            //e.Context.Framebuffer.Clear(BufferBit.Colour);

            Draw(_context);

            e.Context.Render(_context);

            _fpsCounter++;

            double time = Timer;

            if (time >= 10)
            {
                _runTimeLog.Add($"FPS:{_fpsCounter / time}");

                _fpsCounter = 0;
                Timer = 0;
            }
        }
        protected override void OnStop(EventArgs e)
        {
            base.OnStop(e);

            System.IO.File.WriteAllLines("runtimeLog.txt", _runTimeLog);
        }

        private readonly PostProcessing _context;

        private static readonly Vector3 _red = new Vector3(1, 0, 0);
        private static readonly Vector3 _green = new Vector3(0, 1, 0);
        private static readonly Vector3 _blue = new Vector3(0, 0, 1);
        private static readonly Vector3 _yellow = new Vector3(1, 1, 0);

        private readonly Vector3[] _vertData = new Vector3[]
        {
            /*Vertex*/ new Vector3(-5, 5, 5), _red,
            /*Vertex*/ new Vector3(5, 5, 5), _green,
            /*Vertex*/ new Vector3(5, -5, 5), _blue,
            /*Vertex*/ new Vector3(-5, -5, 5), _yellow,

            /*Vertex*/ new Vector3(-5, 5, -5), _blue,
            /*Vertex*/ new Vector3(5, 5, -5), _yellow,
            /*Vertex*/ new Vector3(5, -5, -5), _red,
            /*Vertex*/ new Vector3(-5, -5, -5), _green
        };
        private readonly uint[] _indexData = new uint[]
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
        };

        private LightingShader _shader;

        private DrawObject<Vector3, uint> _drawObject;
        private float _objectRotation = 0.001f;

        private Material _objectMaterial = new Material(Material.Source.Default, Material.Source.None, Shine.None);

        private Object3D _loadObject;
        private Texture2D _loadObjectImage;

        private DrawObject<Vector3, uint> _floor;

        private Texture2D _floorTexture;
        private Texture2D _floorNormalMap;

        private Material _floorMaterial = new Material(Material.Source.Default, Material.Source.Default, Shine.L);

        private Light _lightObject;
        private Light _farLightObject;

        private Zene.Graphics.Light _light;
        private Zene.Graphics.Light _farLight;

        private float _lightAmplitude = 0;

        private Room _room;

        private TextRenderer _textDisplay;
        private Font _font;

        protected virtual void SetUp()
        {
            _shader = new LightingShader(4, 1);

            Object3D.AddNormals(_vertData, 2, _indexData, out List<Vector3> vertices, out List<uint> indices);

            _drawObject = new DrawObject<Vector3, uint>(vertices.ToArray(), indices.ToArray(), 3, 0, AttributeSize.D3, BufferUsage.DrawFrequent);

            _drawObject.AddAttribute(ShaderLocation.Colour, 1, AttributeSize.D3); // Colour attribute
            _drawObject.AddAttribute(ShaderLocation.Normal, 2, AttributeSize.D3); // Normals

            Object3D.AddNormalTangents(new Vector3[] { new Vector3(-500, 10, -500), new Vector3(0, 0, 0),
                new Vector3(500, 10, -500), new Vector3(100, 0, 0),
                new Vector3(500, 10, 500), new Vector3(100, 100, 0),
                new Vector3(-500, 10, 500), new Vector3(0, 100, 0)}, 2, 1, new uint[] { 0, 1, 2, 2, 3, 0 }, out List<Vector3> floorVerts, out List<uint> floorInds);

            _floor = new DrawObject<Vector3, uint>(floorVerts.ToArray(), floorInds.ToArray(), 4, 0, AttributeSize.D3, BufferUsage.DrawFrequent);

            _floorTexture = Texture2D.LoadAsync("Resources/wood.png", WrapStyle.Repeated, TextureSampling.BlendMipMapBlend, true);

            _floorNormalMap = Texture2D.LoadAsync("Resources/woodNor.png", WrapStyle.Repeated, TextureSampling.BlendMipMapBlend, true);

            _floor.AddAttribute(ShaderLocation.TextureCoords, 1, AttributeSize.D3); // Texture Coordinates
            _floor.AddAttribute(ShaderLocation.NormalTexture, 1, AttributeSize.D3); // Normal Map Coordinates
            _floor.AddAttribute(ShaderLocation.Normal, 2, AttributeSize.D3); // Normals
            _floor.AddAttribute(ShaderLocation.Tangent, 3, AttributeSize.D3); // Tangents

            //GL.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);

            _rotationMatrix = Matrix3.CreateRotationX(0);

            _light = new Zene.Graphics.Light(new Colour3(255, 255, 255), Colour3.Zero, 0.0014f, 0.000007f, new Vector3(10, 0, 10));
            _shader.SetLight(0, _light);

            _farLight = new Zene.Graphics.Light(new Colour3(200, 200, 200), Colour3.Zero, 0.014f, 0.0007f, new Vector3(500, 0, 500));
            _shader.SetLight(1, _farLight);

            _shader.AmbientLight = new Colour(12, 12, 15);

            _lightObject = new Light(Vector3.Zero, 0.5, BufferUsage.DrawFrequent);

            _farLightObject = new Light(new Vector3(500, 0, 500), 0.5, BufferUsage.DrawFrequent);

            _cameraLightCC = _cameraLight = new Colour(120, 110, 100);
            _shader.SetSpotLight(0, new SpotLight((Colour3)_cameraLight, Radian.Degrees(22.5f), Radian.Degrees(40), 0.0045f, 0.00075f, Vector3.Zero, new Vector3(0, 0, 1)));

            _room = new Room(_shader);

            _shader.IngorBlackLight = true;

            _loadObject = Object3D.FromObjNT("Resources/Sphere.obj");

            _textDisplay = new TextRenderer();
            _font = new SampleFont();

            _loadObjectImage = Texture2D.Create(new GLArray<Colour>(1, 1, 1, new Colour(134, 94, 250)), WrapStyle.EdgeClamp, TextureSampling.Blend, false);
        }

        private readonly Matrix3 _lightRotation = Matrix3.CreateRotationY(Radian.Percent(0.001f));

        private Vector3 _cameraPos = Vector3.Zero;

        private Radian _rotateX = Radian.Percent(0.5f);
        private Radian _rotateY = 0;
        private Radian _rotateZ = 0;

        private Matrix3 _rotationMatrix;
        private readonly PerspectiveMatrix _perspective;

        private Colour _cameraLight;
        private float _moveSpeed = 1;

        protected virtual void Draw(IDrawingContext context)
        {
            _rotationMatrix = Matrix3.CreateRotationX(-_rotateX) * Matrix3.CreateRotationY(-_rotateY);

            Vector3 cameraMove = new Vector3(0, 0, 0);

            if (this[Keys.A])       { cameraMove.X += _moveSpeed; }
            if (this[Keys.D])       { cameraMove.X -= _moveSpeed; }
            if (this[Keys.W])       { cameraMove.Z -= _moveSpeed; }
            if (this[Keys.S])       { cameraMove.Z += _moveSpeed; }
            if (this[Keys.Space])   { cameraMove.Y -= _moveSpeed; }
            if (this[Mods.Control]) { cameraMove.Y += _moveSpeed; }

            if (this[Keys.LeftShift])   { cameraMove *= 2; }
            if (this[Keys.RightShift])  { cameraMove *= 0.25; }
            if (this[Mods.Alt])         { cameraMove *= 4; }

            _cameraPos += cameraMove * _rotationMatrix;

            _shader.CameraPosition = -_cameraPos;

            Vector3 lv3 = (Vector3)_light.LightVector;
            lv3 *= _lightRotation;
            _light.LightVector = (lv3, _light.LightVector.W);

            _shader.SetLightPosition(0, _light.LightVector);

            context.View = Matrix4.CreateTranslation(_cameraPos) * Matrix4.CreateRotationY(_rotateY) *
                Matrix4.CreateRotationX(_rotateX) * Matrix4.CreateRotationZ(_rotateZ);

            _shader.SetSpotLightPosition(0, -_cameraPos);
            _shader.SetSpotLightDirection(0, new Vector3(0, 0, -1) *
                Matrix3.CreateRotationX(-_rotateX) * Matrix3.CreateRotationY(-_rotateY) * Matrix3.CreateRotationZ(_rotateZ));

            //IFrameBuffer.ClearColour(new ColourF(0.2f, 0.4f, 0.8f, 1.0f));
            context.Framebuffer.Clear(BufferBit.Colour | BufferBit.Depth);

            _shader.NormalMapping = false;
            _shader.DrawLighting = _doLight;
            context.Model = Matrix4.CreateRotationX(Radian.Percent(_objectRotation));
            _objectRotation += 0.001f;
            _shader.ColourSource = ColourSource.AttributeColour;
            _shader.Material = _objectMaterial;

            context.Shader = _shader;
            context.Draw(_drawObject);

            _shader.DrawLighting = _doLight;
            context.Model = Matrix4.CreateRotationZ(Radian.Percent(0.5f)) * Matrix4.CreateRotationY(Radian.Percent(0.25f)) * Matrix4.CreateTranslation(100, 0, 0);
            //Shader.SetColourSource(ColourSource.UniformColour);
            _shader.ColourSource = ColourSource.Texture;
            _shader.Colour = new Colour(134, 94, 250);

            _shader.Texture = _loadObjectImage;
            context.Draw(_loadObject);

            _shader.ColourSource = ColourSource.Texture;
            context.Model = Matrix.Identity;
            _shader.Material = _floorMaterial;
            _shader.NormalMapping = true;

            _shader.Texture = _floorTexture;
            _shader.NormalMap = _floorNormalMap;
            context.Draw(_floor);

            _shader.NormalMapping = false;
            _shader.DrawLighting = false;
            context.Model = Matrix4.CreateTranslation((Vector3)_light.LightVector);
            _shader.ColourSource = ColourSource.UniformColour;
            _shader.Colour = (ColourF)_light.LightColour;
            context.Draw(_lightObject);

            float lA = MathF.Sin(Radian.Percent(_lightAmplitude));
            _lightAmplitude += 0.005f;

            byte colourValue = (byte)(255 - (int)((lA + 1) * 100));
            _farLight.LightColour = new Colour3(colourValue, colourValue, colourValue);

            //float lightScale = (3 - ((lA + 1) * 1.5)) + 1;

            float lightSizeL = ((lA + 1) * 4.5f) + 1;
            float lightSizeQ = ((lA + 1) * 4.5f) + 1;
            _farLight.Linear = 0.0014f * lightSizeL;
            _farLight.Quadratic = 0.000007f * lightSizeQ;

            _shader.SetLight(1, _farLight);

            context.Model = Matrix.Identity;
            _shader.ColourSource = ColourSource.UniformColour;
            _shader.Colour = (ColourF)_farLight.LightColour;
            context.Draw(_farLightObject);

            _shader.DrawLighting = _doLight;
            context.Model = Matrix4.CreateTranslation(8000, 0, 0);
            context.Render(_room, _shader);

            context.Model = Matrix4.CreateTranslation(0, 0, -5.1f) * Matrix4.CreateRotationX(Radian.Percent(_objectRotation));
            _textDisplay.DrawCentred(context, $"{Core.Time:N3}\n", _font, 0, 0);
            _textDisplay.DrawCentred(context, $"\n{_cameraPos.SquaredLength:N3}", _font, 0, 0);
        }

        private int _fpsCounter = 0;

        private bool _torchLight = true;
        private bool _doLight = true;
        private Colour _cameraLightCC;

        private bool _postProcess = false;

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
            if (e[Keys.BackSpace])
            {
                _cameraPos = Vector3.Zero;
                _rotateX = Radian.Percent(0.5f);
                _rotateY = 0;
                _rotateZ = 0;
                _moveSpeed = 1;

                _cameraLightCC = _cameraLight;
                if (_torchLight)
                {
                    Actions.Push(() => _shader.SetSpotLightColour(0, _cameraLightCC));
                }
                return;
            }
            if (e[Keys.Enter])
            {
                _cameraPos = new Vector3(-8008, -2, 8);
                _rotateX = Radian.Percent(0.5f);
                _rotateY = 0;
                _rotateZ = 0;
                _moveSpeed = 0.125f;

                _cameraLightCC = new Colour(255, 235, 210);
                if (_torchLight)
                {
                    Actions.Push(() => _shader.SetSpotLightColour(0, _cameraLightCC));
                }
                return;
            }
            if (e[Keys.L])
            {
                _torchLight = !_torchLight;

                if (_torchLight)
                {
                    Actions.Push(() => _shader.SetSpotLightColour(0, _cameraLightCC));
                    return;
                }

                Actions.Push(() => _shader.SetSpotLightColour(0, Colour.Zero));
                return;
            }
            if (e[Keys.N])
            {
                _doLight = !_doLight;
                return;
            }
            if (e[Keys.P])
            {
                _postProcess = !_postProcess;

                Actions.Push(() =>
                {
                    _context.Pixelated = _postProcess;
                    _context.UseKernel = _postProcess;
                });
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
            if (e[Keys.J])
            {
                if (_context.RenderState.PolygonMode == PolygonMode.Fill)
                {
                    _context.RenderState.PolygonMode = PolygonMode.Line;
                    return;
                }

                _context.RenderState.PolygonMode = PolygonMode.Fill;
                return;
            }
        }
        private Vector2 _mouseLocation = Vector2.Zero;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (CursorMode != CursorMode.Disabled) { return; }

            if (new Vector2(e.X, e.Y) == _mouseLocation) { return; }

            float distanceX = e.X - _mouseLocation.X;
            float distanceY = e.Y - _mouseLocation.Y;

            _mouseLocation = new Vector2(e.X, e.Y);

            _rotateY += Radian.Degrees(distanceX * 0.1f);
            _rotateX += Radian.Degrees(distanceY * 0.1f);
        }

        protected override void OnSizePixelChange(VectorIEventArgs e)
        {
            base.OnSizePixelChange(e);

            float mWidth;
            float mHeight;

            if (e.X > e.Y)
            {
                float heightPercent = (float)e.Y / e.X;

                mWidth = 400;

                mHeight = mWidth * heightPercent;
            }
            else
            {
                float widthPercent = (float)e.X / e.Y;

                mHeight = 56.25f * 4;

                mWidth = mHeight * widthPercent;
            }

            Actions.Push(() => _context.PixelateSize = new Vector2(mWidth, mHeight));
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);

            lock (_perspective)
            {
                float zoom = Degrees.Radian(_perspective.Fovy);
                zoom -= e.DeltaY * zoom * 0.02f;

                if (zoom < 1)
                {
                    zoom = 1;
                }
                else if (zoom > 179)
                {
                    zoom = 179;
                }

                _perspective.Fovy = Radian.Degrees(zoom);
            }
        }

        protected override void OnFileDrop(FileDropEventArgs e)
        {
            base.OnFileDrop(e);

            Actions.Push(() => OnFileDropReceive(e));
        }
        private void OnFileDropReceive(FileDropEventArgs e)
        {
            if (Bitmap.GetImageEncoding(e.Paths[0]) != ImageEncoding.Unknown)
            {
                _loadObjectImage.Dispose();

                _loadObjectImage = Texture2D.Create(e.Paths[0], WrapStyle.EdgeClamp, TextureSampling.Blend, true);

                return;
            }

            _loadObject.Dispose();

            try
            {
                _loadObject = Object3D.FromObjNT(e.Paths[0]);
            }
            catch (Exception)
            {
                _loadObject = Object3D.FromObjN(e.Paths[0]);
            }
        }
    }
}
