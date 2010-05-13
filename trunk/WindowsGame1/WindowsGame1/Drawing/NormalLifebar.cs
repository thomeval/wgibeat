using System;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1.Drawing
{
    public class NormalLifebar : Lifebar
    {
        private const double LIFEBAR_CAPACITY = 100;
        private double _life;
        private double _displayedLife;
        
        private Sprite _frontPart;
        private Sprite _sidePart;
        private Sprite _basePart;
        private Sprite _negativeAdjustPart;
        private Sprite _positiveAdjustPart;
        private Sprite _overchargePart;
        private double _overchargeTextureOffset;
        public override event EventHandler LifebarFull;
        public override event EventHandler LifebarEmpty;

        private const double OVERCHARGE_OFFSET_CLIP = 250;
        public int SideLocation { private get; set; }
        public NormalLifebar()
        {
            _life = 0;
            _displayedLife = 0;
        }
        public override void Draw(SpriteBatch sb)
        {
            if (_basePart == null)
            {
                _basePart = new Sprite
              {
                  Height = this.Height,
                  Width = this.Width,
                  X = this.X,
                  Y = this.Y,
                  SpriteTexture = TextureManager.Textures["lifebarBase"]
              };
            }

            int sidePosX, sidePosY;

            switch (SideLocation)
            {
                case 1:
                    //Top Right
                    sidePosX = this.X + this.Width - 50;
                    sidePosY = this.Y + this.Height;
                    break;
                case 2:
                    //Bottom Left
                    sidePosX = this.X;
                    sidePosY = this.Y  -25;
                    break; 
                case 3:
                    //Bottom Right
                    sidePosX = this.X + this.Width - 50;
                    sidePosY = this.Y -25;
                    break;
                    default:
                    //Top Left
                    sidePosX = this.X;
                    sidePosY = this.Y + this.Height;
                    break;
                
            }
            if (_sidePart == null)
            {
                _sidePart = new Sprite
              {
                  Height = 25,
                  Width = 50,
                  X = sidePosX,
                  Y = sidePosY,
                  SpriteTexture = TextureManager.Textures["lifebarBaseSide"]
              };
                if (SideLocation > 1)
                {
                    _sidePart.SpriteTexture = TextureManager.Textures["lifebarBaseSideUp"];
                }
            }


            var solidLife = Math.Min(_displayedLife, _life);
            solidLife = Math.Min(solidLife, 100);
            if (_frontPart == null)
            {
                _frontPart = new Sprite
                                 {
                                     Height = this.Height - 6,
                                     X = this.X + 3,
                                     Y = this.Y + 3,
                                     SpriteTexture = TextureManager.Textures["lifebarP"+(SideLocation + 1) + "front"]
                                 };
            }


            _frontPart.Width = (int) ((this.Width - 6)/LIFEBAR_CAPACITY*solidLife);
            _basePart.Draw(sb);
            _sidePart.Draw(sb);
            _frontPart.Draw(sb);

            if (_displayedLife > 100)
            {
                if (_overchargePart == null)
                {
                    _overchargePart = new Sprite
                {
                    Height = this.Height - 6,
                    X = this.X + 3,
                    Y = this.Y + 3,
                    SpriteTexture = TextureManager.Textures["lifebarOvercharge"]
                };
 
                }
                _overchargePart.Width = (int)((this.Width - 5) / LIFEBAR_CAPACITY * (_displayedLife - 100));
                _overchargePart.DrawTiled(sb,(int) _overchargeTextureOffset, 0, _overchargePart.Width, _overchargePart.Height );
            }
            DrawAdjustments(sb);
            _overchargeTextureOffset = (_overchargeTextureOffset + 0.5) % OVERCHARGE_OFFSET_CLIP;

        }

        private void DrawAdjustments(SpriteBatch sb)
        {

            if (_displayedLife < _life)
            {
                _displayedLife += Math.Min(0.1, _life - _displayedLife);

                if ((_displayedLife > 100) && (_life > 100))
                {
                    return;
                }
                var actualPoint = (int)((this.Width - 6) / LIFEBAR_CAPACITY * _life);
                var myWidth = (int)((this.Width - 6) / LIFEBAR_CAPACITY * (_life - _displayedLife));
                var startPoint = this.X + 2 + actualPoint - myWidth;
                if (_positiveAdjustPart == null)
                {
                    _positiveAdjustPart = new Sprite
                                              {
                                                  Height = this.Height - 6,
                                                  Y = this.Y + 3,
                                                  SpriteTexture = TextureManager.Textures["lifebarP"+(SideLocation + 1)+"front"]

                                              };

                }
                _positiveAdjustPart.Width = myWidth;
                _positiveAdjustPart.X = startPoint;
                _positiveAdjustPart.ColorShading.A = 128;
                _positiveAdjustPart.Draw(sb);

            }
            if (_displayedLife > _life)
            {
                _displayedLife -= Math.Min(0.1, _displayedLife - _life);
                if ((_displayedLife > 100) && (_life > 100))
                {
                    return;
                }
                var actualPoint = (int) ((this.Width - 6)/LIFEBAR_CAPACITY*_life);
                var myWidth = (int) ((this.Width - 6)/LIFEBAR_CAPACITY*(_displayedLife - _life));
                var startPoint = this.X + 3 + actualPoint;
                if (_negativeAdjustPart == null)
                {
                    _negativeAdjustPart = new Sprite
                                              {
                                                  Height = this.Height - 6,
                                                  Y = this.Y + 3,
                                                  SpriteTexture = TextureManager.Textures["lifebarP" + (SideLocation + 1) + "front"],
                                                  ColorShading = Color.Gray
                                              };
                }

                _negativeAdjustPart.Width = myWidth;
                _negativeAdjustPart.X = startPoint;
                _negativeAdjustPart.Draw(sb);

            }
           
        }

        public override void SetLife(double amount)
        {
            _life = amount;

            if ((_life <= 0.0) && (LifebarEmpty != null))
            {
                LifebarEmpty(this, null);
            }
            if ((_life >= 200.0) && (LifebarFull != null))
            {
                LifebarFull(this, null);
            }
        }
    }
}
