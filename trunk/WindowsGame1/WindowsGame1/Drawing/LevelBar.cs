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
        private SpriteMap3D _barSprite;
        private Sprite3D _baseSprite;
        private Sprite3D _maxBaseSprite;
        private Sprite3D _maxFrontSprite;
        private bool _spritesInit;
        private int _lastLevelDrawn;
        private double _lastLevelOpacity;
        private double _maxFrontOpacity;

        public LevelBarSet Parent { get; set; }
        public int PlayerID { get; set; }
        public double Multiplier { get; set; }

        private const int MAX_FRONT_SHOW_SPEED = 360;
        private const int MAX_FRONT_HIDE_SPEED = 750;
        private const int FRONT_BAR_CHANGE_SPEED = 16;
        private const int FULL_BAR_FADEOUT_SPEED = 300;
        private double _displayedLevel = 1;
        private float _maxBarWidth;

        private bool LevelBarFull
        {
            get { return (int) (Parent.Players[PlayerID].Level) == Parent.Players[PlayerID].MaxArrowLevel(); }
        }
 
        private void InitSprites()
        {
            _maxBarWidth = this.Width - 52;
            _barPosition = new Vector2(this.X + 4, this.Y + 3);
            _baseSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("LevelBarBase"),
                Position=  this.Position,
                Size = this.Size
            };

            _maxBaseSprite = new Sprite3D
                                 {
                                     Texture = TextureManager.Textures("LevelBarMaxBase"),
                                     Position = this.Position,
                                     Size = new Vector2(this.Width , this.Height)
                                 };

            _barSprite = new SpriteMap3D
            {
                Columns = 1,
                Rows = 12,
                Texture = TextureManager.Textures("LevelBarFronts")
            };

            _maxFrontSprite = new Sprite3D
                                  {
                                      Texture = TextureManager.Textures("LevelBarMaxFront"),
                                      Position = _barPosition,
                                     Size = new Vector2(_maxBarWidth, this.Height - 6)

                                 };

            _textPosition = new Vector2(this.X + this.Width - 25, this.Y + 3);
          
            _spritesInit = true;
        }

        public override void Draw()
        {
            Draw( 0.0);
        }
        public void Draw( double phraseNumber)
        {
            if (!_spritesInit)
            {
                InitSprites();
            }
  
            _baseSprite.Size = this.Size;
            _baseSprite.Draw();
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

            
            _maxBaseSprite.Draw();
                FontManager.DrawString("" + (int)(Parent.Players[PlayerID].Level * Multiplier), "DefaultFont",
                       _textPosition, Color.Black,FontAlign.Center);

             DrawBars();
            _maxFrontSprite.DrawTiled((int)(maxFrontBeatFraction * _maxFrontSprite.Texture.Width), 0, _maxFrontSprite.Texture.Width, _maxFrontSprite.Texture.Height);
         
        }

        private void DrawBars()
        {
            _displayedLevel *= Multiplier;
            var diff = (Multiplier * Parent.Players[PlayerID].Level) - _displayedLevel;
            _displayedLevel += diff * Math.Min(TextureManager.LastDrawnPhraseDiff * FRONT_BAR_CHANGE_SPEED, 0.5);
          
            //The current progress towards the next level.
            double levelFraction = _displayedLevel - Math.Floor(_displayedLevel);
            //The calculated width of the level bar.
            var barWidth = (levelFraction * _maxBarWidth);

            if (LevelBarFull)
            {
                barWidth = _maxBarWidth;       
            }

            _lastLevelDrawn = Math.Min(_lastLevelDrawn, (int)_displayedLevel - 1);

            //Level maxed out, draw a full bar.
            if (Math.Floor(_displayedLevel) - 1 > _lastLevelDrawn)
            {
                _lastLevelDrawn = (int) ((Parent.Players[PlayerID].Level*Multiplier) - 1);
                _lastLevelOpacity = 255;
            }
            //Draw the last level bar (gradually fading out) if appropriate.
            if (_lastLevelDrawn > 0)
            {
                _barSprite.ColorShading.A = Convert.ToByte(_lastLevelOpacity);
                _barSprite.Draw( _lastLevelDrawn - 1, _maxBarWidth, this.Height - 6, _barPosition );
                diff = TextureManager.LastDrawnPhraseDiff*FULL_BAR_FADEOUT_SPEED;
                _lastLevelOpacity = Math.Max(_lastLevelOpacity - diff, 0);
            }

            //Draw the current level bar.
            _barSprite.ColorShading.A = LevelBarFull ? (byte) 255 : (byte) (40 + (215 * levelFraction));
            _barSprite.Draw(((int)(_displayedLevel - 1) % _barSprite.Rows), (float) barWidth, this.Height - 6, _barPosition);

            _displayedLevel /= Multiplier;
        }

   
    }
}
