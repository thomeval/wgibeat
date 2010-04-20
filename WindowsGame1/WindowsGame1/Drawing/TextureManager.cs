using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1.Drawing
{
    public static class TextureManager
    {
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        public static Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>();

        public static void AddTexture(string name, Texture2D tex)
        {
            Textures.Add(name,tex);
        }

        public static void AddFont(string name, SpriteFont font)
        {
            Fonts.Add(name,font);
        }
    }
}
