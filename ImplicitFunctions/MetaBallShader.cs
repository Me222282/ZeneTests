using System;
using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace ImplicitFunctions
{
    public class MetaBallShader : BaseShaderProgram
    {
        public MetaBallShader(int nBalls)
        {
            string fSource = File.ReadAllText("resources/MetaBallFrag.shader");

            Create(File.ReadAllText("resources/MetaBallVert.shader"),
                fSource.Replace("@SIZE", nBalls.ToString()),
                "scale", "offset", "balls");

            _balls = new Vector3[nBalls];
        }

        private readonly Vector3[] _balls;
        public Vector3 this[int index]
        {
            get => _balls[index];
            set
            {
                _balls[index] = value;

                SetUniformF(Uniforms[2], index, value);
            }
        }

        private Vector2 _scale = Vector2.Zero;
        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;

                SetUniformF(Uniforms[0], value);
            }
        }
        private Vector2 _offset = Vector2.Zero;
        public Vector2 Offset
        {
            get => _offset;
            set
            {
                _offset = value;

                SetUniformF(Uniforms[1], value);
            }
        }
    }
}
