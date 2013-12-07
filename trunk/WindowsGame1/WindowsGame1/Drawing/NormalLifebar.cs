using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class NormalLifeBar : LifeBar
    {
        private const double LIFEBAR_CAPACITY = 100;
        private double _displayedLife;

        private SpriteMap3D _frontPart;
        private Sprite3D _basePart;
        private Sprite3D _gridPart;
        private Sprite3D _overchargePart;
        private Sprite3D _2ndOverchargePart;
        private Sprite3D _blazingPart;
        private double _overchargeTextureOffset;
        private Vector2 _textPosition;

        private double _overchargeOffsetClip;
        private const int BAR_X_OFFSET = 45;
        public int PlayerID { private get; set; }

        private readonly Color[] _fullColors =
            {
                new Color(255, 128, 128),
                new Color(128, 128, 255),
                new Color(128, 255, 128),
                new Color(255, 255, 128)
            };
        public NormalLifeBar()
        {
            _displayedLife = 0;
            

        }

        private void InitSprites()
        {

            _basePart = new Sprite3D
            {
                Texture = TextureManager.Textures("LifeBarBase"),
                Position = this.Position,
                Size = this.Size
            };
            _frontPart = new SpriteMap3D
            {
                Texture = TextureManager.Textures("LifeBarFront"),
                Columns = 1,
                Rows = 4
            };

            _overchargePart = new Sprite3D
            {

                Texture = TextureManager.Textures("LifeBarOvercharge"),
                Position = new Vector2(this.X + BAR_X_OFFSET, this.Y + 3),
                Height = this.Height - 6,
            };

            _2ndOverchargePart = new Sprite3D
            {
                Texture = TextureManager.Textures("LifeBarOvercharge2"),
                Position = new Vector2(this.X + BAR_X_OFFSET, this.Y + 3),
                Height = this.Height - 6
            };
            _gridPart = new Sprite3D
            {
                Texture = TextureManager.Textures("LifeBarGridBase"),
                Position = new Vector2(this.X + BAR_X_OFFSET, this.Y + 3),
                Size = new Vector2(this.Width - BAR_X_OFFSET, this.Height - 6),
            };
            _blazingPart = new Sprite3D
            {
                Texture = TextureManager.Textures("LifeBarBlazing"),
                Position = this.Position,
                Size = this.Size
                
            };
            _overchargeOffsetClip = _overchargePart.Texture.Width;
            _textPosition = this.Position.Clone();
            _textPosition.X += 25;

        }

        const double BEAT_FRACTION_SEVERITY = 0.3;
        private const int FRONT_WIDTH = 4;
        private int _blocksCount;
        private const int OVERCHARGE_FLOW_SPEED = 250;

        public override void Draw(SpriteBatch spriteBatch, double gameTime)
        {
            if (_basePart == null)
            {
                InitSprites();
            }
            
            _blocksCount = (int)Math.Ceiling((this.Width - BAR_X_OFFSET - 8) / (double)FRONT_WIDTH);
            var solidLife = Math.Min(_displayedLife, 100);

            //Causes the bar to pulse on every beat.
            gameTime *= 4;
            var beatFraction = (gameTime) - Math.Floor(gameTime);
            beatFraction *= BEAT_FRACTION_SEVERITY;

            //Causes the bar to not pulse before the first beat.
            if (gameTime >= 0)
            {
                solidLife *= (1 - beatFraction);
            }

            Debug.Assert(_basePart != null);
            _basePart.Draw();

            //Draw each block in sequence. Either in colour, or black depending on the Player's life.
            var highestBlock = GetHighestBlockLevel();
            var startPoint = this.X + BAR_X_OFFSET;
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
                _frontPart.Draw( PlayerID, FRONT_WIDTH, this.Height - 6, startPoint + (FRONT_WIDTH * x), this.Y + 3);
            }

            UpdateDisplayedLife();

           
            _gridPart.DrawTiled(0, 0, this.Width - BAR_X_OFFSET, this.Height - 6);

            //Draw the overcharge above the normal bar.
            DrawFirstOvercharge();
            DrawSecondOvercharge();
         
            DrawBlazingEffect(beatFraction);
            DrawFullEffect();   

            DrawText(spriteBatch);
            UpdateOverchargeTexture();

        }

        private void UpdateOverchargeTexture()
        {
            var amount = + (OVERCHARGE_FLOW_SPEED*TextureManager.LastDrawnPhraseDiff);
            if (Parent.Players[PlayerID].IsBlazing)
            {
                amount *= 4;
            }
            
            _overchargeTextureOffset = (_overchargeTextureOffset + amount ) % _overchargeOffsetClip;
        }

        private void DrawFullEffect()
        {
            if (Parent.LifebarFull(PlayerID) && (!Parent.Players[PlayerID].IsBlazing))
            {
                _blazingPart.ColorShading = _fullColors[PlayerID];
                _blazingPart.ColorShading.A = 128;
                _blazingPart.Draw();
               
            }
        }



        private void DrawBlazingEffect(double beatFraction)
        {
            if (!Parent.Players[PlayerID].IsBlazing)
            {
                return;
            }
            beatFraction /= BEAT_FRACTION_SEVERITY;
            var opacity = (byte)((1 - beatFraction) * 255);
            _blazingPart.ColorShading = Color.White;
            _blazingPart.ColorShading.A = opacity;
            _blazingPart.Draw();
        }

        private void DrawFirstOvercharge()
        {
            if (_displayedLife <= 100)
            {
                return;
            }
            var amount = Math.Min(_displayedLife - 100, 100);
            _overchargePart.Width = (int)((this.Width - BAR_X_OFFSET - 4) / LIFEBAR_CAPACITY * amount);

            if (_overchargePart.Width > 0)
            {
                _overchargePart.DrawTiled((int)_overchargeTextureOffset, 0, (int) amount * 10,
                                          _overchargePart.Texture.Height);
            }

        }
        private void DrawSecondOvercharge()
        {
            if (_displayedLife <= 200)
            {
                return;
            }

            var amount = Math.Min(_displayedLife - 200, 100);
            _2ndOverchargePart.Width = (int)((this.Width - BAR_X_OFFSET - 4) / LIFEBAR_CAPACITY * amount);

            _2ndOverchargePart.DrawTiled((int)_overchargeTextureOffset, 0, (int) amount * 10,
                                          _2ndOverchargePart.Texture.Height);

        }

        public int GetHighestBlockLevel()
        {
            for (int x = _blocksCount - 1; x >= 0; x--)
            {
                var minLife = LIFEBAR_CAPACITY / _blocksCount * x;

                if (_displayedLife > minLife)
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

        private void DrawText(SpriteBatch spriteBatch)
        {

            TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", (int)_displayedLife),
                    "DefaultFont", _textPosition, Color.Black, FontAlign.Center);

        }


        public override void Reset()
        {
            _displayedLife = Parent.Players[PlayerID].Life;
        }

        private const double LIFE_CHANGE_SPEED = 8;
        public void UpdateDisplayedLife()
        {

            var diff = Parent.Players[PlayerID].Life - _displayedLife;
            if (Math.Abs(diff) < 0.01)
            {
                _displayedLife = Parent.Players[PlayerID].Life;
            }
            else
            {
                var changeMx = Math.Min(1, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * LIFE_CHANGE_SPEED);
                _displayedLife += diff * (changeMx);
            }

        }
    }
}
