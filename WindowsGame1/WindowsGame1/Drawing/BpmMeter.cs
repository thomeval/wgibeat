using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class BpmMeter :DrawableObject 
    {
        private SpriteMap _meterSprite;
        private Sprite _baseSprite;
        public double Bpm { get; set; }
        public double SongTime { get; set; }

        private const int DEFAULT_HEIGHT = 138;
        private const int DEFAULT_WIDTH = 240;
        public int[] BpmLevels = {
                                     230,220,210,200,
                                     190,185,180,175,170,
                                     165,160,155,150,145,
                                     140,135,130,125,120,
                                     116,112,108,104,100,
                                     97,94,91,88,85,
                                     82,79,76,73,70,
                                 };
        public BpmMeter()
        {
            _meterSprite = new SpriteMap()
                               {
                                   Columns = 1,
                                   Rows = BpmLevels.Count(),
                                   SpriteTexture = TextureManager.Textures["BpmMeterOverlay"]
                               };
            _baseSprite = new Sprite()
                              {

                                  SpriteTexture = TextureManager.Textures["BpmMeterBase"]
                              };
            this.Width = DEFAULT_WIDTH;
            this.Height = DEFAULT_HEIGHT;
            
            
        }

        private readonly Color _notLitColor = new Color(64, 64, 64, 255);

        private const double BEAT_FRACTION_SEVERITY = 0.2;
        public override void Draw(SpriteBatch spriteBatch)
        {

            var beatFraction = (SongTime) - Math.Floor(SongTime);
            beatFraction *= BEAT_FRACTION_SEVERITY;

            var displayBpm = Math.Max(BpmLevels[BpmLevels.Count()-1], Bpm*(1 - beatFraction));

            SetDimensions();
            _baseSprite.Draw(spriteBatch);
            int height = (this.Height-2)/_meterSprite.Rows;
            for (int x = 0; x < BpmLevels.Count(); x++)
            {
                if (displayBpm >= BpmLevels[x])
                {
                    _meterSprite.ColorShading = Color.White;
                }
                else
                {
                    _meterSprite.ColorShading = _notLitColor;
                }
                _meterSprite.Draw(spriteBatch, x, this.Width, height, this.X, this.Y + (x*height));
            }
        }

        private void SetDimensions()
        {
            _baseSprite.Width = this.Width;
            _baseSprite.Height = this.Height;
            _baseSprite.X = this.X;
            _baseSprite.Y = this.Y;
        }
    }
}
