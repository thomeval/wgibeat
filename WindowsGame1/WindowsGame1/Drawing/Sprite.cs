using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class Sprite : DrawableObject
    {

        public Sprite()
        {
            ColorShading = Color.White;
        }

        public Color ColorShading;
        public Texture2D SpriteTexture;

        public static GraphicsDevice Device;

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            CheckIfDimensionsSet();
            var boundingBox = new Rectangle
                                  {
                                      Height = this.Height,
                                      Width = this.Width,
                                      X = this.X,
                                      Y = this.Y,
                                  };


            spriteBatch.Draw(SpriteTexture, boundingBox, ColorShading);
            spriteBatch.End();
        }

        public void Draw(SpriteBatch spriteBatch, SpriteEffects flip)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend,SpriteSortMode.Immediate,SaveStateMode.None);
            CheckIfDimensionsSet();
            var boundingBox = new Rectangle
            {
                Height = this.Height,
                Width = this.Width,
                X = this.X,
                Y = this.Y,
            };

            spriteBatch.Draw(SpriteTexture, boundingBox, null, ColorShading, 0,new Vector2(0,0), flip, 0);
            spriteBatch.End();
        }

        public void DrawTiled(SpriteBatch spriteBatch, int texU1, int texV1, int texU2, int texV2)
        {
            DrawTiled(spriteBatch,texU1,texV1,texU2,texV2,SpriteEffects.None);
        }
        public void DrawTiled(SpriteBatch spriteBatch, int texU1, int texV1, int texU2, int texV2, SpriteEffects flip)
        {
        
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            CheckIfDimensionsSet();
            //Ignore drawing 'zero' part of a texture.
            if ((texU2 == 0) || (texV2 == 0))
            {
                spriteBatch.End();
                return;
            }

            
            
            var textureRect = new Rectangle(texU1, texV1, texU2, texV2);
            var dest = new Rectangle
            {
                Height = this.Height,
                Width = this.Width,
                X = this.X,
                Y = this.Y,
            };

            spriteBatch.Draw(SpriteTexture, dest, textureRect, ColorShading,0.0f, new Vector2(0,0),flip,0.0f );

            spriteBatch.End();
            Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
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
