using System;
using System.IO;
using Zene.Graphics;
using Zene.Graphics.Base;
using Zene.Structs;

namespace FXAA
{
    /// <summary>
    /// http://blog.simonrodriguez.fr/articles/2016/07/implementing_fxaa.html
    /// </summary>
    public class FXAAShader2 : BaseShaderProgram
    {
        public FXAAShader2()
        {
            Create(File.ReadAllText("resources/fxaaV2.glsl"),
                File.ReadAllText("resources/fxaaF2.glsl"),
                "u_colorTexture", "inverseScreenSize");
            
            GL.BindAttribLocation(Id, 2, "vTex");
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
        
        private Vector2 _size = Vector2.Zero;
        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;

                SetUniformF(Uniforms[1], (1d, 1d) / value);
            }
        }
    }
}