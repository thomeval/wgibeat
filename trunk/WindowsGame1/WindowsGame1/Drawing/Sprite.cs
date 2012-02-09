using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class Sprite : DrawableObject
    {
        public static GameCore Core;

        public Sprite()
        {
            ColorShading = Color.White;
        }

        public Color ColorShading;
        public Texture2D SpriteTexture;

        public static GraphicsDevice Device;
        private VertexPositionTexture[] _vertices;

        public override void Draw(SpriteBatch spriteBatch)
        {
            SetupPrimitives();
            
            CheckIfDimensionsSet();
            var boundingBox = new Rectangle
            {
                Height = this.Height,
                Width = this.Width,
                X = this.X,
                Y = this.Y,
            };

           // SetupPrimitives();
        //    Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 1);
            spriteBatch.Draw(SpriteTexture,boundingBox,ColorShading);
        }

        private void SetupPrimitives()
        {
            _vertices = new[] {
                new VertexPositionTexture{Position = new Vector3{X = this.X, Y = this.Y},TextureCoordinate = new Vector2{X = 0, Y = 0}}, 
                new VertexPositionTexture{Position = new Vector3{X = this.X + this.Width, Y = this.Y},TextureCoordinate = new Vector2{X = 1, Y = 0}},
                new VertexPositionTexture{Position = new Vector3{X = this.X + this.Width, Y = this.Y + this.Height},TextureCoordinate = new Vector2{X = 1, Y = 1}}, 
                new VertexPositionTexture{Position = new Vector3{X = this.X, Y = this.Y + this.Height},TextureCoordinate = new Vector2{X = 0, Y = 1}}, };       
        }

        public void Draw(SpriteBatch spriteBatch, SpriteEffects flip)
        {
            CheckIfDimensionsSet();
            var boundingBox = new Rectangle
            {
                Height = this.Height,
                Width = this.Width,
                X = this.X,
                Y = this.Y,
            };

           // SetupPrimitives();
           // Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 1);
            spriteBatch.Draw(SpriteTexture, boundingBox, null, ColorShading, 0,new Vector2(0,0), flip, 0); 
        }

        public void DrawTiled(SpriteBatch spriteBatch, int texU1, int texV1, int texU2, int texV2)
        {
            DrawTiled(spriteBatch,texU1,texV1,texU2,texV2,SpriteEffects.None);
        }
        public void DrawTiled(SpriteBatch spriteBatch, int texU1, int texV1, int texU2, int texV2, SpriteEffects flip)
        {
            CheckIfDimensionsSet();
            //Ignore drawing 'zero' part of a texture.
            if ((texU2 == 0) || (texV2 == 0))
            {
                return;
            }

            Core.ShiftSpriteBatch(true);
            
            
            var textureRect = new Rectangle(texU1, texV1, texU2, texV2);
            var dest = new Rectangle
            {
                Height = this.Height,
                Width = this.Width,
                X = this.X,
                Y = this.Y,
            };

            spriteBatch.Draw(SpriteTexture, dest, textureRect, ColorShading,0.0f, new Vector2(0,0),flip,0.0f );
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
    }

}
