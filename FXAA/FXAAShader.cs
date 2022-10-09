using System;
using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace FXAA
{
    /// <summary>
    /// https://github.com/McNopper/OpenGL/tree/master/Example42/shader
    /// </summary>
    public class FXAAShader : BaseShaderProgram
    {
        public FXAAShader()
        {
            Create(File.ReadAllText("resources/fxaaVert.glsl"),
                File.ReadAllText("resources/fxaaFrag.glsl"),
                "u_colorTexture", "u_texelStep", "u_showEdges", "u_fxaaOn",
                "u_lumaThreshold", "u_mulReduce", "u_minReduce", "u_maxSpan");
        }
        
        private int _texture = 0;
        public int TextureSlot
        {
            get => _texture;
            set
            {
                _texture = value;

                SetUniformI(Uniforms[0], value);
            }
        }
        
        private Vector2 _texelStep = Vector2.Zero;
        public Vector2 TexelStep
        {
            get => _texelStep;
            set
            {
                _texelStep = value;

                SetUniformF(Uniforms[1], value);
            }
        }
        private bool _showEdges = false;
        public bool ShowEdges
        {
            get => _showEdges;
            set
            {
                _showEdges = value;

                SetUniformI(Uniforms[2], value ? 1 : 0);
            }
        }
        private bool _fxaaOn = false;
        public bool FXAAOn
        {
            get => _fxaaOn;
            set
            {
                _fxaaOn = value;

                SetUniformI(Uniforms[3], value ? 1 : 0);
            }
        }
        
        private double _lumaThreshold = 0.0;
        public double LumaThreshold
        {
            get => _lumaThreshold;
            set
            {
                _lumaThreshold = value;

                SetUniformF(Uniforms[4], value);
            }
        }
        private double _mulReduce = 0.0;
        public double MulReduce
        {
            get => _mulReduce;
            set
            {
                _mulReduce = value;

                SetUniformF(Uniforms[5], 1d / value);
            }
        }
        private double _minReduce = 0.0;
        public double MinReduce
        {
            get => _minReduce;
            set
            {
                _minReduce = value;

                SetUniformF(Uniforms[6], 1d / value);
            }
        }
        private double _maxSpan = 0.0;
        public double MaxSpan
        {
            get => _maxSpan;
            set
            {
                _maxSpan = value;

                SetUniformF(Uniforms[7], value);
            }
        }
    }
}