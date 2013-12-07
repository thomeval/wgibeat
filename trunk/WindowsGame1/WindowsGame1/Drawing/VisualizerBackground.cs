using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class VisualizerBackground : DrawableObject
    {
        public AudioManager AudioManager { get; set; }

        public byte MaxBrightness { get; set; }
        public int SongChannel { get; set; }

        public double Opacity { get; set; }
        private double _displayOpacity;

        public Color _colour = Color.Black;

        public Color Colour 
        { 
            get
            {
                return _colour;
            }
            set
            {
                _colour = value;
                if (!_init )
                {
                    return;
                }
                _waveformDrawer.ColorShading = _colour;
                _spectrumDrawer.ColorShading = _colour;
                _spectrumDrawerTop.ColorShading = _colour;
                _mySprite.ColorShading = Color.Black;
            }
        }
           
        private Sprite3D _mySprite;

        private SpectrumDrawer _spectrumDrawer;
        private WaveformDrawer _waveformDrawer;
 

        private bool _init;
        private SpectrumDrawer _spectrumDrawerTop;



        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch,0.0);
        }

        private const int WAVEFORM_POINTS = 512;
        private const int SPECTRUM_POINTS = 64;
        private const int BAR_WIDTH = 15;
        private const int BAR_HEIGHT = 300;
        private const int OPACITY_CHANGE_SPEED = 1;
        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            if (MaxBrightness == 0)
            {
                return;
            }
            if (!_init)
            {
                Initialize();
                _init = true;
            }

            var diff = Opacity - _displayOpacity;
       
            var changeMx = Math.Min(0.5, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * OPACITY_CHANGE_SPEED);
            _displayOpacity += (diff * (changeMx));

            var myOpacity = 255 * Math.Min(MaxBrightness, _displayOpacity) / 255 * (GetBeatFraction(phraseNumber));

   
                float[] levels = AudioManager.GetChannelSpectrum(SongChannel, SPECTRUM_POINTS);
            float[] waveLevels = AudioManager.GetChannelWaveform(SongChannel, WAVEFORM_POINTS);

                _mySprite.ColorShading.A = (byte)(myOpacity);
           _mySprite.Draw();


     

            _spectrumDrawer.ColorShading.A = _spectrumDrawerTop.ColorShading.A = _waveformDrawer.ColorShading.A = (byte) Math.Min(MaxBrightness, _displayOpacity);
            _spectrumDrawerTop.Height = 50 + (int)(250 * _displayOpacity / 255);
            _spectrumDrawer.Height = -_spectrumDrawerTop.Height;
            _waveformDrawer.Height = 25 + (int)(125 * _displayOpacity / 255);
            DrawLevels(spriteBatch, levels);

            DrawWaveform(waveLevels);
        }

        private void DrawWaveform(float[] waveLevels)
        {
            _waveformDrawer.Draw(waveLevels);
        }

        private void Initialize()
        {
            _mySprite = new Sprite3D
            {
                Texture = TextureManager.BlankTexture(),
                Height = this.Height,
                Width = this.Width,
                ColorShading = Colour,
            };
            _spectrumDrawer = new SpectrumDrawer
                                  {
                                      Position = new Vector2(0, 600),
                                      Size = new Vector2(BAR_WIDTH, -BAR_HEIGHT),
                                      ColorShading = Colour,
                                      LevelsCount = WAVEFORM_POINTS
                                  };
            _spectrumDrawerTop = new SpectrumDrawer
            {
                Position = new Vector2(0, 0),
                Size = new Vector2(BAR_WIDTH, BAR_HEIGHT),
                ColorShading =  Colour,
                LevelsCount = WAVEFORM_POINTS
            };

            _waveformDrawer = new WaveformDrawer
                                  {
                                      ColorShading = Colour,
                                      Size = new Vector2(800, 150),
                                      Position = new Vector2(0, 300),
                                   
                                  };
            _waveformDrawer.Init();
            _spectrumDrawer.Init();
        }

        private void DrawLevels(SpriteBatch spriteBatch, float[] levels)
        {
            _spectrumDrawer.Draw( levels);
            _spectrumDrawerTop.Draw(levels);
        }


        private double GetBeatFraction(double phraseNumber)
        {
            return 1 - (phraseNumber - Math.Floor(phraseNumber));
        }
    }
    }

