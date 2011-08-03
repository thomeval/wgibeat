using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class SpriteMap
    {

        public Texture2D SpriteTexture { get; set; }
        public Color ColorShading = Color.White;
        public int Columns { get; set; }
        public int Rows { get; set; }

        public SpriteMap()
        {
            Columns = 1;
            Rows = 1;
        }

        public void Draw(SpriteBatch spriteBatch, int cellnumber, int width, int height, int x, int y)
        {

            Rectangle sourceRect = CalculateSourceRectangle(cellnumber);
            var destRect = new Rectangle
                               {
                                   Height =  (int) Math.Ceiling(height * Sprite.Multiplier.Y),
                                   Width = (int)Math.Ceiling(width * Sprite.Multiplier.X),
                                   X = (int)Math.Ceiling(x * Sprite.Multiplier.X),
                                   Y = (int)Math.Ceiling(y * Sprite.Multiplier.Y)
                               };
            spriteBatch.Draw(SpriteTexture, destRect, sourceRect, ColorShading);
            
        }

        public void Draw(SpriteBatch spriteBatch, int cellnumber, int width, int height, int x, int y, float rotation)
        {
            Rectangle sourceRect = CalculateSourceRectangle(cellnumber);
            var destRect = new Rectangle
            {
                Height = (int)Math.Ceiling(height * Sprite.Multiplier.Y),
                Width = (int)Math.Ceiling(width * Sprite.Multiplier.X),
                X = (int)Math.Ceiling(x * Sprite.Multiplier.X),
                Y = (int)Math.Ceiling(y * Sprite.Multiplier.Y) 
            };
            var origin = new Vector2 {X = destRect.Width/2, Y = destRect.Height/2};
            destRect.X += (int) origin.X;
            destRect.Y += (int) origin.Y;

            spriteBatch.Draw(SpriteTexture, destRect, sourceRect, ColorShading,rotation,origin, SpriteEffects.None,0);
        }


        public void Draw(SpriteBatch spriteBatch, int cellnumber, int width, int height, int x, int y, SpriteEffects flip)
        {
            Rectangle sourceRect = CalculateSourceRectangle(cellnumber);
            var destRect = new Rectangle
            {
                Height = (int)Math.Ceiling(height * Sprite.Multiplier.Y),
                Width = (int)Math.Ceiling(width * Sprite.Multiplier.X),
                X = (int)Math.Ceiling(x * Sprite.Multiplier.X),
                Y = (int)Math.Ceiling(y * Sprite.Multiplier.Y)
            };

            spriteBatch.Draw(SpriteTexture, destRect, sourceRect, ColorShading, 0.0f, new Vector2(0,0), flip, 0);
        }
        public void Draw(SpriteBatch spriteBatch, int cellnumber, int width, int height, Vector2 position)
        {
            Draw(spriteBatch, cellnumber, width, height,(int) position.X, (int) position.Y);
        }
        private Rectangle CalculateSourceRectangle(int cellnumber)
        {
     
            int xOffset = 0, yOffset = 0;
            int xSize = 0, ySize = 0;

            xSize = SpriteTexture.Width / Columns;
            ySize = SpriteTexture.Height / Rows;


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

        public void Draw(SpriteBatch spriteBatch, int cellnumber, int x, int y)
        {
            Draw(spriteBatch,cellnumber,SpriteTexture.Width / Columns, SpriteTexture.Height / Rows,x,y);
        }

        public void Draw(SpriteBatch spriteBatch, int cellnumber, Vector2 position)
        {
            Draw(spriteBatch, cellnumber, SpriteTexture.Width / Columns, SpriteTexture.Height / Rows, position);
        }
    }
}
