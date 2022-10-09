using Zene.Graphics;
using Zene.Structs;

namespace GUITest
{
    public class DFFont : Font
    {
        public DFFont()
            : base(0.65, 1d, -0.31, -0.10)
        {
            // Load font image
            byte[] byteData = Bitmap.ExtractData("resources/DFFont.png", out int w, out int h);
            // Convert to one channel GLArray
            GLArray<Vector2<byte>> texData = new GLArray<Vector2<byte>>(w, h);
            for (int i = 0; i < texData.Length; i++)
            {
                texData[i] = new Vector2<byte>(byteData[i * 4], 0);
            }
            // Create and setup texture
            _texture = new Texture2D(TextureFormat.R8, TextureData.Byte)
            {
                WrapStyle = WrapStyle.EdgeClamp,
                MinFilter = TextureSampling.Blend,
                MagFilter = TextureSampling.Blend
            };
            // Asign data
            _texture.SetData(w, h, BaseFormat.Rg, texData);
        }

        private readonly Texture2D _texture;

        public override void BindTexture(uint slot) => _texture.Bind(slot);

        private const double _pixelHeight = 1d / 512d;
        private const double _pixelWidth = 1d / 512d;

        private static readonly Vector2 _texSize = new Vector2(_pixelWidth * 40, _pixelHeight * 40);
        private static readonly Vector2 _charSize = Vector2.One;

        private const double _charHeight = 1d / 26d;
        private static Vector2 GetSize(int px, int py) => (_pixelWidth * px, _pixelHeight * py);
        private static Vector2 GetChar(int pw, int ph) => (_charHeight * pw, _charHeight * ph);
        private static Vector2 CentreOffset(int ph) => (0d, (1d - _charHeight * ph) * -0.5);

        private static CharFontData GetChar(int ox, int oy, int sx, int sy)
        {
            return new CharFontData(
                GetSize(ox, oy),
                GetSize(sx, sy),
                GetChar(sx, sy),
                CentreOffset(sy));
        }
        private static CharFontData GetChar(int ox, int oy, int sx, int sy, int offset)
        {
            return new CharFontData(
                GetSize(ox, oy),
                GetSize(sx, sy),
                GetChar(sx, sy),
                CentreOffset(sy + offset));
        }

