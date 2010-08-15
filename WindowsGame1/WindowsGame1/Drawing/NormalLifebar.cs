using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class NormalLifeBar : LifeBar
    {
        private const double LIFEBAR_CAPACITY = 100;
        private double _displayedLife;

        private SpriteMap _frontPart;
        private SpriteMap _sidePart;
        private Sprite _basePart;
        private Sprite _gridPart;
        private Sprite _overchargePart;
        private double _overchargeTextureOffset;
        private Vector2 _sidePos;

        private const double OVERCHARGE_OFFSET_CLIP = 250;
        public int PlayerID { private get; set; }

        public NormalLifeBar()
        {
            _displayedLife = 0;
            InitSprites();
        }

        private void InitSprites()
        {

            _sidePos = new Vector2();

            switch (PlayerID)
            {
                case 1:
                    //Top Right
                    _sidePos.X = this.X + this.Width - 50;
                    _sidePos.Y = this.Y + this.Height;
                    break;
                case 2:
                    //Bottom Left
                    _sidePos.X = this.X;
                    _sidePos.Y = this.Y - 25;
                    break;
                case 3:
                    //Bottom Right
                    _sidePos.X = this.X + this.Width - 50;
                    _sidePos.Y = this.Y - 25;
                    break;
                default:
                    //Top Left
                    _sidePos.X = this.X;
                    _sidePos.Y = this.Y + this.Height;
                    break;

            }

            _basePart = new Sprite
            {
                Height = this.Height,
                Width = this.Width,
                X = this.X,
                Y = this.Y,
                SpriteTexture = TextureManager.Textures["lifeBarBase"]
            };

            _sidePart = new SpriteMap
                            {
                                SpriteTexture = TextureManager.Textures["lifeBarBaseSide"],
                                Columns=1,
                                Rows=2
                            };

            _frontPart = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures["lifeBarFront"],
                Columns = 1,
                Rows = 4
            };

            _overchargePart = new Sprite
            {
                Height = this.Height - 6,
                X = this.X + 3,
                Y = this.Y + 3,
                SpriteTexture = TextureManager.Textures["lifeBarOvercharge"]
            };

            _gridPart = new Sprite
                            {
                                Height = this.Height - 6,
                                Width = this.Width - 4,
                                X = this.X + 2,
                                Y = this.Y + 3,
                                SpriteTexture = TextureManager.Textures["lifeBarGridBase"]
                            };

        }

        const double BEAT_FRACTION_SEVERITY = 0.3;
        private const int FRONT_WIDTH = 4;
        private int _blocksCount;

        public override void Draw(SpriteBatch spriteBatch, double gameTime)
        {
            _blocksCount = (int) Math.Ceiling((this.Width - 6)/4.00);
            var solidLife = Math.Min(Parent.Players[PlayerID].Life, 100);

            //Causes the bar to pulse on every beat.
            gameTime *= 4;
            var beatFraction = (gameTime) -  Math.Floor(gameTime);
            beatFraction *= BEAT_FRACTION_SEVERITY;

            //Causes the bar to not pulse before the first beat.
            if (gameTime >= 0)
            {
                solidLife *= (1 - beatFraction);
            }

            _basePart.Draw(spriteBatch);
            _sidePart.Draw(spriteBatch,PlayerID/2,50,25,_sidePos);

            //Draw each block in sequence. Either in colour, or black depending on the Player's life.
            var highestBlock = GetHighestBlockLevel();
            for (int x = 0; x < _blocksCount;x++)
            {
                var minLife = LIFEBAR_CAPACITY/_blocksCount*x;
                if (solidLife > minLife)
                {
                    _frontPart.ColorShading = Color.White;
                }
                else if (x == highestBlock)
                {
                    _frontPart.ColorShading = Color.LightGray;
                }
                else
                {
                    _frontPart.ColorShading = Color.Black;
                }
                _frontPart.Draw(spriteBatch, PlayerID, FRONT_WIDTH, this.Height - 6, this.X + 3 + (FRONT_WIDTH * x), this.Y + 3);
            }

            _displayedLife = Parent.Players[PlayerID].Life;

            //Draw the overcharge above the normal bar.
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
            DrawText(spriteBatch, _sidePos);
            _overchargeTextureOffset = (_overchargeTextureOffset + 0.5) % OVERCHARGE_OFFSET_CLIP;
        }

        public int GetHighestBlockLevel()
        {
            for (int x = _blocksCount-1; x >= 0; x--)
            {
                var minLife = LIFEBAR_CAPACITY / _blocksCount * x;

                if (Parent.Players[PlayerID].Life > minLife)
                {
                    return x;
                }
            }
            return 0;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0.0);
        }

        private void DrawText(SpriteBatch spriteBatch,Vector2 position)
        {
            position.X += 25;
            TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", (int)Parent.Players[PlayerID].Life),
                    "DefaultFont",position, Color.Black,FontAlign.CENTER);
            position.X -= 25;
        }


        public override void Reset()
        {
            _displayedLife = 0;
            InitSprites();
        }


    }
}
