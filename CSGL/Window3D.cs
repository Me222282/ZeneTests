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

            SetUp();

            CursorMode = CursorMode.Disabled;

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

                Floor.Dispose();
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

            _actions.Flush();

            State.PolygonMode = _polygonMode;
            Draw(_context);
            State.PolygonMode = PolygonMode.Fill;

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

        private PostProcessing _context;

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
        private double _objectRotation = 0.001;

        private Material _objectMaterial = new Material(Material.Source.Default, Material.Source.None, Shine.None);

        private Object3D _loadObject;
        private Texture2D _loadObjectImage;

        private DrawObject<Vector3, uint> Floor;

        private Texture2D _floorTexture;
        private Texture2D _floorNormalMap;

        private Material _floorMaterial = new Material(Material.Source.Default, Material.Source.Default, Shine.L);

        private Light _lightObject;
        private Light _farLightObject;

        private Zene.Graphics.Light _light;
        private Zene.Graphics.Light _farLight;

        private double _lightAmplitude = 0;

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

            Floor = new DrawObject<Vector3, uint>(floorVerts.ToArray(), floorInds.ToArray(), 4, 0, AttributeSize.D3, BufferUsage.DrawFrequent);

            _floorTexture = Texture2D.LoadAsync("Resources/wood.png", WrapStyle.Repeated, TextureSampling.BlendMipMapBlend, true);

            _shader.TextureSlot = 0;
            _shader.NormalMapSlot = 1;
            _floorNormalMap = Texture2D.LoadAsync("Resources/woodNor.png", WrapStyle.Repeated, TextureSampling.BlendMipMapBlend, true);

            Floor.AddAttribute(ShaderLocation.TextureCoords, 1, AttributeSize.D3); // Texture Coordinates
            Floor.AddAttribute(ShaderLocation.NormalTexture, 1, AttributeSize.D3); // Normal Map Coordinates
            Floor.AddAttribute(ShaderLocation.Normal, 2, AttributeSize.D3); // Normals
            Floor.AddAttribute(ShaderLocation.Tangent, 3, AttributeSize.D3); // Tangents

            State.Blending = true;
            State.SourceScaleBlending = BlendFunction.SourceAlpha;
            State.DestinationScaleBlending = BlendFunction.OneMinusSourceAlpha;

            //GL.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);

            rotationMatrix = Matrix3.CreateRotationX(0);

            _light = new Zene.Graphics.Light(new Colour3(255, 255, 255), Colour3.Zero, 0.0014, 0.000007, new Vector3(10, 0, 10));
            _shader.SetLight(0, _light);

            _farLight = new Zene.Graphics.Light(new Colour3(200, 200, 200), Colour3.Zero, 0.014, 0.0007, new Vector3(500, 0, 500));
            _shader.SetLight(1, _farLight);

            _shader.AmbientLight = new Colour(12, 12, 15);

            _lightObject = new Light(Vector3.Zero, 0.5, BufferUsage.DrawFrequent);

            _farLightObject = new Light(new Vector3(500, 0, 500), 0.5, BufferUsage.DrawFrequent);

            cameraLightCC = cameraLight = new Colour(120, 110, 100);
            _shader.SetSpotLight(0, new SpotLight((Colour3)cameraLight, Radian.Degrees(22.5), Radian.Degrees(40), 0.0045, 0.00075, Vector3.Zero, new Vector3(0d, 0d, 1d)));

            _room = new Room(_shader);

            _shader.IngorBlackLight = true;

            _loadObject = Object3D.FromObjNT("Resources/Sphere.obj");

            _textDisplay = new TextRenderer();
            _font = new SampleFont();

            _loadObjectImage = Texture2D.Create(new GLArray<Colour>(1, 1, 1, new Colour(134, 94, 250)), WrapStyle.EdgeClamp, TextureSampling.Blend, false);
        }

        private readonly Matrix3 lightRotation = Matrix3.CreateRotationY(Radian.Percent(0.001));

        private Vector3 CameraPos = Vector3.Zero;

        private Radian rotateX = Radian.Percent(0.5);
        private Radian rotateY = 0;
        private Radian rotateZ = 0;

        private IMatrix rotationMatrix;

        private Colour cameraLight;

        private double moveSpeed = 1;

        protected virtual void Draw(IDrawingContext context)
        {
            rotationMatrix = Matrix3.CreateRotationY(rotateY) * Matrix3.CreateRotationX(rotateX);

            Vector3 cameraMove = new Vector3(0, 0, 0);

            if (this[Keys.A])       { cameraMove.X += moveSpeed; }
            if (this[Keys.D])       { cameraMove.X -= moveSpeed; }
            if (this[Keys.W])       { cameraMove.Z -= moveSpeed; }
            if (this[Keys.S])       { cameraMove.Z += moveSpeed; }
            if (this[Keys.Space])   { cameraMove.Y -= moveSpeed; }
            if (this[Mods.Control]) { cameraMove.Y += moveSpeed; }

            if (this[Keys.LeftShift])   { cameraMove *= 2; }
            if (this[Keys.RightShift])  { cameraMove *= 0.25; }
            if (this[Mods.Alt])         { cameraMove *= 4; }

            CameraPos += (Vector3)(cameraMove * rotationMatrix);

            _shader.Bind();

            _shader.CameraPosition = -CameraPos;

            Vector3 lv3 = (Vector3)_light.LightVector;
            lv3 *= lightRotation;
            _light.LightVector = (lv3, _light.LightVector.W);

            _shader.SetLightPosition(0, _light.LightVector);

            IMatrix view = Matrix4.CreateTranslation(CameraPos) * Matrix4.CreateRotationY(rotateY) *
                Matrix4.CreateRotationX(rotateX) * Matrix4.CreateRotationZ(rotateZ);
            _shader.Matrix2 = view;
            _textDisplay.View = view;

            _shader.SetSpotLightPosition(0, -CameraPos);
            _shader.SetSpotLightDirection(0, new Vector3(0, 0, 1) *
                (Matrix3.CreateRotationX(rotateX - Radian.Percent(0.5)) * Matrix3.CreateRotationY(rotateY) * Matrix3.CreateRotationZ(rotateZ)));

            _shader.TEMP();

            State.DepthTesting = true;

            //IFrameBuffer.ClearColour(new ColourF(0.2f, 0.4f, 0.8f, 1.0f));
            Framebuffer.Clear(BufferBit.Colour | BufferBit.Depth);

            _shader.NormalMapping = false;
            _shader.DrawLighting = doLight;
            _shader.Matrix1 = Matrix4.CreateRotationX(Radian.Percent(_objectRotation));
            _objectRotation += 0.001;
            _shader.ColourSource = ColourSource.AttributeColour;
            _shader.SetMaterial(_objectMaterial);

            context.Shader = _shader;
            context.Draw(_drawObject);

            _shader.DrawLighting = doLight;
            _shader.Matrix1 = Matrix4.CreateRotationZ(Radian.Percent(0.5)) * Matrix4.CreateRotationY(Radian.Percent(0.25)) * Matrix4.CreateTranslation(100, 0, 0);
            //Shader.SetColourSource(ColourSource.UniformColour);
            _shader.ColourSource = ColourSource.Texture;
            _shader.Colour = new Colour(134, 94, 250);

            _loadObjectImage.Bind(0);
            _shader.TextureSlot = 0;
            context.Draw(_loadObject);

            _shader.ColourSource = ColourSource.Texture;
            _shader.Matrix1 = Matrix.Identity;
            _shader.SetMaterial(_floorMaterial);
            _shader.NormalMapping = true;

            _floorTexture.Bind(0);
            _floorNormalMap.Bind(1);
            context.Draw(Floor);

            _floorTexture.Unbind();
            _floorNormalMap.Unbind();

            _shader.NormalMapping = false;
            _shader.DrawLighting = false;
            _shader.Matrix1 = Matrix4.CreateTranslation((Vector3)_light.LightVector);
            _shader.ColourSource = ColourSource.UniformColour;
            _shader.Colour = (ColourF)_light.LightColour;
            context.Draw(_lightObject);

            double lA = Math.Sin(Radian.Percent(_lightAmplitude));
            _lightAmplitude += 0.005;

            byte colourValue = (byte)(255 - (int)((lA + 1) * 100));
            _farLight.LightColour = new Colour3(colourValue, colourValue, colourValue);

            //double lightScale = (3 - ((lA + 1) * 1.5)) + 1;

            double lightSizeL = ((lA + 1) * 4.5) + 1;
            double lightSizeQ = ((lA + 1) * 4.5) + 1;
            _farLight.Linear = 0.0014 * lightSizeL;
            _farLight.Quadratic = 0.000007 * lightSizeQ;

            _shader.SetLight(1, _farLight);

            _shader.Matrix1 = Matrix.Identity;
            _shader.ColourSource = ColourSource.UniformColour;
            _shader.Colour = (ColourF)_farLight.LightColour;
            context.Draw(_farLightObject);

            _shader.DrawLighting = doLight;
            _shader.SetMaterial(_room.RoomMat);
            _shader.Matrix1 = Matrix4.CreateTranslation(8000, 0, 0);
            _shader.ColourSource = ColourSource.Texture;
            _shader.TextureSlot = Room.TexTexSlot;
            _shader.NormalMapSlot = Room.NorTexSlot;
            _shader.NormalMapping = true;
            context.Render(_room);

            _textDisplay.Model = Matrix4.CreateTranslation(0, 0, -5.1) * Matrix4.CreateRotationX(Radian.Percent(_objectRotation));
            _textDisplay.DrawCentred(context, $"{Core.Time:N3}\n", _font, 0, 0);
            _textDisplay.DrawCentred(context, $"\n{CameraPos.SquaredLength:N3}", _font, 0, 0);
        }

        private int _fpsCounter = 0;

        private bool torchLight = true;
        private bool doLight = true;
        private Colour cameraLightCC;

        private bool _postProcess = false;
        private PolygonMode _polygonMode = PolygonMode.Fill;

        private readonly ActionManager _actions = new ActionManager();

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
                CameraPos = Vector3.Zero;
                rotateX = Radian.Percent(0.5);
                rotateY = 0;
                rotateZ = 0;
                moveSpeed = 1;

                cameraLightCC = cameraLight;
                if (torchLight)
                {
                    _actions.Push(() => _shader.SetSpotLightColour(0, cameraLightCC));
                }
                return;
            }
            if (e[Keys.Enter])
            {
                CameraPos = new Vector3(-8008, -2, 8);
                rotateX = Radian.Percent(0.5);
                rotateY = 0;
                rotateZ = 0;
                moveSpeed = 0.125;

                cameraLightCC = new Colour(255, 235, 210);
                if (torchLight)
                {
                    _actions.Push(() => _shader.SetSpotLightColour(0, cameraLightCC));
                }
                return;
            }
            if (e[Keys.L])
            {
                torchLight = !torchLight;

                if (torchLight)
                {
                    _actions.Push(() => _shader.SetSpotLightColour(0, cameraLightCC));
                    return;
                }

                _actions.Push(() => _shader.SetSpotLightColour(0, Colour.Zero));
                return;
            }
            if (e[Keys.N])
            {
                doLight = !doLight;
                return;
            }
            if (e[Keys.P])
            {
                _postProcess = !_postProcess;

                _actions.Push(() =>
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
                if (_polygonMode == PolygonMode.Fill)
                {
                    _polygonMode = PolygonMode.Line;
                    return;
                }

                _polygonMode = PolygonMode.Fill;
                return;
            }
        }
        private Vector2 _mouseLocation = Vector2.Zero;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (CursorMode != CursorMode.Disabled) { return; }

            if (new Vector2(e.X, e.Y) == _mouseLocation) { return; }

            double distanceX = e.X - _mouseLocation.X;
            double distanceY = e.Y - _mouseLocation.Y;

            _mouseLocation = new Vector2(e.X, e.Y);

            rotateY += Radian.Degrees(distanceX * 0.1);
            rotateX += Radian.Degrees(distanceY * 0.1);
        }

        private const double _near = 0.1;
        private const double _far = 3000;
        protected override void OnSizePixelChange(VectorIEventArgs e)
        {
            base.OnSizePixelChange(e);

            _actions.Push(() => OnSizePixelChangeReceive(e));
        }
        private void OnSizePixelChangeReceive(VectorIEventArgs e)
        {
            // Matrices
            Matrix4 matrix = Matrix4.CreatePerspectiveFieldOfView(Radian.Degrees(_zoom), (double)e.X / e.Y, _near, _far);

            _shader.Matrix3 = matrix;
            _textDisplay.Projection = matrix;

            _context.Size = e.Value;

            double mWidth;
            double mHeight;

            if (e.X > e.Y)
            {
                double heightPercent = (double)e.Y / e.X;

                mWidth = 400;

                mHeight = mWidth * heightPercent;
            }
            else
            {
                double widthPercent = (double)e.X / e.Y;

                mHeight = 56.25 * 4;

                mWidth = mHeight * widthPercent;
            }

            _context.PixelateSize = (mWidth, mHeight);
        }

        private double _zoom = 60;
        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);

            _actions.Push(() => OnScrollReceive(e));
        }
        private void OnScrollReceive(ScrollEventArgs e)
        {
            _zoom -= e.DeltaY * _zoom * 0.02;

            if (_zoom < 1)
            {
                _zoom = 1;
            }
            else if (_zoom > 179)
            {
                _zoom = 179;
            }

            Matrix4 matrix = Matrix4.CreatePerspectiveFieldOfView(Radian.Degrees(_zoom), (double)Width / Height, _near, _far);

            _shader.Matrix3 = matrix;
            _textDisplay.Projection = matrix;
        }

        protected override void OnFileDrop(FileDropEventArgs e)
        {
            base.OnFileDrop(e);

            _actions.Push(() => OnFileDropReceive(e));
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
