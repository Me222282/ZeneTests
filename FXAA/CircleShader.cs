using System;
using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace FXAA
{
    public class CircleShader : BaseShaderProgram
    {
        public CircleShader()
        {
            Create(File.ReadAllText("resources/circleVert.glsl"),
                File.ReadAllText("resources/circleFrag.glsl"),
                "matrix", "size", "radius", "minRadius", "uColour");

            // Set matrix to "0"
            SetMatrices();
            // Set size to "1"
            Size = 1.0;
        }

        private Matrix4 _m1 = Matrix4.Identity;
        public Matrix4 Matrix1
        {
            get
            {
                return _m1;
            }
            set
            {
                _m1 = value;
                SetMatrices();
            }
        }
        private Matrix4 _m2 = Matrix4.Identity;
        public Matrix4 Matrix2
        {
            get
            {
                return _m2;
            }
            set
            {
                _m2 = value;
                SetMatrices();
            }
        }
        private Matrix4 _m3 = Matrix4.Identity;
        public Matrix4 Matrix3
        {
            get
            {
                return _m3;
            }
            set
            {
                _m3 = value;
                SetMatrices();
            }
        }
        private void SetMatrices()
        {
            Matrix4 matrix = _m1 * _m2 * _m3;
            SetUniformF(Uniforms[0], ref matrix);
        }
        
        private double _size;
        public double Size
        {
            get => _size;
            set
            {
                _size = value;

                SetUniformF(Uniforms[1], value);
                SetUniformF(Uniforms[2], value * value * 0.25);
            }
        }
        
        private double _lWidth;
        public double LineWidth
        {
            get => _lWidth;
            set
            {
                _lWidth = value;
                
                double len = (_size * 0.5) - value;

                SetUniformF(Uniforms[3], len * len);
            }
        }
        
        private ColourF _colour;
        public ColourF Colour
        {
            get => _colour;
            set
            {
                _colour = value;

                SetUniformF(Uniforms[4], (Vector4)value);
            }
        }
    }
}