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
                SetUniform(Uniforms[0], value);
            }
        }
        private IMatrix _m3 = Matrix.Identity;
        public IMatrix Projection
        {
            get => _m3;
            set
            {
                _m3 = value;

                SetMatrices();
            }
        }
        private IMatrix _m2 = Matrix.Identity;
        public IMatrix View
        {
            get => _m2;
            set
            {
                _m2 = value;

                SetMatrices();
            }
        }
        private IMatrix _m1 = Matrix.Identity;
        public IMatrix Model
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
            SetUniform(Uniforms[1], _m1 * _m2 * _m3);
        }
    }
}
