using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class BpmMeter :DrawableObject 
    {
        private SpriteMap _meterSprite;

        public double Bpm { get; set; }

        public int[] BpmLevels = {
                                     60, 62, 64, 66, 68, 70, 72, 74, 76, 78,
                                     80, 83, 86, 89, 92, 95, 98, 101, 104, 107,
                                     110, 114, 118, 122, 126, 130, 134, 138, 142, 146,
                                     150, 155, 160, 165, 170, 175, 180, 185, 190, 195,
                                     200
                                 };
        public BpmMeter()
        {
            _meterSprite = new SpriteMap()
                               {
                                   Columns = 50,
                                   Rows = 1,
                                   SpriteTexture = TextureManager.Textures["BpmMeter"]
                               };
            
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            int width = this.Width/50;
            for (int x = 0; x < BpmLevels.Count(); x++)
            {
                if (Bpm >= BpmLevels[x])
                {
                    _meterSprite.ColorShading = Color.White;
                }
                else
                {
                    _meterSprite.ColorShading = Color.Gray;
                }
                _meterSprite.Draw(spriteBatch, x, width, this.Height, this.X + (x * width), this.Y);
            }
        }
    }
}
