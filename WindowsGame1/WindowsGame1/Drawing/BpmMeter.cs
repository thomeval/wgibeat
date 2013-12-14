using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class BpmMeter : DrawableObject
    {
        private SpriteMap3D  _meterSprite;
        private Sprite3D _baseSprite;
        private Sprite3D _baseBlinkSprite;
        private Sprite3D _songLengthBase;
        private Sprite3D _songTitleBase;
        private Sprite3D _warningSprite;
        private double _displayedMinBpm;
        private double _displayedMaxBpm;
        private double _actualMinBpm;
        private double _actualMaxBpm;

        private const int BPM_LENGTH_ANIMATION_SPEED = 10;

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
                                     82,79,76,73,70
                                          };
        public BpmMeter()
        {
            InitSprites();
            SongTime = 1;
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
            _meterSprite = new SpriteMap3D 
            {
                Columns = 1,
                Rows = BpmLevels.Count(),
                Texture = TextureManager.Textures("BpmMeterOverlay")
            };
            _baseSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("BpmMeterBase")
            };
            _songLengthBase = new Sprite3D
            {
                Texture = TextureManager.Textures("LengthDisplayBase")
            };

            _songTitleBase = new Sprite3D { Texture = TextureManager.Textures("SongTitleBase") };

     
            _baseBlinkSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("BpmMeterBlink"),
            };

            _warningSprite = new Sprite3D
                                 {
                                     Texture = TextureManager.Textures("RestrictionIcon"),
                                     Size = new Vector2(64,64)
                                 };
            RepositionSprites();
        }

        private void RepositionSprites()
        {
            //TODO: Fix to use metrics
            _songTitleBase.Position = this.Position.Clone();
            
            _songLengthBase.Position = this.Position.Clone();
            _songLengthBase.X += 185;
            _songLengthBase.Y += 190;


            _baseSprite.Size = this.Size;
            _baseSprite.Position = this.Position.Clone();
            _baseSprite.X += 165;
            _baseSprite.Y += 55;
            _baseBlinkSprite.Position = _baseSprite.Position;
            _baseBlinkSprite.Size = this.Size;

            _bpmTextPosition = _baseSprite.Position.Clone();
            _bpmTextPosition.X += 125;
            _bpmTextPosition.Y += 85;
            _songTitleBase.Size = new Vector2(383,82);
            _warningSprite.Position = new Vector2(this.X + _songTitleBase.Width - _warningSprite.Width - 10, this.Y + this.Height  + 60 - _warningSprite.Height);
        }

        private readonly Color _notLitColor = new Color(64, 64, 64, 255);
        private double _displayedLength;
        private Vector2 _bpmTextPosition;

        public override void Draw()
        {
            DrawBase();
            DrawBPMMeter();
            DrawLengthDisplay();
            DrawTitleDisplay();
        }

        private void DrawTitleDisplay()
        {
            _songTitleBase.Draw();

            var textPosition = _songTitleBase.Position.Clone();
            Vector2 scale;
            textPosition.X += 185;
            if (!String.IsNullOrEmpty(DisplayedSong.Title))
            {
                scale = FontManager.ScaleTextToFit(DisplayedSong.Title, "LargeFont", 350, 100);
                FontManager.DrawString(DisplayedSong.Title, "LargeFont",
                                          textPosition, scale, Color.Black, FontAlign.Center);
            }
            textPosition.X += 5;
            textPosition.Y += 25;
            if (!String.IsNullOrEmpty(DisplayedSong.Subtitle))
            {
                scale = FontManager.ScaleTextToFit(DisplayedSong.Subtitle, "DefaultFont", 360, 100);
                FontManager.DrawString(DisplayedSong.Subtitle, "DefaultFont",
                                          textPosition, scale, Color.Black, FontAlign.Center);
            }
            textPosition.Y += 30;
            if (!String.IsNullOrEmpty(DisplayedSong.Artist))
            {
                scale = FontManager.ScaleTextToFit(DisplayedSong.Artist, "DefaultFont", 360, 100);
                FontManager.DrawString(DisplayedSong.Artist, "DefaultFont",
                                          textPosition, scale, Color.Black, FontAlign.Center);
            }
        }

        private void DrawLengthDisplay()
        {
            _songLengthBase.Draw();
            var diff = (DisplayedSong.Length - DisplayedSong.Offset) - _displayedLength;
            var changeMx = Math.Min(0.5, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * BPM_LENGTH_ANIMATION_SPEED);
            _displayedLength += (diff * (changeMx));
            var textPosition = _songLengthBase.Position.Clone();
            textPosition.X += 100;
            textPosition.Y -= 0;
            var ts = TimeSpan.FromSeconds(_displayedLength);

            FontManager.DrawString(String.Format("{0}:{1:00}", ts.Minutes, ts.Seconds), "TwoTech36", textPosition, Color.Black, FontAlign.Right);

        }

        private const double BEAT_FRACTION_SEVERITY = 0.35;
        private void DrawBPMMeter()
        {
            var changeMx = Math.Min(0.5, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * BPM_LENGTH_ANIMATION_SPEED);

            var diff = _displayedMinBpm - _actualMinBpm;
            _displayedMinBpm -= (diff * (changeMx));

            diff = _displayedMaxBpm - _actualMaxBpm;
            _displayedMaxBpm -= (diff * (changeMx));

            var beatFraction = (SongTime) - Math.Floor(SongTime);
            beatFraction *= BEAT_FRACTION_SEVERITY;

            var meterBPM = Math.Max(BpmLevels[BpmLevels.Count() - 1], DisplayedSong.StartBPM * (1 - beatFraction));

          
            float height = (this.Height - 2) / _meterSprite.Rows;
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
                _meterSprite.Draw(x, this.Width, height, _baseSprite.X, _baseSprite.Y + (x * height));
            }

            DrawBPMText();
            DrawWarningIcon();

        }

        private void DrawWarningIcon()
        {
            if (_actualMaxBpm >= 180)
            {
                var beatFraction = (SongTime) - Math.Floor(SongTime);
                _warningSprite.ColorShading.A = (byte)(255 * (1 - beatFraction));
                _warningSprite.Draw();
            }
        }

        private readonly double[] _bpmColors = {70, 140, 195, 210};
        private readonly Color[] _blinkColors = new[] { Color.Lime, Color.Yellow, Color.Red, new Color(234, 15, 158) };


        private void DrawBase()
        {
            var beatFraction = (SongTime) - Math.Floor(SongTime);

            _baseSprite.Draw();
            _baseBlinkSprite.ColorShading = GetBlinkColour();
            _baseBlinkSprite.ColorShading.A = (byte) (255 * (1-beatFraction));
            _baseBlinkSprite.Draw();
        }

        private Color GetBlinkColour()
        {
            var blinkBPM = _displayedSong.BPMs[0.0];
            if (blinkBPM < _bpmColors[0])
            {
                return _blinkColors[0];
            }
            if (blinkBPM > _bpmColors[_bpmColors.Length -1])
            {
                return _blinkColors[_bpmColors.Length - 1];
            }

            var idx = (from e in _bpmColors where blinkBPM >= e select e).Count();

            var part =  blinkBPM - _bpmColors[idx-1];
            part /=  (_bpmColors[idx] - _bpmColors[idx - 1]);
            part = Math.Max(0, part);
            return Color.Lerp(_blinkColors[idx-1], _blinkColors[idx], (float) part);
        }

        private void DrawBPMText()
        {
            var bpmLabelText = _bpmTextPosition.Clone();
            bpmLabelText.X -= 120;
            bpmLabelText.Y += 4;
            if (_actualMinBpm == _actualMaxBpm)
            {
                FontManager.DrawString(String.Format("{0:000.0}", Math.Min(999.9, _displayedMinBpm)),
                                          "TwoTechLarge",
                                          _bpmTextPosition, Color.Black, FontAlign.Right);
            }
            else
            {
                _bpmTextPosition.Y += 2;
                FontManager.DrawString(String.Format("{0:000.0}", Math.Min(999.9, _displayedMinBpm)),
                          "TwoTech24",
                          _bpmTextPosition, Color.Black, FontAlign.Right);
                _bpmTextPosition.Y += 19;
                FontManager.DrawString(String.Format("{0:000.0}", Math.Min(999.9, _displayedMaxBpm)),
                          "TwoTech24",
                          _bpmTextPosition, Color.Black, FontAlign.Right);
                _bpmTextPosition.Y -= 21;

                FontManager.DrawString("Min:",
                          "DefaultFont",
                          bpmLabelText, Color.Black, FontAlign.Left);
                bpmLabelText.Y += 20;
                FontManager.DrawString("Max:",
           "DefaultFont",
           bpmLabelText, Color.Black, FontAlign.Left);
            }
        }
    }
}
