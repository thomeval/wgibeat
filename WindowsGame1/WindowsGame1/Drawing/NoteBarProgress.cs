using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class NoteBarProgress :DrawableObject
    {
        private Sprite _baseSprite;
        private Sprite _frontSprite;
        private Sprite _readySprite;
        private double _opacity;
        private const int READY_FADEIN_SPEED = 4000;

        public int Value { get; set; }
        public int Maximum { get; set; }
        public int ID { get; set; }

        public int TextureSet { get; set; }
        private void InitSprites()
        {
            _baseSprite = new Sprite
                              {
                                  SpriteTexture = TextureManager.Textures("NoteBarProgressBase" + TextureSuffix),
                                  Position = this.Position.Clone(),
                                  Height = this.Height,
                                  Width = this.Width
                              };
            _frontSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("NoteBarProgressFront" + TextureSuffix),
                Position = this.Position.Clone(),
                Height = this.Height,
                Width = this.Width
            };
            _readySprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("NoteBarProgressReady" + TextureSuffix),
                Position = this.Position.Clone(),
                Height = this.Height,
                Width = this.Width
            };
        }

        private string TextureSuffix
        {
            get
            {
                if (TextureSet < 2)
                {
                    return "";
                }
                return TextureSet + "x";
            }
         
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            if (_baseSprite == null)
            {
                InitSprites();
            }

            Debug.Assert(_baseSprite != null);
            _baseSprite.Draw(spriteBatch);

            if (Maximum == 0)
            {
                return;
            }
            Value = Math.Min(Value, Maximum);
            var drawHeight = this.Height * Value / Maximum;
            var texHeight = _frontSprite.SpriteTexture.Height*Value/Maximum;
            _frontSprite.Height = drawHeight;
            _frontSprite.Y = this.Y + this.Height - drawHeight;
            _frontSprite.DrawTiled(spriteBatch, 0, _frontSprite.SpriteTexture.Height  - texHeight, _frontSprite.Width, texHeight);
            if (Value == Maximum)
            {
             
                _opacity = Math.Min(_opacity + (TextureManager.LastDrawnPhraseDiff * READY_FADEIN_SPEED), 255);

                _readySprite.ColorShading.A = (byte) _opacity;
                _readySprite.Draw(spriteBatch);

            }
            else
            {
                _opacity = 0;

            }


        }
    }
}
