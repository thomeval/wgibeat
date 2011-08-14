using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing.Sets;

namespace WGiBeat.Drawing
{
    public class LevelBar : DrawableObject
    {
       
        private Vector2 _textPosition;
        private Vector2 _barPosition;
        private SpriteMap _barSprite;
        private Sprite _baseSprite;
        private Sprite _maxBaseSprite;
        private bool _spritesInit;
        private int _lastLevelDrawn;
        private byte _lastLevelOpacity;

        public LevelBarSet Parent { get; set; }
        public int PlayerID { get; set; }

        private bool LevelBarFull
        {
            get { return Parent.Players[PlayerID].Level == Parent.Players[PlayerID].MaxArrowLevel(); }
        }

        //TODO: Possible solution to constantly setting position of sprites. Investigate.
        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;

                _textPosition = this.Position.Clone();
                _textPosition.X += 201;
                _barPosition = this.Position.Clone();
                _barPosition.X += 3;
                _barPosition.Y += 3;


            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch,0.0);
        }
        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            if (!_spritesInit)
            {
                InitSprites();
            }

            _baseSprite.Position = this.Position;
            _baseSprite.Draw(spriteBatch);
            _maxBaseSprite.ColorShading = Parent.MaxHighlightColors[PlayerID];
            _maxBaseSprite.ColorShading.A = _lastLevelOpacity;

            if (LevelBarFull)
            {
                phraseNumber *= 4;
                var beatFraction = (phraseNumber) - Math.Floor(phraseNumber);    
                _maxBaseSprite.ColorShading.A = (byte) ((1-beatFraction)*255);
                
            }
            _maxBaseSprite.Position = this.Position;
            _maxBaseSprite.Draw(spriteBatch);
                TextureManager.DrawString(spriteBatch, "" + (int)Parent.Players[PlayerID].Level, "DefaultFont",
                       _textPosition, Color.Black,FontAlign.CENTER);
            DrawBars(spriteBatch);
                     
        }

        private void DrawBars(SpriteBatch spriteBatch)
        {
            //The maximum possible width of the bar.
            var maxWidth = this.Width - 32;
            //The current progress towards the next level.
            double levelFraction = Parent.Players[PlayerID].Level - Math.Floor(Parent.Players[PlayerID].Level);
            //The calculated width of the level bar.
            var barWidth = (int)(levelFraction * maxWidth);

            if (LevelBarFull)
            {
                barWidth = maxWidth;
            }

            _lastLevelDrawn = Math.Min(_lastLevelDrawn, (int)Parent.Players[PlayerID].Level - 1);

            //Level maxed out, draw a full bar.
            if (Math.Floor(Parent.Players[PlayerID].Level) - 1 > _lastLevelDrawn)
            {
                _lastLevelDrawn = (int)Parent.Players[PlayerID].Level - 1;
                _lastLevelOpacity = 255;
            }
            //Draw the last level bar (gradually fading out) if appropriate.
            if (_lastLevelDrawn > 0)
            {
                _barSprite.ColorShading.A = _lastLevelOpacity;
                _barSprite.Draw(spriteBatch, _lastLevelDrawn - 1, maxWidth, this.Height - 6, _barPosition );
                _lastLevelOpacity = (byte)Math.Max(_lastLevelOpacity - 5, 0);
            }

            //Draw the current level bar.
            _barSprite.ColorShading.A = 255;
            _barSprite.Draw(spriteBatch, (int)Parent.Players[PlayerID].Level - 1, barWidth, this.Height - 6, _barPosition);
        }

        private void InitSprites()
        {
            _baseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("LevelBarBase")
            };

            _maxBaseSprite = new Sprite
                                 {
                                     SpriteTexture = TextureManager.Textures("LevelBarMaxBase")
                                 };

            _barSprite = new SpriteMap
            {
                Columns = 1,
                Rows = 12,
                SpriteTexture = TextureManager.Textures("LevelBarFronts")
            };
            _spritesInit = true;
        }
    }
}
