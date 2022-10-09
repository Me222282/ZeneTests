using System;
using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace PhysicsTest
{
    public class TextureShader : BaseShaderProgram
    {
        public TextureShader()
        {
            Create(File.ReadAllText("Resources/Vertex.shader"),
                File.ReadAllText("Resources/Fragment.shader"),
                "uTextureSlot", "uProj_View");

            // Set matrix to identiy
            SetMatrices();
        }

        private int _texSlot = 0;
        public int TextureSlot
        {
            get => _texSlot;
            set
            {
                _texSlot = value;

                SetUniformI(Uniforms[0], value);
            }
        }

        private Matrix4 _mView = Matrix4.Identity;
        public Matrix4 View
        {
            get
            {
                return _mView;
            }
            set
            {
                _mView = value;
                SetMatrices();
            }
        }
        private Matrix4 _mProj = Matrix4.Identity;
        public Matrix4 Projection
        {
            get
            {
                return _mProj;
            }
            set
            {
                _mProj = value;
                SetMatrices();
            }
        }
        private void SetMatrices()
        {
            Matrix4 matrix = _mView * _mProj;
            SetUniformF(Uniforms[1], ref matrix);
        }
    }
}
