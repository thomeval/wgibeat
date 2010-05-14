using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1.Drawing
{
    public class Sprite : DrawableObject
    {

        public Sprite()
        {
            ColorShading = Color.White;
        }

        public Color ColorShading;
        public Texture2D SpriteTexture;

        public override void Draw(SpriteBatch spriteBatch)
        {
            CheckIfDimensionsSet();
            var boundingBox = new Rectangle(X, Y, Width, Height);
                spriteBatch.Draw(SpriteTexture, boundingBox, ColorShading);
        }


        public void DrawTiled(SpriteBatch spriteBatch, int texU1, int texV1, int texU2, int texV2)
        {
            CheckIfDimensionsSet();
            var textureRect = new Rectangle(texU1,texV1,texU2,texV2);
            var dest = new Rectangle(X,Y, Width,Height);
            spriteBatch.Draw(SpriteTexture, dest, textureRect, Color.White);
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

    }
}
