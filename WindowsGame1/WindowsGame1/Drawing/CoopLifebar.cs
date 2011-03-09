using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class CoopLifeBar : LifeBar
    {

        public bool SideLocationTop;
        private readonly double[] _displayedLife;

        private Sprite _basePart;
        private SpriteMap _sidePart;
        private Sprite _gridPart;
        private SpriteMap _middlePart;
        private SpriteMap _frontPart;
        private Sprite _overchargePart;
        private Sprite _blazingPart;
        private SpriteMap _blazingSidePart;

        private double _overchargeTextureOffset;
        private const double OVERCHARGE_OFFSET_CLIP = 250;

        public double BaseCapacity { get; set;}
        public double TrueCapacity { get; set; }

        public CoopLifeBar()
        {
            _displayedLife = new double[4];
            InitSprites();
        }


        private void InitSprites()
        {
            _basePart = new Sprite
            {
                Height = this.Height,
                Width = this.Width,
                SpriteTexture = TextureManager.Textures("CoopLifebarBase")
            };

            _gridPart = new Sprite
            {
                X = this.X + 2,
                Y = this.Y + 3,
                SpriteTexture = TextureManager.Textures("LifeBarGridBase")
            };

            _sidePart = new SpriteMap
                            {
                                SpriteTexture = TextureManager.Textures("LifeBarBaseSide"),
                                Rows = 2,
                                Columns = 1
                            };

            _middlePart = new SpriteMap { Columns = 1, Rows = 2, SpriteTexture = TextureManager.Textures("CoopLifebarMiddle") };

            _frontPart = new SpriteMap { Columns = 1, Rows = 4, SpriteTexture = TextureManager.Textures("LifeBarFront") };

            _overchargePart = new Sprite
                                  {
                                      SpriteTexture = TextureManager.Textures("LifeBarOvercharge"),
     
                                  };

            _blazingPart = new Sprite { SpriteTexture = TextureManager.Textures("LifeBarBlazingCoop") };
            _blazingSidePart = new SpriteMap
            {
                Columns = 1,
                Rows = 2,
                SpriteTexture = TextureManager.CreateWhiteMask("LifeBarBaseSide")
            };
        }

        const double BEAT_FRACTION_SEVERITY = 0.3;
        private const int BLOCK_WIDTH = 4;
        private int _blocksCount;

        public override void Draw(SpriteBatch spriteBatch, double gameTime)
        {
            DrawBase(spriteBatch);
            DrawSides(spriteBatch);
            DrawBlocks(spriteBatch, gameTime);
            DrawOvercharge(spriteBatch);        
            DrawGrid(spriteBatch);
            DrawBlazingEffect(spriteBatch, gameTime);
            DrawText(spriteBatch);
        }

        private void DrawText(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].Playing)
                {
                    continue;
                }

                DrawText(spriteBatch,x,_sidePositions[x]);
            }
        }

        private void DrawOvercharge(SpriteBatch spriteBatch)
        {
            var maxOvercharge = TrueCapacity - BaseCapacity;
            var currentOvercharge = TotalLife() - BaseCapacity;
            if ((maxOvercharge <= 0) || (currentOvercharge <= 0))
            {
                return;
            }

            _overchargePart.Width = (int) (this.Width*currentOvercharge/maxOvercharge) - 5;
            _overchargePart.Width /= 2;
            _overchargePart.Height = this.Height - 18;
            _overchargePart.X = this.X + 2;
            _overchargePart.Y = this.Y + 15;

            _overchargePart.DrawTiled(spriteBatch,(int) (this.X + _overchargeTextureOffset),0,_overchargePart.Width,_overchargePart.Height);
            _overchargePart.X += this.Width - 5 - _overchargePart.Width;
            _overchargePart.DrawTiled(spriteBatch, (int) (this.X + _overchargeTextureOffset) + _overchargePart.Width, 0, _overchargePart.Width, _overchargePart.Height);
            _overchargeTextureOffset = (_overchargeTextureOffset + 0.5) % OVERCHARGE_OFFSET_CLIP;
        }

        private void DrawBlocks(SpriteBatch spriteBatch, double gameTime)
        {
            var beatFraction = GetBeatFraction(gameTime);

            int[] blockAssignments = AssignBlocks(beatFraction);

            int posX = this.X + 3;

            for (int y = 0; y < blockAssignments.Length; y++)
            {

                if ((blockAssignments[y] < 10) && (blockAssignments[y] > -1))
                {
                    _frontPart.ColorShading = Color.White;
                }
                else if (blockAssignments[y] >= 10)
                {
                    _frontPart.ColorShading = Color.DarkGray;
                    blockAssignments[y] -= 10;
                }
                else
                {
                    _frontPart.ColorShading = Color.Black;
                }
                _frontPart.Draw(spriteBatch, blockAssignments[y], BLOCK_WIDTH, this.Height - 6, posX + (BLOCK_WIDTH * y), this.Y + 3);
            }
        }

        private int[] AssignBlocks(double beatFraction)
        {
            _blocksCount = (int)Math.Ceiling((this.Width - 6.00) / BLOCK_WIDTH);
            var result = new int[_blocksCount];
            var capacity = Math.Max(TotalLife(), BaseCapacity);
            double penaltyMx = Math.Max(0, TotalLife() / TotalPositive());

            var position = 0;

            for (int x = 0; x < result.Length; x++)
            {
                result[x] = -1;
            }
            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].Playing)
                {
                    continue;
                }

                var displayedLife = Parent.Players[x].Life;
                displayedLife *= (1 - beatFraction) * penaltyMx;
                //Draw each block in sequence. Either in colour, or black depending on the Player's life.
                var highestBlock = GetHighestBlockLevel(x, capacity);

                for (int y = 0; y <= highestBlock; y++)
                {
                    if (position + y >= result.Length)
                    {
                        result[position + y - 1] = 10 + x;
                        break;
                    }

                    var minLife = capacity / _blocksCount * y;
                    if (displayedLife > minLife)
                    {
                        result[position + y] = x;
                    }
                    else if (y == highestBlock)
                    {
                        result[position + y] = 10 + x;
                    }

                }
                position += highestBlock + 1;

            }

            return result;

        }

        private void DrawGrid(SpriteBatch spriteBatch)
        {
            _gridPart.SetPosition(this.X + 2, this.Y + 3);
            _gridPart.Width = this.Width - 4;
            _gridPart.Height = this.Height - 4;
            _gridPart.DrawTiled(spriteBatch, 0, 0, this.Width - 4, this.Height - 4);
        }

        private void DrawBlazingEffect(SpriteBatch spriteBatch, double gameTime)
        {
            var anyBlazer = (from e in Parent.Players where e.IsBlazing select e).Any();
            if (!anyBlazer)
            {
                return;
            }

            gameTime *= 4;
            var beatFraction = gameTime - Math.Floor(gameTime);
            beatFraction = 1 - beatFraction;
            var opacity = (byte)(beatFraction * 255);
            _blazingPart.ColorShading.A = opacity;
            _blazingPart.X = this.X;
            _blazingPart.Y = this.Y;
            _blazingPart.Draw(spriteBatch);
            _blazingSidePart.ColorShading.A = opacity;

            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].IsBlazing)
                {
                    continue;
                }
                _blazingSidePart.Draw(spriteBatch, x/2, 50, 25, _sidePositions[x]);
            }

        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0.0);
        }

        private void DrawBase(SpriteBatch spriteBatch)
        {
            _basePart.SetPosition(this.X, this.Y);
            _basePart.Draw(spriteBatch);
        }

        private void DrawText(SpriteBatch spriteBatch, int player, Vector2 position)
        {
            var textPosition = position.Clone();
            textPosition.X += 25;

            TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", (int)Parent.Players[player].Life),
                    "DefaultFont",textPosition, Color.Black,FontAlign.CENTER);
           
        }

        private int GetBonusMultiplier()
        {
            var blazers = (from e in Parent.Players where e.IsBlazing select e).Count();
            switch (blazers)
            {
                case 4:
                    return 8;
                case 3:
                    return 4;
                case 2:
                    return 2;
                  default:
                    return 1;
            }

        }

        private void DrawTotal(SpriteBatch spriteBatch, int x, int y)
        {
            var position = new Vector2(x, y);
            TextureManager.DrawString(spriteBatch, GetBonusMultiplier() + "x",
"DefaultFont", position, Color.Black, FontAlign.CENTER);
            position.X += 40;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), String.Format("{0:D3}", (int)TotalLife()),
                    position, Color.Black);

        }

        private Vector2[] _sidePositions = new Vector2[4];
        private void DrawSides(SpriteBatch spriteBatch)
        {
            int playerIdx = 0;

 
            _sidePositions[0].X = this.X;
            _sidePositions[0].Y = this.Y + this.Height;
            _sidePositions[1].X = this.X + this.Width - 50;
            _sidePositions[1].Y = this.Y + this.Height;
            _sidePositions[2].X = this.X;
            _sidePositions[2].Y = this.Y -25;
            _sidePositions[3].X = this.X + this.Width - 50;
            _sidePositions[3].Y = this.Y -25;

            //Lifebar amounts appear on top for Player 3 and 4.
            if (SideLocationTop)
            {
                playerIdx += 2;

            }

            //Draw on the right side.
            if (Parent.Players[playerIdx + 1].Playing)
            {
                _sidePart.Draw(spriteBatch, playerIdx / 2, 50, 25,_sidePositions[playerIdx+1]);
            }
            //Draw on the left.
            if (Parent.Players[playerIdx].Playing)
            {
                _sidePart.Draw(spriteBatch, playerIdx / 2, 50, 25, _sidePositions[playerIdx]);
            }

            _middlePart.Draw(spriteBatch, playerIdx / 2, 139, 25, (this.X + this.Width - 134) / 2, (int) _sidePositions[playerIdx].Y);
            DrawTotal(spriteBatch, (this.X + this.Width - 90) / 2, (int)_sidePositions[playerIdx].Y);

        }


        #region Helper Methods

        public double TotalLife()
        {
            return (from e in Parent.Players where e.Playing select e.Life).Sum();
        }

        private double TotalPositive()
        {
            return (from e in Parent.Players where (e.Playing && e.Life >= 0) select e.Life).Sum();
        }
        public override void Reset()
        {
            for (int x = 0; x < 4; x++)
            {
                _displayedLife[x] = 0;
            }
        }

        public int GetHighestBlockLevel(int player, double capacity)
        {
            double penaltyMx = Math.Max(0, TotalLife() / TotalPositive());
            
            for (int x = _blocksCount - 1; x >= 0; x--)
            {
                var minLife = capacity / _blocksCount * x;

                if (Parent.Players[player].Life * penaltyMx > minLife)
                {
                    return x;
                }
            }
            return -1;
        }

        private double GetBeatFraction(double gameTime)
        {
            //Causes the bar to pulse on every beat.
            gameTime *= 4;
            return (gameTime - Math.Floor(gameTime)) * BEAT_FRACTION_SEVERITY;
        }

        #endregion
    }
}
