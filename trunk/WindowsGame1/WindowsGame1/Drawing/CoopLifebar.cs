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
                SpriteTexture = TextureManager.Textures["coopLifebarBase"]
            };

            _gridPart = new Sprite
            {
                X = this.X + 2,
                Y = this.Y + 3,
                SpriteTexture = TextureManager.Textures["lifeBarGridBase"]
            };

            _sidePart = new SpriteMap()
                            {
                                SpriteTexture = TextureManager.Textures["lifeBarBaseSide"],
                                Rows = 2,
                                Columns = 1
                            };

            _middlePart = new SpriteMap { Columns = 1, Rows = 2, SpriteTexture = TextureManager.Textures["coopLifebarMiddle"] };

            _frontPart = new SpriteMap { Columns = 1, Rows = 4, SpriteTexture = TextureManager.Textures["lifeBarFront"] };

        }

        const double BEAT_FRACTION_SEVERITY = 0.3;
        private const int FRONT_WIDTH = 4;
        private int _blocksCount;
        public override void Draw(SpriteBatch spriteBatch, double gameTime)
        {
            DrawBase(spriteBatch);
            DrawSides(spriteBatch);

            _blocksCount = (int)Math.Ceiling((this.Width - 6.00) / FRONT_WIDTH);
            var beatFraction = GetBeatFraction(gameTime);
            double penaltyMx = Math.Max(0, TotalLife() / TotalPositive());

            
            int posX = this.X + 3;
            double capacity = 100.0 * Participants();
            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].Playing)
                {
                    continue;
                }
  
                var displayedLife = Parent.Players[x].Life;
                displayedLife *= (1 - beatFraction) * penaltyMx;
                //Draw each block in sequence. Either in colour, or black depending on the Player's life.
                var highestBlock = GetHighestBlockLevel(x);

                for (int y = 0; y <= highestBlock; y++)
                {
                    if (posX >= this.Width)
                    {
                        break;
                    }
                    var minLife = capacity/_blocksCount*y;
                    if (displayedLife > minLife)
                    {
                        _frontPart.ColorShading = Color.White;
                    }
                    else if (y == highestBlock)
                    {
                        _frontPart.ColorShading = Color.DarkGray;
                    }
                    else
                    {
                        _frontPart.ColorShading = Color.Black;
                    }
                    _frontPart.Draw(spriteBatch, x, FRONT_WIDTH, this.Height - 6, posX + (FRONT_WIDTH*y), this.Y + 3);
                    
                }
                posX += FRONT_WIDTH * (highestBlock+1);

            }
            _frontPart.ColorShading = Color.Black;
            while (posX < this.Width)
            {
                _frontPart.Draw(spriteBatch,0,FRONT_WIDTH, this.Height - 6, posX,this.Y + 3);
                posX += FRONT_WIDTH;
            }
            _gridPart.SetPosition(this.X+2,this.Y+3);
            _gridPart.Width = this.Width - 4;
            _gridPart.Height = this.Height - 4;
            _gridPart.DrawTiled(spriteBatch,0,0,this.Width-4,this.Height-4);
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

        private void DrawText(SpriteBatch spriteBatch, int player, int x, int y)
        {
            var position = new Vector2(x + 25, y);
            TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", (int)Parent.Players[player].Life),
                    "DefaultFont",position, Color.Black,FontAlign.CENTER);
        }

        private void DrawTotal(SpriteBatch spriteBatch, int x, int y)
        {
            var position = new Vector2(x, y);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("{0:D3}", (int)TotalLife()),
                    position, Color.Black);
        }
        private void DrawSides(SpriteBatch spriteBatch)
        {
            int playerIdx = 0;
            int posX, posY;

            //Lifebar amounts appear on top for Player 3 and 4.
            if (SideLocationTop)
            {
                playerIdx += 2;
                posY = this.Y - 25;
            }
            else
            {
                posY = this.Y + this.Height;
            }

            //Draw on the right side.
            if (Parent.Players[playerIdx + 1].Playing)
            {
                posX = this.X + this.Width - 50;
                _sidePart.Draw(spriteBatch, playerIdx / 2, 50, 25, posX, posY);
                DrawText(spriteBatch, playerIdx + 1, posX, posY);
            }
            //Draw on the left.
            if (Parent.Players[playerIdx].Playing)
            {
                posX = this.X;
                _sidePart.Draw(spriteBatch, playerIdx / 2, 50, 25, posX, posY);
                DrawText(spriteBatch, playerIdx, posX, posY);
            }

            _middlePart.Draw(spriteBatch, playerIdx / 2, 139, 25, (this.X + this.Width - 134) / 2, posY);
            DrawTotal(spriteBatch, (this.X + this.Width - 40) / 2, posY);

        }

        #region Helper Methods
        public int Participants()
        {
            return (from e in Parent.Players where e.Playing select e).Count();
        }

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

        public int GetHighestBlockLevel(int player)
        {
            double penaltyMx = Math.Max(0, TotalLife() / TotalPositive());
            
            for (int x = _blocksCount - 1; x >= 0; x--)
            {
                var minLife = 100.0 * Participants() / _blocksCount * x;

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
