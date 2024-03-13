using System;
using Zene.Graphics.Base;
using Zene.Graphics.Z3D;
using Zene.Graphics;
using Zene.Windowing;
using Zene.Structs;
using System.Collections.Generic;

namespace CSGL
{
    public unsafe class ShadowLWindow : Window
    {
        public ShadowLWindow(int width, int height, string title)
            : base(width, height, title)
        {
            _context = new PostProcessing(width, height);

            SetUp();

            CursorMode = CursorMode.Disabled;

            //FullScreen = true;
        }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                Shader.Dispose();
                Framebuffer.Dispose();
                ShadowMapper.Dispose();

                DrawObject.Dispose();

                Floor.Dispose();
                FloorTexture.Dispose();
                FloorNormalMap.Dispose();

                Plane.Dispose();

                player.Dispose();
                sphere.Dispose();
            }
        }

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);

            Draw(_context);
            e.Context.Render(_context);
        }

        private PostProcessing _context;

        private static readonly Vector3 Red = new Vector3(1, 0, 0);

        private static readonly Vector3 Green = new Vector3(0, 1, 0);

        private static readonly Vector3 Blue = new Vector3(0, 0, 1);

        private static readonly Vector3 Yellow = new Vector3(1, 1, 0);

        private readonly Vector3[] vertData = new Vector3[]
        {
            /*Vertex*/ new Vector3(-5, 5, 5), Red,
            /*Vertex*/ new Vector3(5, 5, 5), Green,
            /*Vertex*/ new Vector3(5, -5, 5), Blue,
            /*Vertex*/ new Vector3(-5, -5, 5), Yellow,

            /*Vertex*/ new Vector3(-5, 5, -5), Blue,
            /*Vertex*/ new Vector3(5, 5, -5), Yellow,
            /*Vertex*/ new Vector3(5, -5, -5), Red,
            /*Vertex*/ new Vector3(-5, -5, -5), Green
        };
        private readonly uint[] indexData = new uint[]
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

        private LightingShader Shader;

        private ShadowMapper ShadowMapper;

        private DrawObject<Vector3, uint> DrawObject;

        private DrawObject<Vector3, uint> Plane;

        private Material ObjectMaterial = new Material(Material.Source.Default, Material.Source.None, Shine.None);

        private DrawObject<Vector3, uint> Floor;

        private Texture2D FloorTexture;
        private Texture2D FloorNormalMap;

        private Material FloorMaterial = new Material(Material.Source.Default, Material.Source.Default, Shine.L);

        private DrawObject<Vector2, byte> DepthDraw;

        private Light LightObject;

        private Object3D sphere;

        protected virtual void SetUp()
        {
            Shader = new LightingShader(1, 1);

            ShadowMapper = new ShadowMapper(20480, 20480);

            Object3D.AddNormals(vertData, 2, indexData, out List<Vector3> vertices, out List<uint> indices);

            DrawObject = new DrawObject<Vector3, uint>(vertices.ToArray(), indices.ToArray(), 3, 0, AttributeSize.D3, BufferUsage.DrawFrequent);

            DrawObject.AddAttribute((uint)LightingShader.Location.ColourAttribute, 1, AttributeSize.D3); // Colour attribute
            DrawObject.AddAttribute((uint)LightingShader.Location.Normal, 2, AttributeSize.D3); // Normals

            Object3D.AddNormals(new Vector3[] { new Vector3(-20, -100, 0),
                new Vector3(100, -100, 0),
                new Vector3(100, 0, -100),
                new Vector3(-20, 0, -100) }, 1, new uint[] { 0, 1, 2, 2, 3, 0 }, out List<Vector3> planeVerts, out List<uint> planeInds);

            Plane = new DrawObject<Vector3, uint>(planeVerts.ToArray(), planeInds.ToArray(), 2, 0, AttributeSize.D3, BufferUsage.DrawFrequent);
            Plane.AddAttribute((uint)LightingShader.Location.Normal, 1, AttributeSize.D3); // Normals

            Object3D.AddNormalTangents(new Vector3[] { new Vector3(-500, 10, -500), new Vector3(0, 0, 0),
                new Vector3(500, 10, -500), new Vector3(100, 0, 0),
                new Vector3(500, 10, 500), new Vector3(100, 100, 0),
                new Vector3(-500, 10, 500), new Vector3(0, 100, 0)}, 2, 1, new uint[] { 0, 1, 2, 2, 3, 0 }, out List<Vector3> floorVerts, out List<uint> floorInds);

            Floor = new DrawObject<Vector3, uint>(floorVerts.ToArray(), floorInds.ToArray(), 4, 0, AttributeSize.D3, BufferUsage.DrawFrequent);

            //FloorTexture = new TextureBuffer("Resources/wood.png", WrapStyle.Repeated, TextureQuality.Blend, true);
            FloorTexture = Texture2D.Create("Resources/wood.png", WrapStyle.Repeated, TextureSampling.BlendMipMapBlend, true);

            Shader.TextureSlot = 0;
            Shader.NormalMapSlot = 1;
            //FloorNormalMap = new TextureBuffer("Resources/woodNor.png", WrapStyle.Repeated, TextureQuality.Blend, true);
            FloorNormalMap = Texture2D.Create("Resources/woodNor.png", WrapStyle.Repeated, TextureSampling.BlendMipMapBlend, true);

            Floor.AddAttribute((uint)LightingShader.Location.TextureCoords, 1, AttributeSize.D3); // Texture Coordinates
            Floor.AddAttribute((uint)LightingShader.Location.NormalMapTextureCoords, 1, AttributeSize.D3); // Normal Map Coordinates
            Floor.AddAttribute((uint)LightingShader.Location.Normal, 2, AttributeSize.D3); // Normals
            Floor.AddAttribute((uint)LightingShader.Location.Tangents, 3, AttributeSize.D3); // Tangents

            State.Blending = true;
            State.SourceScaleBlending = BlendFunction.SourceAlpha;
            State.DestinationScaleBlending = BlendFunction.OneMinusSourceAlpha;

            Shader.AmbientLight = new Colour(12, 12, 15);

            Shader.IngorBlackLight = true;

            //Shader.SetLight(0, new Zene.Graphics.Light(new Colour(255, 255, 255), Colour.Zero, 0, 0, lightDir, true));
            Shader.SetSpotLight(0, new SpotLight(new ColourF3(1.4f, 1.4f, 1.4f), Radian.Degrees(45), Radian.Degrees(60), 0, 0, lightDir, lightDir));

            DepthDraw = new DrawObject<Vector2, byte>(new Vector2[]
                {
                    new Vector2(-1, -1), new Vector2(0, 0),
                    new Vector2(1, -1), new Vector2(1, 0),
                    new Vector2(1, 1), new Vector2(1, 1),
                    new Vector2(-1, 1), new Vector2(0, 1)
                }, new byte[] { 0, 1, 2, 2, 3, 0 }, 2, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
            DepthDraw.AddAttribute((uint)LightingShader.Location.TextureCoords, 1, AttributeSize.D2);

            LightObject = new Light(Vector3.Zero, 0.5, BufferUsage.DrawFrequent);

            player = new Player(2, 8, 2);

            hMWS = (double)heightMap.Width / 1000;
            hMHS = (double)heightMap.Height / 1000;
            bw = (double)1000 / heightMap.Width;
            bh = (double)1000 / heightMap.Height;

            sphere = Object3D.FromObjNMT("Resources/Sphere.obj");
        }

        private Vector3 CameraPos = Vector3.Zero;
        private Vector3 CameraVelocity = Vector3.Zero;

        private Player player;

        private Radian rotateX = Radian.Percent(0.5);
        private Radian rotateY = 0;
        private Radian rotateZ = 0;

        //private Point3 lightDir = new Point3(-0.5, -2, -0.5) * 100;
        private Vector3 lightDir = new Vector3(1, -1, 1) * 100;
        private readonly IMatrix lightRotation = Matrix3.CreateRotationX(Radian.Percent(-0.0000125)) * Matrix3.CreateRotationZ(Radian.Percent(0.0000125));

        protected virtual void Draw(IDrawingContext context)
        {
            Vector3 cameraMove = new Vector3(0, 0, 0);

            double speed = 0.5;
            if (CameraPos.Y > (GetHeightMapPos(CameraPos) + 0.5))
            {
                speed += 0.25;
            }

            double offset = 0;

            if (_left) { cameraMove.X += speed; }
            if (_right) { cameraMove.X -= speed; }
            if (_forward) { cameraMove.Z += speed; }
            if (_backward) { cameraMove.Z -= speed; }
            if (_up) { CameraVelocity.Y = 2; }
            if (_down) { offset = 3; }

            if (_lShift) { cameraMove *= 2; }

            if (_lAltGoFast) { cameraMove *= 4; }

            CameraPos += cameraMove * Matrix3.CreateRotationY(rotateY);

            //CameraPos += cameraMove * Matrix3.CreateRotationY(rotateY);

            PlayerPhysics(ref CameraPos, ref CameraVelocity, offset);

            Matrix4 playerMatrix = Matrix4.CreateTranslation(-CameraPos.X, -(CameraPos.Y - 5), -CameraPos.Z);

            lightDir *= lightRotation;

            //Shader.SetLightPosition(0, lightDir, true);
            Shader.SetSpotLightPosition(0, lightDir);
            Shader.SetSpotLightDirection(0, lightDir);

            State.DepthTesting = true;

            CreateShadowMap(playerMatrix);

            Framebuffer.Bind();
            Framebuffer.Clear(BufferBit.Colour | BufferBit.Depth);

            Shader.Bind();

            //Shader.SetProjectionMatrix(Matrix4.Identity);
            //Shader.SetViewMatrix(Matrix4.Identity);
            //Shader.SetModelMatrix(Matrix4.Identity);

            Shader.CameraPosition = -CameraPos;
            Shader.Matrix2 = Matrix4.CreateTranslation(CameraPos) * Matrix4.CreateRotationY(rotateY) *
                Matrix4.CreateRotationX(rotateX) * Matrix4.CreateRotationZ(rotateZ);

            //Shader.SetCameraPosition(lightDir);
            //Shader.SetViewMatrix(Matrix4.LookAt(lightDir, Point3.Zero, new Point3(0, 1, 0)));
            
            Shader.DrawLighting = doLight;
            Shader.Matrix1 = Matrix.Identity;

            Shader.NormalMapping = false;
            Shader.ColourSource = ColourSource.AttributeColour;
            Shader.SetMaterial(ObjectMaterial);
            context.Shader = Shader;
            context.Draw(DrawObject);

            Shader.NormalMapping = false;
            Shader.ColourSource = ColourSource.UniformColour;
            Shader.Colour = new Colour(100, 200, 255);
            Shader.SetMaterial(FloorMaterial);
            context.Draw(Plane);

            Shader.NormalMapping = false;
            Shader.DrawLighting = false;
            Shader.Matrix1 = Matrix4.CreateTranslation(lightDir);
            Shader.ColourSource = ColourSource.UniformColour;
            Shader.Colour = new Colour(255, 255, 255);
            context.Draw(LightObject);

            Shader.ColourSource = ColourSource.Texture;
            Shader.SetMaterial(FloorMaterial);
            Shader.NormalMapping = true;
            Shader.DrawLighting = doLight;

            FloorTexture.Bind(0);
            FloorNormalMap.Bind(1);

            Shader.Matrix1 = Matrix4.CreateTranslation(10, 2.5, 10);
            context.Draw(sphere);
            Shader.Matrix1 = Matrix.Identity;
            context.Draw(Floor);

            FloorTexture.Unbind();
            FloorNormalMap.Unbind();

            //DepthDraw.Draw();
        }

        private void CreateShadowMap(Matrix4 playerMatrix)
        {
            ShadowMapper.Clear();

            IMatrix smP = /*Matrix4.CreateOrthographic(100, 100, 0, 1000);*/ Matrix4.CreatePerspectiveFieldOfView(Radian.Degrees(110), 1, 0.1, 5000);
            IMatrix smV = Matrix4.LookAt(lightDir, Vector3.Zero, new Vector3(0, 1, 0)) * Matrix4.CreateRotationX(Radian.Percent(0.5));

            ShadowMapper.ProjectionMatrix = smP;
            ShadowMapper.ViewMatrix = smV;
            Shader.LightSpaceMatrix = smV * smP;
            ShadowMapper.ModelMatrix = Matrix.Identity;

            ShadowMapper.Draw(Floor);
            ShadowMapper.Draw(DrawObject);
            ShadowMapper.Draw(Plane);

            ShadowMapper.ModelMatrix = Matrix4.CreateTranslation(10, 2.5, 10);
            ShadowMapper.Draw(sphere);

            ShadowMapper.ModelMatrix = playerMatrix;
            ShadowMapper.Draw(player);

            ShadowMapper.DepthMap.Bind(2);
            Shader.ShadowMapSlot = 2;
        }

        private void PlayerPhysics(ref Vector3 pos, ref Vector3 velocity, double yOffset)
        {
            velocity.Y -= 0.15;

            Vector3 shift = cube.Collision(player.ColBox, player.ColBox.Shifted(-velocity));

            pos += velocity + shift;

            player.ColBox.Shift(shift);

            double floor = GetHeightMapPos(pos);

            if ((pos.Y + yOffset) < floor)
            {
                pos.Y = floor - yOffset;

                velocity.Y = 0;
            }
        }

        private readonly BitmapD heightMap = new BitmapD(new Bitmap("Resources/floorHeight.png"), -1, 0.5);

        private readonly CObject cube = new CObject(-5, 5, -5, 5, 5, -5);

        private double hMWS;
        private double hMHS;
        private double bw;
        private double bh;

        private double GetHeightMapPos(Vector3 worldPos)
        {
            double wx = worldPos.X + 500;
            double wy = worldPos.Z + 500;

            int x1 = (int)Math.Floor(wx * hMWS);
            int y1 = (int)Math.Floor(wy * hMHS);

            int x2 = (int)Math.Ceiling(wx * hMWS);
            int y2 = (int)Math.Ceiling(wy * hMHS);

            double x1y1 = heightMap.GetValue(x1, y1);
            double x2y1 = heightMap.GetValue(x2, y1);
            double x1y2 = heightMap.GetValue(x1, y2);
            double x2y2 = heightMap.GetValue(x2, y2);

            double xs = (((x1 / hMWS) - wx) / bw) * -1;
            double ys = (((y1 / hMHS) - wy) / bh) * -1;

            double y1I = Lerp(x1y1, x2y1, xs);
            double y2I = Lerp(x1y2, x2y2, xs);

            return Lerp(y1I, y2I, ys);
        }

        private static double Lerp(double a, double b, double scale)
        {
            return a + ((b - a) * scale);
        }

        private bool _left;
        private bool _right;
        private bool _up;
        private bool _down;
        private bool _forward;
        private bool _backward;
        private bool _lShift;
        private bool _lAltGoFast;

        private bool doLight = true;

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
                player.Small = true;
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
            if (e[Keys.BackSpace])
            {
                CameraPos = Vector3.Zero;
                rotateX = Radian.Percent(0.5);
                rotateY = 0;
                rotateZ = 0;
                return;
            }
            if (e[Keys.N])
            {
                doLight = !doLight;
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
                player.Small = false;
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
            rotateX += Radian.Degrees(distanceY * 0.1);
        }

        protected override void OnSizeChange(VectorIEventArgs e)
        {
            base.OnSizeChange(e);

            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(Radian.Degrees(60), (double)e.X / e.Y, 1, 5000);

            Shader.Matrix1 = proj;
        }

        protected override void OnSizePixelChange(VectorIEventArgs e)
        {
            base.OnSizePixelChange(e);

            // Invalide size
            if (e.X <= 0 || e.Y <= 0) { return; }

            _context.Size = e.Value;
        }
    }
}
