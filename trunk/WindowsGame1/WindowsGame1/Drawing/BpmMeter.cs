using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class BpmMeter : DrawableObject
    {
        private SpriteMap _meterSprite;
        private Sprite _baseSprite;
        private Sprite _songLengthBase;
        private Sprite _songTitleBase;
        private double _displayedMinBpm;
        private double _displayedMaxBpm;
        private double _actualMinBpm;
        private double _actualMaxBpm;

        private GameSong _displayedSong;
        public GameSong DisplayedSong
        {
            get { return _displayedSong; }
            set
            {
                _displayedSong = value;
                _actualMinBpm = (from e in _displayedSong.BPMs.Values select e).Min();
                _actualMaxBpm = (from e in _displayedSong.BPMs.Values select e).Max();
            }
        }

        public double SongTime { get; set; }

        private const int DEFAULT_HEIGHT = 138;
        private const int DEFAULT_WIDTH = 240;
        public readonly int[] BpmLevels = {
                                     210,205,200,195,
                                     190,185,180,175,170,
                                     165,160,155,150,145,
                                     140,135,130,125,120,
                                     116,112,108,104,100,
                                     97,94,91,88,85,
                                     82,79,76,73,70,
                                 };
        public BpmMeter()
        {
            InitSprites();

            this.Width = DEFAULT_WIDTH;
            this.Height = DEFAULT_HEIGHT;


        }

        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                RepositionSprites();
            }
        }

        private void InitSprites()
        {
            _meterSprite = new SpriteMap
            {
                Columns = 1,
                Rows = BpmLevels.Count(),
                SpriteTexture = TextureManager.Textures("BpmMeterOverlay")
            };
            _baseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("BpmMeterBase")
            };
            _songLengthBase = new Sprite
            {
                SpriteTexture = TextureManager.Textures("LengthDisplayBase")
            };

            _songTitleBase = new Sprite { SpriteTexture = TextureManager.Textures("SongTitleBase") };

            RepositionSprites();
        }

        private void RepositionSprites()
        {

            _songTitleBase.Position = this.Position.Clone();

            _songLengthBase.Position = this.Position.Clone();
            _songLengthBase.X += 185;
            _songLengthBase.Y += 190;


            _baseSprite.Width = this.Width;
            _baseSprite.Height = this.Height;
            _baseSprite.Position = this.Position.Clone();
            _baseSprite.X += 165;
            _baseSprite.Y += 55;

            _bpmTextPosition = _baseSprite.Position.Clone();
            _bpmTextPosition.X += 125;
            _bpmTextPosition.Y += 85;
        }

        private readonly Color _notLitColor = new Color(64, 64, 64, 255);
        private double _displayedLength;
        private Vector2 _bpmTextPosition;

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBPMMeter(spriteBatch);
            DrawLengthDisplay(spriteBatch);
            DrawTitleDisplay(spriteBatch);
        }

        private void DrawTitleDisplay(SpriteBatch spriteBatch)
        {
            _songTitleBase.Draw(spriteBatch);

            var textPosition = _songTitleBase.Position.Clone();
            Vector2 scale;
            textPosition.X += 185;
            if (!String.IsNullOrEmpty(DisplayedSong.Title))
            {
                scale = TextureManager.ScaleTextToFit(DisplayedSong.Title, "LargeFont", 350, 100);
                TextureManager.DrawString(spriteBatch, DisplayedSong.Title, "LargeFont",
                                          textPosition, scale, Color.Black, FontAlign.CENTER);
            }
            textPosition.X += 5;
            textPosition.Y += 25;
            if (!String.IsNullOrEmpty(DisplayedSong.Subtitle))
            {
                scale = TextureManager.ScaleTextToFit(DisplayedSong.Subtitle, "DefaultFont", 360, 100);
                TextureManager.DrawString(spriteBatch, DisplayedSong.Subtitle, "DefaultFont",
                                          textPosition, scale, Color.Black, FontAlign.CENTER);
            }
            textPosition.Y += 30;
            if (!String.IsNullOrEmpty(DisplayedSong.Artist))
            {
                scale = TextureManager.ScaleTextToFit(DisplayedSong.Artist, "DefaultFont", 360, 100);
                TextureManager.DrawString(spriteBatch, DisplayedSong.Artist, "DefaultFont",
                                          textPosition, scale, Color.Black, FontAlign.CENTER);
            }
        }

        private void DrawLengthDisplay(SpriteBatch spriteBatch)
        {
            _songLengthBase.Draw(spriteBatch);
            var diff = DisplayedSong.Length - _displayedLength;

            _displayedLength += (diff / 6);
            var textPosition = _songLengthBase.Position.Clone();
            textPosition.X += 100;
            textPosition.Y -= 0;
            var ts = TimeSpan.FromSeconds(_displayedLength);

            TextureManager.DrawString(spriteBatch, String.Format("{0}:{1:00}", ts.Minutes, ts.Seconds), "TwoTech36", textPosition, Color.Black, FontAlign.RIGHT);

        }

        private const double BEAT_FRACTION_SEVERITY = 0.35;
        private void DrawBPMMeter(SpriteBatch spriteBatch)
        {

            var diff = _displayedMinBpm - _actualMinBpm;
            _displayedMinBpm -= diff / 6;

            diff = _displayedMaxBpm - _actualMaxBpm;
            _displayedMaxBpm -= diff / 6;

            var beatFraction = (SongTime) - Math.Floor(SongTime);
            beatFraction *= BEAT_FRACTION_SEVERITY;

            var meterBPM = Math.Max(BpmLevels[BpmLevels.Count() - 1], DisplayedSong.StartBPM * (1 - beatFraction));

            _baseSprite.Draw(spriteBatch);
            int height = (this.Height - 2) / _meterSprite.Rows;
            for (int x = 0; x < BpmLevels.Count(); x++)
            {
                if ((meterBPM >= BpmLevels[x]))
                {
                    _meterSprite.ColorShading = Color.White;
                }
                else
                {
                    _meterSprite.ColorShading = _notLitColor;
                }
                _meterSprite.Draw(spriteBatch, x, this.Width, height, _baseSprite.X, _baseSprite.Y + (x * height));
            }

            DrawBPMText(spriteBatch);

        }

        private void DrawBPMText(SpriteBatch spriteBatch)
        {
            var bpmLabelText = _bpmTextPosition.Clone();
            bpmLabelText.X -= 120;
            bpmLabelText.Y += 4;
            if (_actualMinBpm == _actualMaxBpm)
            {
                TextureManager.DrawString(spriteBatch, String.Format("{0:000.0}", Math.Min(999.9, _displayedMinBpm)),
                                          "TwoTechLarge",
                                          _bpmTextPosition, Color.Black, FontAlign.RIGHT);
            }
            else
            {
                _bpmTextPosition.Y += 2;
                TextureManager.DrawString(spriteBatch, String.Format("{0:000.0}", Math.Min(999.9, _displayedMinBpm)),
                          "TwoTech24",
                          _bpmTextPosition, Color.Black, FontAlign.RIGHT);
                _bpmTextPosition.Y += 19;
                TextureManager.DrawString(spriteBatch, String.Format("{0:000.0}", Math.Min(999.9, _displayedMaxBpm)),
                          "TwoTech24",
                          _bpmTextPosition, Color.Black, FontAlign.RIGHT);
                _bpmTextPosition.Y -= 21;

                TextureManager.DrawString(spriteBatch, "Min:",
                          "DefaultFont",
                          bpmLabelText, Color.Black, FontAlign.LEFT);
                bpmLabelText.Y += 20;
                TextureManager.DrawString(spriteBatch, "Max:",
           "DefaultFont",
           bpmLabelText, Color.Black, FontAlign.LEFT);
            }
        }
    }
}
