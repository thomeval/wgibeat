using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public static class TextureManager
    {
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        public static Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>();

        public static void AddTexture(string name, Texture2D tex)
        {
            Textures.Add(name, tex);
        }

        public static void AddFont(string name, SpriteFont font)
        {
            Fonts.Add(name, font);
        }

        public static void DrawString(SpriteBatch spriteBatch, string text, string fontName, Vector2 position, Color color, FontAlign align)
        {
            var measuredPosition = new Vector2(position.X, position.Y);

            switch (align)
            {

                    case FontAlign.CENTER:
                    measuredPosition.X -= Fonts[fontName].MeasureString(text).X/2;
                    break;
                    case FontAlign.RIGHT:
                    measuredPosition.X -= Fonts[fontName].MeasureString(text).X;
                    break;
            }
            spriteBatch.DrawString(Fonts[fontName], text, measuredPosition, color);
        }

    }

    public enum FontAlign
    {
        LEFT = 0,
        CENTER = 1,
        RIGHT = 2
    }
}
