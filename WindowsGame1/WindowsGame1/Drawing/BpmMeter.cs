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

        public GameSong DisplayedSong { get; set; }

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
            _bpmTextPosition.Y += 90;
        }

        private readonly Color _notLitColor = new Color(64, 64, 64, 255);
        private double _displayedLength;
        private Vector2 _bpmTextPosition;
        private double _displayedBpm;

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
            textPosition.X += 180;
            if (!String.IsNullOrEmpty(DisplayedSong.Title))
            {
                TextureManager.DrawString(spriteBatch, DisplayedSong.Title, "LargeFont",
                                          textPosition, Color.Black, FontAlign.CENTER);
            }
            textPosition.Y += 25;
            if (!String.IsNullOrEmpty(DisplayedSong.Subtitle))
            {
                TextureManager.DrawString(spriteBatch, DisplayedSong.Subtitle, "DefaultFont",
                                          textPosition, Color.Black, FontAlign.CENTER);
            }
            textPosition.Y += 30;
            if (!String.IsNullOrEmpty(DisplayedSong.Artist))
            {
                TextureManager.DrawString(spriteBatch, DisplayedSong.Artist, "DefaultFont",
                                          textPosition, Color.Black, FontAlign.CENTER);
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

            if (_displayedBpm == 0.0)
            {
                _displayedBpm = DisplayedSong.Bpm;
            }
            else
            {
                var diff = _displayedBpm - DisplayedSong.Bpm;
                _displayedBpm -= diff / 6;
            }

            var beatFraction = (SongTime) - Math.Floor(SongTime);
            beatFraction *= BEAT_FRACTION_SEVERITY;

            var meterBPM = Math.Max(BpmLevels[BpmLevels.Count() - 1], DisplayedSong.Bpm * (1 - beatFraction));

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

            TextureManager.DrawString(spriteBatch, String.Format("{0:000.0}", Math.Min(999.9,_displayedBpm)), "TwoTechLarge",
                        _bpmTextPosition, Color.Black, FontAlign.RIGHT);
        }

    }
}
