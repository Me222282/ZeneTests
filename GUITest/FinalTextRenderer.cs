﻿using System;
using System.Collections.Generic;
using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace GUITest
{
    public class FinalTextRenderer : IDisposable
    {
        private class M1Shader : BaseShaderProgram
        {
            public M1Shader()
            {
                Create(ShaderPresets.BasicVertex, File.ReadAllText("resources/textfrag.shader"),
                    "matrix", "uColour", "uTextureSlot");

                // Set matrices in shader to default
                SetMatrix(Matrix4.Identity);
            }

            public void SetMatrix(Matrix4 m)
            {
                SetUniformF(Uniforms[0], ref m);
            }

            public Colour Colour
            {
                set
                {
                    SetUniformF(Uniforms[1], (Vector4)(ColourF)value);
                }
            }
        }
        private class M2Shader : BaseShaderProgram
        {
            public M2Shader()
            {
                Create(ShaderPresets.TextVert, File.ReadAllText("resources/textfrag.shader"),
                    "matrix", "uColour", "uTextureSlot");

                // Set matrices in shader to default
                SetMatrix(Matrix4.Identity);
            }

            public void SetMatrix(Matrix4 m)
            {
                SetUniformF(Uniforms[0], ref m);
            }

            public Colour Colour
            {
                set
                {
                    SetUniformF(Uniforms[1], (Vector4)(ColourF)value);
                }
            }
        }

        private Matrix4 _m1 = Matrix4.Identity;
        public Matrix4 Model
        {
            get => _m1;
            set
            {
                _m1 = value;
            }
        }
        private Matrix4 _m2 = Matrix4.Identity;
        public Matrix4 View
        {
            get => _m2;
            set
            {
                _m2 = value;
            }
        }
        private Matrix4 _m3 = Matrix4.Identity;
        public Matrix4 Projection
        {
            get => _m3;
            set
            {
                _m3 = value;
            }
        }

        private Colour _colour;
        public Colour Colour
        {
            get => _colour;
            set
            {
                _colour = value;
                _m1Shader.Colour = value;
                _m2Shader.Colour = value;
            }
        }

        public FinalTextRenderer()
        {
            //
            // M1
            //

            // Create drawable object
            _m1Drawable = new DrawObject<Vector2, byte>(new Vector2[]
            {
                new Vector2(-0.5, 0.5), new Vector2(0, 1),
                new Vector2(-0.5, -0.5), new Vector2(0, 0),
                new Vector2(0.5, -0.5), new Vector2(1, 0),
                new Vector2(0.5, 0.5), new Vector2(1, 1)
            }, new byte[] { 0, 1, 2, 2, 3, 0 }, 2, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
            _m1Drawable.AddAttribute(2, 1, AttributeSize.D2); // Texture Coordinates

            _frame = new TextureRenderer(1, 1);
            _frame.SetColourAttachment(0, TextureFormat.R8);
            _frame.ClearColour = new ColourF(0f, 0f, 0f, 0f);
            _source = new Framebuffer();

            _m1Shader = new M1Shader();

            //
            // M2
            //

            _m2Capacity = 16;

            // Create drawable object
            _m2Drawable = new DrawObject<Vector2, byte>(new Vector2[]
            {
                new Vector2(0d, 0d), new Vector2(0d, 1d),
                new Vector2(0d, -1d), new Vector2(0d, 0d),
                new Vector2(1d, -1d), new Vector2(1d, 0d),
                new Vector2(1d, 0d), new Vector2(1d, 1d)
            }, new byte[] { 0, 1, 2, 2, 3, 0 }, 2, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
            _m2Drawable.AddAttribute(1, 1, AttributeSize.D2); // Texture Coordinates

            // Setup instance offsets ready for drawing
            _instanceData = new ArrayBuffer<Vector2>(_blockSize, BufferUsage.DrawRepeated);
            _instanceData.InitData(_m2Capacity * _blockSize);

            // Add instance reference
            _m2Drawable.Vao.AddInstanceBuffer(_instanceData, 2, 0, DataType.Double, AttributeSize.D2, 1);
            _m2Drawable.Vao.AddInstanceBuffer(_instanceData, 3, 1, DataType.Double, AttributeSize.D2, 1);
            _m2Drawable.Vao.AddInstanceBuffer(_instanceData, 4, 2, DataType.Double, AttributeSize.D2, 1);
            _m2Drawable.Vao.AddInstanceBuffer(_instanceData, 5, 3, DataType.Double, AttributeSize.D2, 1);
            // Colour
            //_drawable.Vao.AddInstanceBuffer(_instanceData, 6, 4, DataType.Double, AttributeSize.D4, 1);
            // Set indexes as instance referances

            _m2Shader = new M2Shader();

            // Default colour
            Colour = new Colour(255, 255, 255);
        }
        private readonly DrawObject<Vector2, byte> _m1Drawable;
        private readonly M1Shader _m1Shader;
        private readonly TextureRenderer _frame;
        private readonly Framebuffer _source;

        public int TabSize { get; set; } = 4;

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) { return; }

            _disposed = true;

            _m1Drawable.Dispose();
            _m1Shader.Dispose();
            _frame.Dispose();
            _source.Dispose();

            _m2Drawable.Dispose();
            _m2Shader.Dispose();
            _instanceData.Dispose();

            GC.SuppressFinalize(this);
        }

        public void DrawLeftBound(ReadOnlySpan<char> text, NewFont font, int charSpace, int lineSpace)
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

            _source[0] = font.SourceTexture;

            // Set frame size
            _frame.Size = font.GetFrameSize(text, charSpace, lineSpace, TabSize);

            _frame.Clear(BufferBit.Colour);

            Vector2I offset = (0, _frame.Height - font.LineHeight);
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
                NewCharFontData charData = font.GetCharacterData(text[i]);

                if (!charData.Supported)
                {
                    //throw new UnsupportedCharacterException(text[i], font);
                    throw new Exception();
                }

                Vector2I src = charData.TexturePosision;
                Vector2I size = charData.Size;
                Vector2I pos = offset + charData.ExtraOffset;
                //Console.WriteLine($"{_frame.Height} | {size.Y}");
                //_frame.CopyTexture(fontTexture, src.X, src.Y, 0, size.X, size.Y, pos.X, pos.Y);
                _source.CopyFrameBuffer(_frame,
                    new Rectangle(pos, size),
                    new Rectangle(src, size),
                    BufferBit.Colour, TextureSampling.Nearest);

                // Adjust offset for next character
                offset.X += charData.Size.X + charData.Buffer + charSpace;
                // Continue counters
                count++;
                i++;
            }

            // Bind framebuffer to draw to
            drawFrame.Bind();

            // Bind shader
            _m1Shader.Bind();

            // Set texture slot - already 0
            //_m1Shader.SetUniformI(Uniforms[2], 0);

            Matrix4 m1 = Matrix4.CreateScale(_frame.Size / (Vector2)font.LineHeight) * _m1;
            _m1Shader.SetMatrix(m1 * _m2 * _m3);

            //_frame.Bind(0);
            _frame.GetTexture(FrameAttachment.Colour0).Bind(0);

            _m1Drawable.Draw();
        }

        private readonly DrawObject<Vector2, byte> _m2Drawable;
        private readonly M2Shader _m2Shader;
        private readonly ArrayBuffer<Vector2> _instanceData;

        private const int _blockSize = 4;
        private int _m2Capacity;
        private void SetCap(int v)
        {
            _m2Capacity = v;

            _instanceData.InitData(_m2Capacity * _blockSize);
        }

        public void DrawCentred(ReadOnlySpan<char> text, NewFont font, int charSpace, int lineSpace)
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

            if (compText.Length > _m2Capacity)
            {
                SetCap(compText.Length);
            }

            // Pixel space to normalised space
            Vector2 textureMultiplier = 1d /
                // Texture Size
                (Vector2)(font.SourceTexture.Properties.Width, font.SourceTexture.Properties.Height);
            double sizeMultiplier = 1d / font.LineHeight;

            double cs = charSpace * sizeMultiplier;
            double sw = font.SpaceWidth * sizeMultiplier;

            // The widths of each line in text
            List<double> lineWidths = font.GetLineWidths(text, cs, TabSize, sizeMultiplier);

            // The current character offset
            Vector2 offsetCurrent = new Vector2(
                lineWidths[0] * -0.5,
                // The offset for Y
                (
                    lineWidths.Count +
                    ((lineWidths.Count - 1) * lineSpace)
                ) * 0.5);
            // The instance data containing offsets for each character
            Vector2[] data = new Vector2[compText.Length * _blockSize];

            // Special for loop with two counters
            int i = 0;
            int count = 0;
            // The current line in the text that is being calculated
            int lineCurrent = 0;
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
                    offsetCurrent.X += sw + cs;
                    i++;
                    // Index in compressed text shouldn't be changed - it has no white space
                    continue;
                }
                if (text[i] == '\t')
                {
                    offsetCurrent.X += (sw * TabSize) + cs;
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

                    offsetCurrent.Y -= 1d + lineSpace;
                    lineCurrent++;
                    offsetCurrent.X = lineWidths[lineCurrent] * -0.5;
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
                        offsetCurrent.Y -= 1d + lineSpace;
                        lineCurrent++;
                        offsetCurrent.X = lineWidths[lineCurrent] * -0.5;
                    }

                    i++;
                    // Index in compressed text shouldn't be changed - it has no white space
                    continue;
                }
                NewCharFontData charData = font.GetCharacterData(text[i]);

                if (!charData.Supported)
                {
                    //throw new UnsupportedCharacterException(text[i], font);
                    throw new Exception();
                }

                Vector2 size = charData.Size * sizeMultiplier;

                // Set drawing offset data
                data[count * _blockSize] = offsetCurrent + (charData.ExtraOffset * sizeMultiplier);
                data[count * _blockSize].Y -= 1d - size.Y;
                data[(count * _blockSize) + 1] = charData.TexturePosision * textureMultiplier;
                data[(count * _blockSize) + 2] = charData.Size * textureMultiplier;
                data[(count * _blockSize) + 3] = size;

                // Adjust offset for next character
                offsetCurrent.X += size.X + (charData.Buffer * sizeMultiplier) + cs;
                // Continue counters
                count++;
                i++;
            }
            // Pass instance data to gpu
            _instanceData.EditData(0, data);

            //
            // Draw object
            //

            // Bind shader
            _m2Shader.Bind();
            _m2Shader.SetMatrix(_m1 * _m2 * _m3);

            // Set texture slot - already 0
            //SetUniformI(Uniforms[2], 0);
            font.SourceTexture.Bind(0);

            _m2Drawable.DrawMultiple(compText.Length);
        }
    }
}
