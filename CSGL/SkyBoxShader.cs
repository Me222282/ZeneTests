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
                File.ReadAllText("Resources/skyBoxFrag.shader"), 1,
                "skybox", "matrix");

            SetUniform(Uniforms[0], 0);
        }

        public ITexture Texture { get; set; }
        public override void PrepareDraw()
        {
            base.PrepareDraw();
            Texture?.Bind(0);
        }
    }
}
