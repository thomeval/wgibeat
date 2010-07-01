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
        private Sprite _gridPart;
        private SpriteMap _adjustPart;
        private Sprite _overchargePart;
        private double _overchargeTextureOffset;


        private const double OVERCHARGE_OFFSET_CLIP = 250;
        public int PlayerID { private get; set; }

        public NormalLifebar()
        {
            _displayedLife = 0;
            InitSprites();
        }

        private void InitSprites()
        {

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
                    sidePosY = this.Y - 25;
                    break;
                case 3:
                    //Bottom Right
                    sidePosX = this.X + this.Width - 50;
                    sidePosY = this.Y - 25;
                    break;
                default:
                    //Top Left
                    sidePosX = this.X;
                    sidePosY = this.Y + this.Height;
                    break;

            }

            _basePart = new Sprite
            {
                Height = this.Height,
                Width = this.Width,
                X = this.X,
                Y = this.Y,
                SpriteTexture = TextureManager.Textures["lifebarBase"]
            };

            _sidePart = new Sprite
                            {
                                X = sidePosX,
                                Y = sidePosY,
                                SpriteTexture = TextureManager.Textures["lifebarBaseSide"]
                            };

            _frontPart = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures["lifebarFront"],
                Columns = 1,
                Rows = 4
            };

            _adjustPart = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures["lifebarFront"],
                Columns = 1,
                Rows = 4
            };

            _overchargePart = new Sprite
            {
                Height = this.Height - 6,
                X = this.X + 3,
                Y = this.Y + 3,
                SpriteTexture = TextureManager.Textures["lifebarOvercharge"]
            };

            _gridPart = new Sprite
                            {
                                Height = this.Height - 6,
                                Width = this.Width - 4,
                                X = this.X + 2,
                                Y = this.Y + 3,
                                SpriteTexture = TextureManager.Textures["lifebarGridBase"]
                            };

            if (PlayerID > 1)
            {
                _sidePart.SpriteTexture = TextureManager.Textures["lifebarBaseSideUp"];
            }
        }

        const double BEAT_FRACTION_SEVERITY = 0.3;

        public override void Draw(SpriteBatch spriteBatch, double gameTime)
        {
            var solidLife = Math.Min(Parent.Players[PlayerID].Life, 100);
            gameTime *= 4;
            var beatFraction = (gameTime) -  Math.Floor(gameTime);
            beatFraction *= BEAT_FRACTION_SEVERITY;

            if (gameTime >= 0)
            {
                solidLife *= (1 - beatFraction);
            }

            int frontWidth = (int)((this.Width - 6) / LIFEBAR_CAPACITY * solidLife);
            _basePart.Draw(spriteBatch);
            _sidePart.Draw(spriteBatch);

            _frontPart.Draw(spriteBatch, PlayerID, frontWidth, this.Height - 6, this.X + 3, this.Y + 3);

            _displayedLife = Parent.Players[PlayerID].Life;
            DrawAdjustments(spriteBatch);
            if (_displayedLife > 100)
            {

                _overchargePart.Width = (int)((this.Width - 5) / LIFEBAR_CAPACITY * (_displayedLife - 100));
                _overchargePart.ColorShading.A = Convert.ToByte((_displayedLife - 100) * 2.55);
                _overchargePart.ColorShading.A = Math.Max(_overchargePart.ColorShading.A, (byte) 80);
                if (_overchargePart.Width > 0)
                {
                    _overchargePart.DrawTiled(spriteBatch, (int)_overchargeTextureOffset, 0, _overchargePart.Width,
                                              _overchargePart.Height);
                }
            }

            _gridPart.Draw(spriteBatch);
            DrawText(spriteBatch, _sidePart.X + 5, _sidePart.Y);
            _overchargeTextureOffset = (_overchargeTextureOffset + 0.5) % OVERCHARGE_OFFSET_CLIP;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0.0);
        }

        private void DrawText(SpriteBatch spriteBatch, int x, int y)
        {
            var position = new Vector2(x, y);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("{0:D3}", (int)Parent.Players[PlayerID].Life),
                    position, Color.Black);
        }

        private void DrawAdjustments(SpriteBatch spriteBatch)
        {

                var myWidth = (int)((this.Width - 6) / LIFEBAR_CAPACITY * (Parent.Players[PlayerID].Life));
            myWidth = Math.Min(this.Width - 6, myWidth);
                var startPoint = this.X + 3;

                _adjustPart.ColorShading = Color.White;
                _adjustPart.ColorShading.A = 128;
                _adjustPart.Draw(spriteBatch, PlayerID, myWidth, this.Height - 6, startPoint, this.Y + 3);

        }

        public override void Reset()
        {
            _displayedLife = 0;
            InitSprites();
        }


    }
}
