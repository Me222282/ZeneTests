using System;
using System.IO;
using Zene.Graphics;

namespace ImplicitFunctions
{
    public class IFShader : BaseShaderProgram
    {
        public IFShader()
        {
            Create(File.ReadAllText("resources/ImplicitFuncVert.shader"),
                File.ReadAllText("resources/ImplicitFuncFrag.shader"));
        }
    }
}
