using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
            Rectangle boundingBox = new Rectangle(X, Y, Width, Height);
                spriteBatch.Draw(SpriteTexture, boundingBox, ColorShading);

        }
        public void DrawTiled(SpriteBatch spriteBatch, int texU1, int texV1, int texU2, int texV2)
        {
            Rectangle textureRect = new Rectangle(texU1,texV1,texU2,texV2);
            Rectangle dest = new Rectangle(X,Y, Width,Height);
            spriteBatch.Draw(SpriteTexture, dest, textureRect, Color.White);
        }
    }
}
