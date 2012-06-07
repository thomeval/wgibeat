using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Players;

namespace WGiBeat.Drawing
{
    public class CoopLifeBar : LifeBar
    {

        private readonly double[] _displayedLife;

        private Sprite _basePart;
        private Sprite _sidePart;
        private Sprite _gridPart;
        private Sprite _middlePart;
        private SpriteMap _frontPart;
        private Sprite _overchargePart;
        private Sprite _blazingPart;
        private Sprite _blazingSidePart;
        private Sprite _blazingMiddlePart;
        

        private double _overchargeTextureOffset;
        private const double OVERCHARGE_OFFSET_CLIP = 250;

        public double BaseCapacity { get; set;}
        public double TrueCapacity { get; set; }
        public Vector2[] SidePositions { get; set; }
        public Vector2 SideSize { get; set; }
        public Vector2 MiddlePosition { get; set; }
        public Vector2 MiddleSize { get; set; }

        public CoopLifeBar()
        {

            _displayedLife = new double[4];
            SidePositions = new Vector2[4];
        
        }


        private void InitSprites()
        {
            _basePart = new Sprite
            {
                Height = this.Height,
                Width = this.Width,
                SpriteTexture = TextureManager.Textures("CoopLifeBarBase")
            };

            _gridPart = new Sprite
            {
                X = this.X + 2,
                Y = this.Y + 3,
                SpriteTexture = TextureManager.Textures("LifeBarGridBase")
            };

            _sidePart = new Sprite
                            {
                                SpriteTexture = TextureManager.Textures("LifeBarBaseCoop"),
                                Size = SideSize,                      
                            };
            
            _middlePart = new Sprite { SpriteTexture = TextureManager.Textures("CoopLifeBarMiddle"), Position = MiddlePosition, Size = MiddleSize };

            _frontPart = new SpriteMap { Columns = 1, Rows = 4, SpriteTexture = TextureManager.Textures("LifeBarFront") };

            _overchargePart = new Sprite
                                  {
                                      SpriteTexture = TextureManager.Textures("LifeBarOvercharge"),
     
                                  };

            _blazingPart = new Sprite { SpriteTexture = TextureManager.Textures("LifeBarBlazingCoop") };
            _blazingSidePart = new Sprite
            {
                SpriteTexture = TextureManager.CreateWhiteMask("LifeBarBaseCoop"),
                Size = SideSize
            };

            _blazingMiddlePart = new Sprite
                                     {
                                         SpriteTexture = TextureManager.Textures("CoopLifebarMiddleBlazing"),
                                         Position = _middlePart.Position,
                                         Size = _middlePart.Size
                                     };
        }

        const double BEAT_FRACTION_SEVERITY = 0.3;
        private const int BLOCK_WIDTH = 4;
        private const int OVERCHARGE_FLOW_SPEED = 80;
        private int _blocksCount;

        public override void Draw(SpriteBatch spriteBatch, double gameTime)
        {
            if (_basePart == null)
            {
                InitSprites();
            }
            DrawBase(spriteBatch);
            DrawSides(spriteBatch);
            DrawBlocks(spriteBatch, gameTime);
            DrawOvercharge(spriteBatch);        
            DrawGrid(spriteBatch);
            DrawFullEffect(spriteBatch);
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
                DrawText(spriteBatch,x,SidePositions[x]);
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
            _overchargeTextureOffset = (_overchargeTextureOffset + (OVERCHARGE_FLOW_SPEED * TextureManager.LastDrawnPhraseDiff)) % OVERCHARGE_OFFSET_CLIP;

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
            _gridPart.X = this.X + 2;
            _gridPart.Y = this.Y + 3;
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
            _blazingSidePart.ColorShading = Color.White;
            _blazingSidePart.ColorShading.A = opacity;
            
            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].IsBlazing)
                {
                    continue;
                }
                _blazingSidePart.Draw(spriteBatch);
            }

            _blazingMiddlePart.ColorShading.A = opacity;
            _blazingMiddlePart.Draw(spriteBatch);
        }


        private void DrawFullEffect(SpriteBatch spriteBatch)
        {
            if (TotalLife() < TrueCapacity)
            {
                return;
            }
            for (int x = 0; x < 4; x++)
            {

                   if (!Parent.Players[x].Playing)
                   {
                       continue;
                   }
                    _blazingSidePart.ColorShading = Parent.FullHighlightColors[x];
                    _blazingSidePart.ColorShading.A = 192;
                    _blazingSidePart.Position = SidePositions[x];
                    _blazingSidePart.Draw(spriteBatch);
                
            }
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0.0);
        }

        private void DrawBase(SpriteBatch spriteBatch)
        {
            _basePart.Position = this.Position;
            _basePart.Draw(spriteBatch);
        }

        private void DrawText(SpriteBatch spriteBatch, int player, Vector2 position)
        {
            var textPosition = position.Clone();
            textPosition.X += 25;

            TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", (int)Parent.Players[player].Life),
                    "DefaultFont",textPosition, Color.Black,FontAlign.CENTER);

            textPosition.X += 60;
            TextureManager.DrawString(spriteBatch, String.Format("{0:P0}",  Parent.Players[player].Life / TrueCapacity),
        "DefaultFont", textPosition, Color.Black, FontAlign.CENTER);
           
        }
  
        private void DrawSides(SpriteBatch spriteBatch)
        {

            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].Playing)
                {
                    continue;
                }

                _sidePart.Position = SidePositions[x];
                _sidePart.Draw(spriteBatch);

            }
            _middlePart.Draw(spriteBatch);

            var position = MiddlePosition.Clone();
            position.X += 25;
            TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", (int)TotalLife()), "LargeFont",
                    position, Color.Black, FontAlign.CENTER);

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
