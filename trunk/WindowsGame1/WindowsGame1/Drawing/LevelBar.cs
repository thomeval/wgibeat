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
        private Sprite _maxFrontSprite;
        private bool _spritesInit;
        private int _lastLevelDrawn;
        private double _lastLevelOpacity;
        private double _maxFrontOpacity;

        public LevelBarSet Parent { get; set; }
        public int PlayerID { get; set; }

        private const int MAX_FRONT_SHOW_SPEED = 360;
        private const int MAX_FRONT_HIDE_SPEED = 750;
        private const int FRONT_BAR_CHANGE_SPEED = 16;
        private const int FULL_BAR_FADEOUT_SPEED = 300;
        private double _displayedLevel = 1;

        private bool LevelBarFull
        {
            get { return (int) (Parent.Players[PlayerID].Level) == Parent.Players[PlayerID].MaxArrowLevel(); }
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

            _maxFrontSprite = new Sprite {SpriteTexture = TextureManager.Textures("LevelBarMaxFront")};

            _spritesInit = true;
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
            _maxBaseSprite.ColorShading.A = Convert.ToByte(_lastLevelOpacity);

            var maxFrontBeatFraction = (phraseNumber / 4) - Math.Floor(phraseNumber / 4);
            var beatFraction = (phraseNumber * 4) - Math.Floor(phraseNumber * 4);

            if (LevelBarFull)
            {              
                _maxBaseSprite.ColorShading.A = (byte) ((1-beatFraction)*255);
                _maxFrontOpacity = Math.Min(255, _maxFrontOpacity + (TextureManager.LastDrawnPhraseDiff * MAX_FRONT_SHOW_SPEED));
            }
            else
            {
                _maxFrontOpacity = Math.Max(0, _maxFrontOpacity - (TextureManager.LastDrawnPhraseDiff * MAX_FRONT_HIDE_SPEED));
            }

            _maxFrontSprite.ColorShading.A = (byte)((_maxFrontOpacity));
            _maxFrontSprite.Position = _barPosition;
            _maxFrontSprite.Width = this.Width - 32;
            _maxFrontSprite.Height = this.Height - 6;
            _maxBaseSprite.Position = this.Position;
            _maxBaseSprite.Draw(spriteBatch);
                TextureManager.DrawString(spriteBatch, "" + (int)Parent.Players[PlayerID].Level, "DefaultFont",
                       _textPosition, Color.Black,FontAlign.CENTER);

             DrawBars(spriteBatch);
            _maxFrontSprite.DrawTiled(spriteBatch, (int)(maxFrontBeatFraction * _maxFrontSprite.Width), 0, _maxFrontSprite.Width, _maxFrontSprite.Height);
         
        }

        private void DrawBars(SpriteBatch spriteBatch)
        {
            var diff = Parent.Players[PlayerID].Level - _displayedLevel;
            _displayedLevel += diff * Math.Min(TextureManager.LastDrawnPhraseDiff * FRONT_BAR_CHANGE_SPEED, 0.5);

            //The maximum possible width of the bar.
            var maxWidth = this.Width - 32;
            //The current progress towards the next level.
            double levelFraction = _displayedLevel - Math.Floor(_displayedLevel);
            //The calculated width of the level bar.
            var barWidth = (int)(levelFraction * maxWidth);

            if (LevelBarFull)
            {
                barWidth = maxWidth;       
            }

            _lastLevelDrawn = Math.Min(_lastLevelDrawn, (int)_displayedLevel - 1);

            //Level maxed out, draw a full bar.
            if (Math.Floor(_displayedLevel) - 1 > _lastLevelDrawn)
            {
                _lastLevelDrawn = (int)Parent.Players[PlayerID].Level - 1;
                _lastLevelOpacity = 255;
            }
            //Draw the last level bar (gradually fading out) if appropriate.
            if (_lastLevelDrawn > 0)
            {
                _barSprite.ColorShading.A = Convert.ToByte(_lastLevelOpacity);
                _barSprite.Draw(spriteBatch, _lastLevelDrawn - 1, maxWidth, this.Height - 6, _barPosition );
                diff = TextureManager.LastDrawnPhraseDiff*FULL_BAR_FADEOUT_SPEED;
                _lastLevelOpacity = Math.Max(_lastLevelOpacity - diff, 0);
            }

            //Draw the current level bar.
            _barSprite.ColorShading.A = LevelBarFull ? (byte) 255 : (byte) (40 + (215 * levelFraction));
            _barSprite.Draw(spriteBatch, (int) _displayedLevel -1, barWidth, this.Height - 6, _barPosition);
        }

    }
}
