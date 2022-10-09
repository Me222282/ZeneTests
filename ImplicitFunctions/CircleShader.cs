using System;
using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace ImplicitFunctions
{
    public class CircleShader : BaseShaderProgram
    {
        public CircleShader()
        {
            Create(File.ReadAllText("resources/CircleVert.shader"),
                File.ReadAllText("resources/CircleFrag.shader"),
                "matrix");
        }

        private Matrix4 _m1 = Matrix4.Identity;
        public Matrix4 Model
        {
            get
            {
                return _m1;
            }
            set
            {
                _m1 = value;
                SetMatrix();
            }
        }
        private Matrix4 _m2 = Matrix4.Identity;
        public Matrix4 View
        {
            get
            {
                return _m2;
            }
            set
            {
                _m2 = value;
                SetMatrix();
            }
        }
        private Matrix4 _m3 = Matrix4.Identity;
        public Matrix4 Projection
        {
            get
            {
                return _m3;
            }
            set
            {
                _m3 = value;
                SetMatrix();
            }
        }
        private void SetMatrix()
        {
            Matrix4 matrix = _m1 * _m2 * _m3;
            SetUniformF(Uniforms[0], ref matrix);
        }
    }
}
