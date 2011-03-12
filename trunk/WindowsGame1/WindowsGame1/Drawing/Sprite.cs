using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class Sprite : DrawableObject
    {
        public static Vector2 Multiplier = new Vector2(1,1);
        public static GameCore Core;

        public const int BASE_WIDTH = 800;
        public const int BASE_HEIGHT = 600;


        public Sprite()
        {
            ColorShading = Color.White;
        }

        public Color ColorShading;
        public Texture2D SpriteTexture;

       
        public override void Draw(SpriteBatch spriteBatch)
        {
            CheckIfDimensionsSet();
            var boundingBox = new Rectangle
                                  {
                                      X = (int)Math.Ceiling(this.X * Multiplier.X),
                                      Y = (int)Math.Ceiling(this.Y * Multiplier.Y),
                                      Height = (int) Math.Ceiling(this.Height * Multiplier.Y),
                                      Width = (int)Math.Ceiling(this.Width * Multiplier.X),
                                  };

         //   boundingBox = AdjustForDrift(boundingBox);
            spriteBatch.Draw(SpriteTexture, boundingBox, ColorShading);
        }

        //Doesn't work - No solution found for sprite tearing yet.
        private static Rectangle AdjustForDrift(Rectangle boundingBox)
        {
           var drift = Math.Ceiling(boundingBox.X * Multiplier.X) - (boundingBox.X * Multiplier.X);
           drift  += Math.Ceiling(boundingBox.Width * Multiplier.X) - (boundingBox.Width * Multiplier.X);

            if (drift >= 0.5)
            {
                boundingBox.Width--;
            }

            drift = Math.Ceiling(boundingBox.Y * Multiplier.Y) - (boundingBox.Y * Multiplier.Y);
            drift += Math.Ceiling(boundingBox.Height * Multiplier.Y) - (boundingBox.Height * Multiplier.Y);

            if (drift >= 0.5)
            {
                boundingBox.Height--;
            }

            return boundingBox;
        }


        public void DrawTiled(SpriteBatch spriteBatch, int texU1, int texV1, int texU2, int texV2)
        {
            Core.ShiftSpriteBatch(true);
            
            CheckIfDimensionsSet();
            var textureRect = new Rectangle(texU1, texV1, texU2, texV2);
            var dest = new Rectangle
            {
                X = (int)Math.Ceiling(this.X * Multiplier.X),
                Y = (int)Math.Ceiling(this.Y * Multiplier.Y),
                Height = (int)Math.Ceiling(this.Height * Multiplier.Y),
                Width = (int)Math.Ceiling(this.Width * Multiplier.X),
            };

            spriteBatch.Draw(SpriteTexture, dest, textureRect, ColorShading);

            Core.ShiftSpriteBatch(false);
            
        }

        /// <summary>
        /// Checks if the Width and Height of the Sprite have been set. If not, the width and height
        /// are determined from the texture itself.
        /// </summary>
        private void CheckIfDimensionsSet()
        {
            if (Width == 0)
            {
                Width = SpriteTexture.Width;
            }
            if (Height == 0)
            {
                Height = SpriteTexture.Height;
            }
        }

        public static void SetMultiplier(int width, int height)
        {
            Multiplier.X = (float) (1.0*width/BASE_WIDTH);
            Multiplier.Y = (float) (1.0*height/BASE_HEIGHT);
        }
    }
}
