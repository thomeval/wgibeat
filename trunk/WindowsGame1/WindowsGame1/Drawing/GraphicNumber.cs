using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class GraphicNumber
    {
       public SpriteMap3D SpriteMap { get; set; }
       public int SpacingAdjustment { get; set; }
        public void DrawNumber(SpriteBatch sb, int number, int x, int y, int width, int height)
        {
            string temp = "" + number;
            int offset = 0;
            foreach (char c in temp)
            {
                SpriteMap.Draw((int) Char.GetNumericValue(c),width,height,x + offset,y);
                offset += width + SpacingAdjustment;
            }
        }

        public void DrawNumber(SpriteBatch sb, int number, int x, int y)
        {
            DrawNumber(sb, number,x,y, SpriteMap.Texture.Width / SpriteMap.Columns, SpriteMap.Texture.Height / SpriteMap.Rows);
        }

        public void DrawNumber(SpriteBatch sb, int number, Vector2 position)
        {
            DrawNumber(sb,number,(int) position.X, (int) position.Y);
        }

        public void DrawNumber(SpriteBatch sb, int number, Vector2 position, int width, int height)
        {
            DrawNumber(sb,number, (int) position.X, (int) position.Y, width, height);
        }
    }
}
