using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1.Drawing
{
    public class GraphicNumber
    {
       public SpriteMap SpriteMap { get; set; }
       public int SpacingAdjustment { get; set; }
        public void DrawNumber(SpriteBatch sb, int number, int x, int y, int width, int height)
        {
            string temp = "" + number;
            int offset = 0;
            foreach (char c in temp)
            {
                SpriteMap.Draw(sb,(int) Char.GetNumericValue(c),width,height,x + offset,y);
                offset += width - SpacingAdjustment;
            }
        }

        public void DrawNumber(SpriteBatch sb, int number, int x, int y)
        {
            DrawNumber(sb, number,x,y, SpriteMap.TextureWidth / SpriteMap.Columns, SpriteMap.TextureHeight / SpriteMap.Rows);
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
