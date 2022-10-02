using System;
using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace CSGL
{
    public class SkyBoxShader : BaseShaderProgram
    {
        public SkyBoxShader()
        {
            Create(
                File.ReadAllText("Resources/skyBoxVert.shader"),
                File.ReadAllText("Resources/skyBoxFrag.shader"),
                "skybox", "matrix");
        }

        public int TextureSlot
        {
            set
            {
                SetUniformI(Uniforms[0], value);
            }
        }
        private Matrix4 _m3 = Matrix4.Identity;
        public Matrix4 Projection
        {
            get => _m3;
            set
            {
                _m3 = value;

                SetMatrices();
            }
        }
        private Matrix4 _m2 = Matrix4.Identity;
        public Matrix4 View
        {
            get => _m2;
            set
            {
                _m2 = value;

                SetMatrices();
            }
        }
        private Matrix4 _m1 = Matrix4.Identity;
        public Matrix4 Model
        {
            get => _m1;
            set
            {
                _m1 = value;

                SetMatrices();
            }
        }
        private void SetMatrices()
        {
            Matrix4 matrix = _m1 * _m2 * _m3;
            SetUniformF(Uniforms[1], ref matrix);
        }
    }
}
