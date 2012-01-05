using System;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Players;

namespace WGiBeat.Drawing
{
    public class GrooveMomentumBar : DrawableObject
    {
        private Sprite _barSpriteBack;
        private Sprite _barSpriteFront;

        private double _displayedGrooveMomentum = 1.0;

        public override void Draw(SpriteBatch spriteBatch)
        {
            UpdateDisplayedGM();
            if (_barSpriteBack == null)
            {
                _barSpriteBack = new Sprite { SpriteTexture = TextureManager.Textures("GrooveMomentumBarBack") };
                _barSpriteFront = new Sprite { SpriteTexture = TextureManager.Textures("GrooveMomentumBarFront") };
            }

            _barSpriteBack.Position = this.Position;
            _barSpriteFront.Position = this.Position;
            _barSpriteBack.Draw(spriteBatch);

            var actMx = _displayedGrooveMomentum - 0.5;
            const double MAX_MX = 5.5;
            if (actMx > 3.5)
            {
                actMx -= (actMx - 3.5)/2;
            }
            actMx = Math.Min(MAX_MX, actMx);

            var width = (int) (this.Width/MAX_MX* (actMx));
            var textPosition = this.Position.Clone();
            textPosition.X += 10;
            textPosition.Y += 7;
            _barSpriteFront.Width = width;
            _barSpriteFront.DrawTiled(spriteBatch, 0, 0, width, _barSpriteFront.Height);
           

            TextureManager.DrawString(spriteBatch, string.Format("{0:0.0}x", _displayedGrooveMomentum), "DefaultFont",
                                      textPosition, Color.Black, FontAlign.LEFT);
            textPosition.Y -= 4;
            textPosition.X += 112;
            TextureManager.DrawString(spriteBatch, string.Format("{0:0.0}x", Player.PeakGrooveMomentum), "DefaultFont",
                          textPosition, Color.Black, FontAlign.RIGHT);
        }

        private void UpdateDisplayedGM()
        {
            var diff = Player.GrooveMomentum - _displayedGrooveMomentum;
            if (Math.Abs(diff) < 0.001)
            {
                _displayedGrooveMomentum = Player.GrooveMomentum;
            }
            else
            {
                _displayedGrooveMomentum += diff / 8.0;
            }      
           
        }
    }
}
