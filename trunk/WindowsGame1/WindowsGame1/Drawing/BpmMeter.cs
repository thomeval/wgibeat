using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class BpmMeter :DrawableObject 
    {
        private SpriteMap _meterSprite;
        private Sprite _baseSprite;
        public double Bpm { get; set; }

        public int[] BpmLevels = {
                                     200,180,165,150,135,
                                     120,115,110,105,100,
                                     95,90,85,80
                                 };
        public BpmMeter()
        {
            _meterSprite = new SpriteMap()
                               {
                                   Columns = 1,
                                   Rows = 14,
                                   SpriteTexture = TextureManager.Textures["BpmMeterOverlay"]
                               };
            _baseSprite = new Sprite()
                              {

                                  SpriteTexture = TextureManager.Textures["BpmMeterBase"]
                              };
            
        }

        private readonly Color _notLitColor = new Color(64, 64, 64, 255);

        public override void Draw(SpriteBatch spriteBatch)
        {
            _baseSprite.Width = this.Width;
            _baseSprite.Height = this.Height;
            _baseSprite.X = this.X;
            _baseSprite.Y = this.Y;
            _baseSprite.Draw(spriteBatch);
            int height = this.Height/_meterSprite.Rows;
            for (int x = 0; x < BpmLevels.Count(); x++)
            {
                if (Bpm >= BpmLevels[x])
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
    }
}
