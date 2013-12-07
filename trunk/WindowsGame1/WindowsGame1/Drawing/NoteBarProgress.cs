using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class NoteBarProgress :DrawableObject
    {
        private Sprite3D _baseSprite;
        private Sprite3D _frontSprite;
        private Sprite3D _readySprite;
        private double _opacity;
        private const int READY_FADEIN_SPEED = 2500;
        private const int PROGRESS_UPDATE_SPEED = 10;
        public int Value { get; set; }
        public int Maximum { get; set; }
        public int ID { get; set; }

        private double _displayedValue;
        public int TextureSet { get; set; }
        private void InitSprites()
        {
            _baseSprite = new Sprite3D
                              {
                                  Texture = TextureManager.Textures("NoteBarProgressBase" + TextureSuffix),
                                  Position = this.Position.Clone(),
                                  Height = this.Height,
                                  Width = this.Width
                              };
            _frontSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("NoteBarProgressFront" + TextureSuffix),
                Position = this.Position.Clone(),
                Height = this.Height,
                Width = this.Width
            };
            _readySprite = new Sprite3D
            {
                Texture = TextureManager.Textures("NoteBarProgressReady" + TextureSuffix),
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
            _baseSprite.Draw();

            if (Maximum == 0)
            {
                return;
            }
            Value = Math.Min(Value, Maximum);

            var diff = Value - _displayedValue;

            var changeMx = Math.Min(0.5, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * PROGRESS_UPDATE_SPEED);
            _displayedValue += (diff * (changeMx));
           
            var drawHeight = this.Height * _displayedValue / Maximum;
            var texHeight = (int) (_frontSprite.Texture.Height*_displayedValue/Maximum);
            _frontSprite.Height = (int) drawHeight;
            _frontSprite.Y = this.Y + this.Height - (int) drawHeight;
            _frontSprite.DrawTiled(0, _frontSprite.Texture.Height  -  texHeight, _frontSprite.Width, texHeight);
            DrawReadyIndicator();


        }

        private void DrawReadyIndicator()
        {
            if (Value == Maximum)
            {
                _opacity = Math.Min(_opacity + (TextureManager.LastDrawnPhraseDiff*READY_FADEIN_SPEED), 255);
                _readySprite.ColorShading.A = (byte) _opacity;
                _readySprite.Draw();
            }
            else
            {
                _opacity = 0;
            }
        }
    }
}
