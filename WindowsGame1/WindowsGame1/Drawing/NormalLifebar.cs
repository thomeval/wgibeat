using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Players;

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
        private Sprite _2ndOverchargePart;
        private Sprite _blazingPart;
        private SpriteMap _blazingSidePart;
        private double _overchargeTextureOffset;
        private Vector2 _sidePos;

        private const double OVERCHARGE_OFFSET_CLIP = 250;
        public int PlayerID { private get; set; }

        public NormalLifeBar()
        {
            _displayedLife = 0;
            InitSprites();

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
                UpdatePositions();
            }
        }

        private void UpdatePositions()
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

            _basePart.Height = this.Height;
            _basePart.Width = this.Width;
            _basePart.Position = this.Position;
            _overchargePart.Height = this.Height - 18;
            _overchargePart.Position = new Vector2(this.X + 3, this.Y + 15);
            _2ndOverchargePart.Height = this.Height - 6;
            _2ndOverchargePart.Position = new Vector2(this.X + 3, this.Y + 3);
            _gridPart.Height = this.Height - 6;
            _gridPart.Width = this.Width - 3;
            _gridPart.Position = new Vector2(this.X + 2, this.Y + 3);

        }

        private void InitSprites()
        {

            _basePart = new Sprite
            {
                SpriteTexture = TextureManager.Textures("LifeBarBase")
            };

            _sidePart = new SpriteMap
                            {
                                SpriteTexture = TextureManager.Textures("LifeBarBaseSide"),
                                Columns = 1,
                                Rows = 2
                            };

            _frontPart = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures("LifeBarFront"),
                Columns = 1,
                Rows = 4
            };

            _overchargePart = new Sprite
            {

                SpriteTexture = TextureManager.Textures("LifeBarOvercharge")
            };

            _2ndOverchargePart = new Sprite
            {
                SpriteTexture = TextureManager.Textures("LifeBarOvercharge")
            };
            _gridPart = new Sprite
                            {

                                SpriteTexture = TextureManager.Textures("LifeBarGridBase")
                            };
            _blazingPart = new Sprite { SpriteTexture = TextureManager.Textures("LifeBarBlazing") };
            _blazingSidePart = new SpriteMap
                                   {
                                       Columns = 1,
                                       Rows = 2,
                                       SpriteTexture = TextureManager.CreateWhiteMask("LifeBarBaseSide")
                                   };

        }

        const double BEAT_FRACTION_SEVERITY = 0.3;
        private const int FRONT_WIDTH = 4;
        private int _blocksCount;
        private const int OVERCHARGE_FLOW_SPEED = 80;

        public override void Draw(SpriteBatch spriteBatch, double gameTime)
        {
            _blocksCount = (int)Math.Ceiling((this.Width - 6) / (double) FRONT_WIDTH);
            var solidLife = Math.Min(Parent.Players[PlayerID].Life, 100);

            //Causes the bar to pulse on every beat.
            gameTime *= 4;
            var beatFraction = (gameTime) - Math.Floor(gameTime);
            beatFraction *= BEAT_FRACTION_SEVERITY;

            //Causes the bar to not pulse before the first beat.
            if (gameTime >= 0)
            {
                solidLife *= (1 - beatFraction);
            }

            _basePart.Draw(spriteBatch);
            _sidePart.Draw(spriteBatch, PlayerID / 2, 50, 25, _sidePos);

            //Draw each block in sequence. Either in colour, or black depending on the Player's life.
            var highestBlock = GetHighestBlockLevel();
            for (int x = 0; x < _blocksCount; x++)
            {
                var minLife = LIFEBAR_CAPACITY / _blocksCount * x;
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
            DrawFirstOvercharge(spriteBatch);
            _gridPart.DrawTiled(spriteBatch, 0, 0, this.Width - 3, this.Height - 6);
            DrawBlazingEffect(spriteBatch, beatFraction);
            DrawFullEffect(spriteBatch);
            DrawSecondOvercharge(spriteBatch);

            DrawText(spriteBatch, _sidePos);
            
            _overchargeTextureOffset = (_overchargeTextureOffset + (OVERCHARGE_FLOW_SPEED*TextureManager.LastDrawnPhraseDiff)) % OVERCHARGE_OFFSET_CLIP;
        }

        private void DrawFullEffect(SpriteBatch spriteBatch)
        {
            if ((Parent.Players[PlayerID].Life == Parent.Players[PlayerID].GetMaxLife()) && (!Parent.Players[PlayerID].IsBlazing))
            {
                _blazingSidePart.ColorShading.A = 255;
                _blazingSidePart.ColorShading = Parent.FullHighlightColors[PlayerID];
                _blazingSidePart.Draw(spriteBatch, PlayerID / 2, 50, 25, _sidePos);
            }
        }

        private void DrawBlazingEffect(SpriteBatch spriteBatch, double beatFraction)
        {
            if (!Parent.Players[PlayerID].IsBlazing)
            {
                return;
            }
            beatFraction /= BEAT_FRACTION_SEVERITY;
            var opacity = (byte)((1 - beatFraction) * 255);
            _blazingPart.ColorShading.A = opacity;
            _blazingPart.X = this.X;
            _blazingPart.Y = this.Y;
            _blazingPart.Draw(spriteBatch);
            _blazingSidePart.ColorShading = Color.White;
            _blazingSidePart.ColorShading.A = opacity;
            _blazingSidePart.Draw(spriteBatch, PlayerID / 2, 50, 25, _sidePos);
        }

        private void DrawFirstOvercharge(SpriteBatch spriteBatch)
        {
            if (_displayedLife <= 100)
            {
                return;
            }
            var amount = Math.Min(_displayedLife - 100, 100);
            var opacity = (_displayedLife - 100) * 2.55;
            opacity = Math.Max(opacity, 80);
            opacity = Math.Min(255, opacity);

            _overchargePart.Width = (int)((this.Width - 4) / LIFEBAR_CAPACITY * amount);
            _overchargePart.ColorShading.A = Convert.ToByte(opacity);

            if (_overchargePart.Width > 0)
            {
                _overchargePart.DrawTiled(spriteBatch, (int)_overchargeTextureOffset, 0, _overchargePart.Width,
                                          _overchargePart.Height);
            }

        }
        private void DrawSecondOvercharge(SpriteBatch spriteBatch)
        {
            if (_displayedLife <= 200)
            {
                return;
            }


            var amount = Math.Min(_displayedLife - 200, 100);
            var opacity = (_displayedLife - 200) * 2.55;
            opacity = Math.Max(opacity, 80);
            opacity = Math.Min(255, opacity);

            _2ndOverchargePart.Width = (int)((this.Width - 4) / LIFEBAR_CAPACITY * amount);
            _2ndOverchargePart.ColorShading.A = Convert.ToByte(opacity);

            _2ndOverchargePart.DrawTiled(spriteBatch, (int)_overchargeTextureOffset, 0, _2ndOverchargePart.Width,
                                          _2ndOverchargePart.Height);

        }

        public int GetHighestBlockLevel()
        {
            for (int x = _blocksCount - 1; x >= 0; x--)
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

        private void DrawText(SpriteBatch spriteBatch, Vector2 position)
        {
            position.X += 25;
            TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", (int)Parent.Players[PlayerID].Life),
                    "DefaultFont", position, Color.Black, FontAlign.CENTER);
            position.X -= 25;
        }


        public override void Reset()
        {
            _displayedLife = 0;
        }


    }
}
