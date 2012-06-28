using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Players;

namespace WGiBeat.Drawing
{
    public class GrooveMomentumBar : DrawableObject
    {
        private Sprite _barSpriteBack;
        private Sprite _barSpriteFront;

        private double _displayedGrooveMomentum = 1.0;

        public double DisplayedGrooveMomentum
        {
            get { return _displayedGrooveMomentum; }
            set { _displayedGrooveMomentum = value; }
        }

        public Vector2 BarOffset { get; set; }

        public override void Draw(SpriteBatch spriteBatch)
        {
            
            if (_barSpriteBack == null)
            {
                _barSpriteBack = new Sprite { SpriteTexture = TextureManager.Textures("GrooveMomentumBarBack") };
                _barSpriteFront = new Sprite { SpriteTexture = TextureManager.Textures("GrooveMomentumBarFront") };
            }

            _barSpriteBack.Position = this.Position;
            _barSpriteBack.Size = this.Size;
            _barSpriteFront.Position = this.Position + this.BarOffset;
            _barSpriteFront.Size = this.Size - this.BarOffset;
            _barSpriteBack.Draw(spriteBatch);

            var actMx = _displayedGrooveMomentum - 0.5;
            const double MAX_MX = 6.5;
            if (actMx > 3.5)
            {
                actMx -= (actMx - 3.5)/2;
            }
            actMx = Math.Min(MAX_MX, actMx);

            var width = (int) (_barSpriteFront.Width/MAX_MX* (actMx));
            var textPosition = this.Position.Clone();
            textPosition.X += 70;
            textPosition.Y += 2;
            _barSpriteFront.Width = width;
            _barSpriteFront.DrawTiled(spriteBatch, 0, 0, width, _barSpriteFront.Height);
           

            TextureManager.DrawString(spriteBatch, string.Format("{0:0.0}x", _displayedGrooveMomentum), "LargeFont",
                                      textPosition, Color.Black, FontAlign.RIGHT);
            textPosition.Y -= 4;
            textPosition.X += 77;
            TextureManager.DrawString(spriteBatch, string.Format("{0:0.0}x", Player.PeakGrooveMomentum), "DefaultFont",
                          textPosition, Color.Black, FontAlign.RIGHT);
        }

 
    }
}