        private static readonly CharFontData[] _characterData = new CharFontData[]
        {
            // !
            GetChar(2, 483, 12, 27),
            /*
            new CharFontData(
                Vector2.Zero,
                new Vector2(1),
                new Vector2(20),
                Vector2.Zero),*/
            // "
            GetChar(16, 494, 16, 16, 10),
            // #
            GetChar(34, 484, 23, 26),
            // $
            GetChar(59, 481, 20, 29),
            // %
            GetChar(81, 483, 26, 27),
            // &
            GetChar(109, 483, 25, 27),
            // '
            GetChar(136, 494, 11, 16, 10),
            // (
            GetChar(149, 480, 15, 30),
            // )
            GetChar(166, 480, 15, 30),
            // *
            GetChar(183, 490, 20, 20, 6),
            // +
            GetChar(205, 489, 20, 21),
            // ,
            GetChar(227, 495, 13, 15, -11),
            // -
            GetChar(242, 499, 15, 11),
            // .
            GetChar(259, 498, 12, 12, -14),
            // /
            GetChar(273, 484, 17, 26),
            // 0
            GetChar(292, 483, 20, 27),
            // 1
            GetChar(314, 484, 15, 26),
            // 2
            GetChar(331, 484, 20, 26),
            // 3
            GetChar(353, 483, 20, 27),
            // 4
            GetChar(375, 484, 22, 26),
            // 5
            GetChar(399, 483, 20, 27),
            // 6
            GetChar(421, 483, 20, 27),
            // 7
            GetChar(443, 484, 20, 26),
            // 8
            GetChar(465, 483, 20, 27),
            // 9
            GetChar(487, 483, 20, 27),
            // :
            GetChar(242, 474, 12, 23),
            // ;
            GetChar(256, 470, 13, 26),
            // <
            GetChar(183, 467, 20, 21),
            // =
            GetChar(205, 472, 20, 15),
            // >
            GetChar(16, 461, 20, 21),
            // ?
            GetChar(271, 455, 18, 27),
            // @
            GetChar(314, 453, 28, 29),

            // Alphabet Caps
            GetChar(344, 455, 24, 26),
            GetChar(375, 456, 21, 26),
            GetChar(398, 454, 22, 27),
            GetChar(422, 455, 23, 26),
            GetChar(38, 456, 18, 26),
            GetChar(291, 455, 18, 26),
            GetChar(447, 454, 23, 27),
            GetChar(472, 455, 22, 26),
            GetChar(227, 467, 11, 26),
            GetChar(81, 450, 15, 31, -5),
            GetChar(98, 455, 21, 26),
            GetChar(121, 455, 18, 26),
            GetChar(141, 452, 26, 26),
            GetChar(240, 442, 22, 26),
            GetChar(169, 438, 25, 27),
            GetChar(58, 453, 20, 26),
            GetChar(196, 434, 25, 31, -5),
            GetChar(2, 433, 21, 26),
            GetChar(370, 427, 20, 27),
            GetChar(25, 428, 22, 26),
            GetChar(264, 426, 22, 27),
            GetChar(288, 427, 23, 26),
            GetChar(472, 427, 30, 26),
            GetChar(422, 427, 22, 26),
            GetChar(344, 427, 22, 26),
            GetChar(98, 427, 21, 26),

            // [
            GetChar(223, 435, 15, 30),
            // \
            GetChar(121, 427, 17, 26),
            // ]
            GetChar(446, 422, 14, 30),
            // ^
            GetChar(392, 432, 21, 20, 6),
            // _
            GetChar(313, 441, 20, 10, -21),
            // `
            GetChar(49, 438, 14, 13, 13),

            // Alphabet lower case
            GetChar(140, 427, 19, 23, -4),
            GetChar(65, 420, 20, 28, 2),
            GetChar(240, 417, 18, 23, -4),
            GetChar(313, 411, 20, 28, 2),
            GetChar(161, 413, 20, 23, -4),
            GetChar(183, 405, 18, 27, 1),
            GetChar(203, 404, 21, 28, -9),
            GetChar(2, 404, 19, 27, 1),
            GetChar(49, 410, 12, 26),
            GetChar(392, 398, 15, 32, -6),
            GetChar(23, 399, 19, 27, 1),
            GetChar(226, 406, 11, 27, 1),
            GetChar(409, 403, 27, 22, -4),
            GetChar(368, 403, 19, 22, -4),
            GetChar(335, 402, 21, 23, -4),
            GetChar(462, 397, 20, 28, -9),
            GetChar(87, 397, 20, 28, -9),
            GetChar(484, 403, 16, 22, -4),
            GetChar(288, 402, 18, 23, -4),
            GetChar(109, 400, 16, 25, -1),
            GetChar(127, 403, 20, 22, -4),
            GetChar(260, 403, 21, 21, -5),
            GetChar(149, 390, 27, 21, -5),
            GetChar(438, 399, 21, 21, -5),
            GetChar(63, 391, 21, 27, -10),
            GetChar(239, 394, 19, 21, -5),

            // {
            GetChar(308, 379, 17, 30),
            // |
            GetChar(44, 375, 11, 33),
            // }
            GetChar(178, 373, 17, 30),
            // ~
            GetChar(197, 390, 20, 12),
        };

        public override CharFontData GetCharacterData(char character)
        {
            int char33 = character - 33;

            if (char33 >= 0 &&
                char33 < _characterData.Length)
            {
                return _characterData[character - 33];
            }

            switch (character)
            {
                case '£':
                    return new CharFontData(
                        new Vector2(_pixelWidth * 120, _pixelHeight * 200),
                        _texSize,
                        _charSize,
                        Vector2.Zero);
            }

            return CharFontData.Unsupported;
        }
    }
}