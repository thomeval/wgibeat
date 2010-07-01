using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing.Sets;

namespace WGiBeat.Drawing
{
    public class LevelBar : DrawableObject
    {
        public Vector2 TextPosition { get; set; }
        public Vector2 BarPosition { get; set; }
        private SpriteMap _barSprite;
        private Sprite _baseSprite;
        private bool _spritesInit;

        public LevelBarSet Parent { get; set; }
        public int PlayerID { get; set; }

        //TODO: Add effect when level increases.

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_spritesInit)
            {
                InitSprites();
            }

            var maxWidth = this.Width - 30;

                _baseSprite.SetPosition(this.X, this.Y);
                _baseSprite.Draw(spriteBatch);
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + (int)Parent.Players[PlayerID].Level,
                       TextPosition, Color.Black);

                double levelFraction = Parent.Players[PlayerID].Level - Math.Floor(Parent.Players[PlayerID].Level);
                var barWidth = (int)(levelFraction * maxWidth);

                if (Parent.Players[PlayerID].Level == Parent.Players[PlayerID].MaxDifficulty())
                {
                    barWidth = maxWidth;
                }

                _barSprite.Draw(spriteBatch, (int)Parent.Players[PlayerID].Level - 1, barWidth, this.Height-6, BarPosition);
            
        }

        private void InitSprites()
        {
            _baseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures["levelBarBase"]
            };

            _barSprite = new SpriteMap
            {
                Columns = 1,
                Rows = 12,
                SpriteTexture = TextureManager.Textures["levelBarFronts"]
            };
            _spritesInit = true;
        }
    }
}
