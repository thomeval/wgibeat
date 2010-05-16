using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class NormalLifebar : Lifebar
    {
        private const double LIFEBAR_CAPACITY = 100;
        private double _displayedLife;
        
        private SpriteMap _frontPart;
        private Sprite _sidePart;
        private Sprite _basePart;
        private SpriteMap _adjustPart;
        private Sprite _overchargePart;
        private double _overchargeTextureOffset;


        private const double OVERCHARGE_OFFSET_CLIP = 250;
        public int PlayerID { private get; set; }
        public NormalLifebar()
        {
            _displayedLife = 0;
        }
        public override void Draw(SpriteBatch spriteBatch)
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

            switch (PlayerID)
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
                if (PlayerID > 1)
                {
                    _sidePart.SpriteTexture = TextureManager.Textures["lifebarBaseSideUp"];
                }
            }


            var solidLife = Math.Min(_displayedLife, Parent.Players[PlayerID].Life);
            solidLife = Math.Min(solidLife, 100);

            
            if (_frontPart == null)
            {
                _frontPart = new SpriteMap
                                 {

                                     SpriteTexture = TextureManager.Textures["lifebarFront"]
                                 };
            }


            int frontWidth = (int) ((this.Width - 6)/LIFEBAR_CAPACITY*solidLife);
            _basePart.Draw(spriteBatch);
            _sidePart.Draw(spriteBatch);
            _frontPart.Draw(spriteBatch,PlayerID,frontWidth, this.Height - 6, this.X + 3, this.Y + 3);

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
                if (_overchargePart.Width > 0)
                {
                    _overchargePart.DrawTiled(spriteBatch, (int) _overchargeTextureOffset, 0, _overchargePart.Width,
                                              _overchargePart.Height);
                }
            }
            DrawAdjustments(spriteBatch);
            DrawText(spriteBatch, sidePosX + 5, sidePosY + 5);
            _overchargeTextureOffset = (_overchargeTextureOffset + 0.5) % OVERCHARGE_OFFSET_CLIP;

        }

        private void DrawText(SpriteBatch spriteBatch, int x, int y)
        {
            var position = new Vector2(x,y);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("{0:D3}", (int) Parent.Players[PlayerID].Life),
                    position, Color.Black);
        }

        private void DrawAdjustments(SpriteBatch spriteBatch)
        {

            if (_adjustPart == null)
            {
                _adjustPart = new SpriteMap {SpriteTexture = TextureManager.Textures["lifebarFront"]};
            }
            int adjustWidth, adjustX;

            var actualPoint = (int)((this.Width - 6) / LIFEBAR_CAPACITY * Parent.Players[PlayerID].Life);

            //Displaying less than actual life.
            if (_displayedLife < Parent.Players[PlayerID].Life)
            {
                _displayedLife += Math.Min(0.1, Parent.Players[PlayerID].Life - _displayedLife);

                if ((_displayedLife > 100) && (Parent.Players[PlayerID].Life > 100))
                {
                    return;
                }

                var myWidth = (int)((this.Width - 6) / LIFEBAR_CAPACITY * (Parent.Players[PlayerID].Life - _displayedLife));
                var startPoint = this.X + 2 + actualPoint - myWidth;

                adjustWidth= myWidth;
                adjustX = startPoint;
                _adjustPart.ColorShading = Color.White;
                _adjustPart.ColorShading.A = 128;
                _adjustPart.Draw(spriteBatch, PlayerID, adjustWidth, this.Height - 6, adjustX, this.Y + 3);

            }
            
            //Displaying more than actual life (shrinking).
            if (_displayedLife > Parent.Players[PlayerID].Life)
            {
                _displayedLife -= Math.Min(0.1, _displayedLife - Parent.Players[PlayerID].Life);
                if ((_displayedLife > 100) && (Parent.Players[PlayerID].Life > 100))
                {
                    return;
                }
                
                var myWidth = (int)((this.Width - 6) / LIFEBAR_CAPACITY * (_displayedLife - Parent.Players[PlayerID].Life));
                var startPoint = this.X + 3 + actualPoint;

                adjustWidth = myWidth;
                adjustX = startPoint;
                _adjustPart.ColorShading = Color.Gray;
                _adjustPart.Draw(spriteBatch, PlayerID, adjustWidth, this.Height - 6, adjustX, this.Y + 3);

            }
           
        }

        public override void Reset()
        {
            _displayedLife = 0;
        }
    }
}
