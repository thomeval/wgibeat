using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1.Drawing
{
    public class SpriteMap
    {

        public Texture2D SpriteTexture { get; set; }
        public Color Color = Color.White;
        public int TextureWidth { get; set;}
        public int TextureHeight { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }

        public void Draw(SpriteBatch spriteBatch, int cellnumber, int width, int height, int x, int y)
        {
            Rectangle sourceRect = CalculateSourceRectangle(cellnumber);
            var destRect = new Rectangle {Height = height, Width = width, X = x, Y = y};

            spriteBatch.Draw(SpriteTexture, destRect, sourceRect, Color);
        }

        public void Draw(SpriteBatch spriteBatch, int cellnumber, int width, int height, Vector2 position)
        {
            Draw(spriteBatch, cellnumber,width,height,(int) position.X, (int) position.Y);
        }
        private Rectangle CalculateSourceRectangle(int cellnumber)
        {
            int xOffset = 0, yOffset = 0;
            int xSize = 0, ySize = 0;

            xSize = TextureWidth/Columns;
            ySize = TextureHeight/Rows;

            while (cellnumber >= Columns)
            {
                yOffset++;
                cellnumber -= Columns;
            }
            xOffset = cellnumber;
            yOffset *= ySize;
            xOffset *= xSize;
            var sourceRect = new Rectangle{Height = ySize, Width = xSize, X = xOffset, Y = yOffset};
            return sourceRect;
        }
    }
}
