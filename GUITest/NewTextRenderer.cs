using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zene.Graphics;
using Zene.Graphics.Base;
using Zene.Structs;

namespace GUITest
{
    public class NewTextRenderer : BaseShaderProgram
    {
        public uint ShaderId => Id;

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
        private void SetMatrices()
        {
            Matrix4 matrix = _m1 * _m2 * _m3;
            SetUniformF(Uniforms[0], ref matrix);
        }

        private Colour _colour;
        public Colour Colour
        {
            get => _colour;
            set
            {
                _colour = value;

                SetUniformF(Uniforms[1], (Vector4)(ColourF)value);
            }
        }

        public NewTextRenderer()
        {
            // Create drawable object
            _drawable = new DrawObject<Vector2, byte>(new Vector2[]
            {
                new Vector2(-0.5, 0.5), new Vector2(0, 1),
                new Vector2(-0.5, -0.5), new Vector2(0, 0),
                new Vector2(0.5, -0.5), new Vector2(1, 0),
                new Vector2(0.5, 0.5), new Vector2(1, 1)
            }, new byte[] { 0, 1, 2, 2, 3, 0 }, 2, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
            _drawable.AddAttribute(2, 1, AttributeSize.D2); // Texture Coordinates

            _frame = new TextureRenderer(1, 1);
            _frame.SetColourAttachment(0, TextureFormat.R8);
            _source = new Framebuffer();

            //
            // Shader
            //

            Create(ShaderPresets.BasicVertex, File.ReadAllText("resources/textfrag.shader"),
                "matrix", "uColour", "uTextureSlot");

            // Set matrices in shader to default
            SetMatrices();
            // Set colour to default
            Colour = new Colour(255, 255, 255);
        }
        private readonly DrawObject<Vector2, byte> _drawable;
        private readonly TextureRenderer _frame;
        private readonly Framebuffer _source;

        public int TabSize { get; set; } = 4;

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                _drawable.Dispose();
            }
        }

        public void DrawCentred(ReadOnlySpan<char> text, Font font, double charSpace, double lineSpace)
        {
            if (font == null)
            {
                throw new ArgumentNullException(nameof(font));
            }
            // No text is to be drawn
            if (text == null || text == "") { return; }

            // Remove all whitespace and null values
            string compText = new string(text).Replace(" ", "");
            compText = compText.Replace("\n", "");
            compText = compText.Replace("\r", "");
            compText = compText.Replace("\t", "");
            compText = compText.Replace("\0", "");
            compText = compText.Replace("\a", "");

            // No visable characters are drawn
            if (compText == "") { return; }

            IFramebuffer drawFrame = State.GetBoundFramebuffer(FrameTarget.Draw);

            font.BindTexture(0);
            ITexture fontTexture = State.GetBoundTexture(0, TextureTarget.Texture2D);
            _source[0] = fontTexture;

            Vector2I texutreSize = (fontTexture.Properties.Width, fontTexture.Properties.Height);
            Vector2I pixelPerDouble = (Vector2I)((font.GetCharacterData('M').TextureRefSize * texutreSize) / font.GetCharacterData('M').Size);

            charSpace += font.CharSpaceBase;
            lineSpace += font.LineSpaceBase;

            // The widths of each line in text
            List<double> lineWidths = font.GetLineWidths(text, charSpace, TabSize);

            double maxWidth = lineWidths.Max();
            double maxHeight = lineWidths.Count + ((lineWidths.Count - 1) * lineSpace);

            // Set frame size
            _frame.Size = ((int)(maxWidth * pixelPerDouble.X), (int)(maxHeight * pixelPerDouble.Y));

            _frame.ClearColour = new ColourF(1f, 0f, 0f, 0.5f);
            _frame.Clear(BufferBit.Colour);

            GL.LogicOp(GLEnum.And);
            State.LogicOPColour = true;

            Vector2 offset = (0d, maxHeight);
            int i = 0;
            int count = 0;
            while (count < compText.Length)
            {
                // No character
                if (text[i] == '\0' || text[i] == '\a')
                {
                    i++;
                    // Index in compressed text shouldn't be changed - it has no null characters
                    continue;
                }
                // Character should be skipped to add space
                if (text[i] == ' ')
                {
                    offset.X += font.SpaceWidth + charSpace;
                    i++;
                    // Index in compressed text shouldn't be changed - it has no white space
                    continue;
                }
                if (text[i] == '\t')
                {
                    offset.X += (font.SpaceWidth * TabSize) + charSpace;
                    i++;
                    // Index in compressed text shouldn't be changed - it has no white space
                    continue;
                }
                // Character should be skipped - offsetCurrent adjusted for new line
                if (text[i] == '\n')
                {
                    // Sometimes there is both
                    if (text.Length > (i + 1) && text[i + 1] == '\r')
                    {
                        i++;
                        continue;
                    }

                    offset.Y -= font.LineHeight + lineSpace;
                    offset.X = 0;
                    i++;
                    // Index in compressed text shouldn't be changed - it has no white space
                    continue;
                }
                // New lines for some operating systems
                if (text[i] == '\r')
                {
                    // Sometimes there is both
                    if (text.Length > (i + 1) && text[i + 1] != '\n')
                    {
                        offset.Y -= font.LineHeight + lineSpace;
                        offset.X = 0;
                    }

                    i++;
                    // Index in compressed text shouldn't be changed - it has no white space
                    continue;
                }
                CharFontData charData = font.GetCharacterData(text[i]);

                if (!charData.Supported)
                {
                    throw new UnsupportedCharacterException(text[i], font);
                }

                Vector2I src = (Vector2I)(charData.TextureCoordOffset * texutreSize);
                Vector2I size = (Vector2I)(charData.TextureRefSize * texutreSize);
                Vector2I pos = (Vector2I)((offset + charData.ExtraOffset) * pixelPerDouble);
                //Console.WriteLine($"{_frame.Height} | {size.Y}");
                //_frame.CopyTexture(fontTexture, src.X, src.Y, 0, size.X, size.Y, pos.X, pos.Y);
                _source.CopyFrameBuffer(_frame,
                    new Rectangle((pos.X, pos.Y - (font.LineHeight * pixelPerDouble.Y)), size),
                    new Rectangle(src, size),
                    BufferBit.Colour, TextureSampling.Nearest);

                // Adjust offset for next character
                offset.X += charData.Size.X + charData.Buffer + charSpace;
                // Continue counters
                count++;
                i++;
            }

            State.LogicOPColour = false;

            // Bind framebuffer to draw to
            drawFrame.Bind();

            // Bind shader
            Bind();

            // Set texture slot
            SetUniformI(Uniforms[2], 0);

            Model = Matrix4.CreateScale(maxWidth, maxHeight) * Model;

            //_frame.Bind(0);
            _frame.GetTexture(FrameAttachment.Colour0).Bind(0);

            _drawable.Draw();
        }
    }
}
