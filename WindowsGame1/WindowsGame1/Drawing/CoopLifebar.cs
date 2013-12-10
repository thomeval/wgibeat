using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class CoopLifeBar : LifeBar
    {
        private readonly double[] _displayedLife;

        private Sprite3D _basePart;
        private Sprite3D _sidePart;
        private Sprite3D _gridPart;
        private Sprite3D _middlePart;
        private SpriteMap3D _frontPart;
        private Sprite3D _overchargePart;
        private Sprite3D _blazingPart;
        private Sprite3D _blazingSidePart;
        private Sprite3D _blazingMiddlePart;


        private double _overchargeTextureOffset;
        private double _overchargeOffsetClip = 250;

        public double BaseCapacity { get; set; }
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
            _basePart = new Sprite3D
            {
                Height = this.Height,
                Width = this.Width,
                Texture = TextureManager.Textures("CoopLifeBarBase")
            };

            _gridPart = new Sprite3D
            {
                X = this.X + 2,
                Y = this.Y + 3,
                Texture = TextureManager.Textures("LifeBarGridBase")
            };

            _sidePart = new Sprite3D
                            {
                                Texture = TextureManager.Textures("LifeBarBaseCoop"),
                                Size = SideSize,
                            };

            _middlePart = new Sprite3D { Texture = TextureManager.Textures("CoopLifeBarMiddle"), Position = MiddlePosition, Size = MiddleSize };

            _frontPart = new SpriteMap3D { Columns = 1, Rows = 4, Texture = TextureManager.Textures("LifeBarFront") };

            _overchargePart = new Sprite3D
                                  {
                                      Texture = TextureManager.Textures("LifeBarOvercharge2"),
                                      Height = this.Height - 6,
                                      Y = this.Y + 3
                                  };

            _blazingPart = new Sprite3D { Texture = TextureManager.Textures("LifeBarBlazingCoop") };
            _blazingSidePart = new Sprite3D
            {
                Texture = TextureManager.CreateWhiteMask("LifeBarBaseCoop"),
                Size = SideSize
            };

            _blazingMiddlePart = new Sprite3D
                                     {
                                         Texture = TextureManager.Textures("CoopLifebarMiddleBlazing"),
                                         Position = _middlePart.Position,
                                         Size = _middlePart.Size
                                     };
            _overchargeOffsetClip = _overchargePart.Texture.Width;
        }

        const double BEAT_FRACTION_SEVERITY = 0.3;
        private const int BLOCK_WIDTH = 4;
        private const int OVERCHARGE_FLOW_SPEED = 320;
        private int _blocksCount;

        public override void Draw( double gameTime)
        {
            if (_basePart == null)
            {
                InitSprites();
            }

            UpdateDisplayedLife();
            DrawBase();
            DrawSides();
            DrawBlocks( gameTime);
            DrawGrid();
            DrawOvercharge(); 
            DrawFullEffect();
            DrawBlazingEffect(gameTime);
            DrawText();
        }

        private void DrawText()
        {
            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].Playing)
                {
                    continue;
                }
                DrawText(x, SidePositions[x]);
            }
        }

        private void DrawOvercharge()
        {
            var maxOvercharge = TrueCapacity - BaseCapacity;
            var currentOvercharge = TotalLife() - BaseCapacity;
            if ((maxOvercharge <= 0) || (currentOvercharge <= 0))
            {
                return;
            }

            _overchargePart.Width = (int)(this.Width * currentOvercharge / maxOvercharge) - 5;
            _overchargePart.Width /= 2;

            _overchargePart.X = this.X + 2;

            _overchargePart.DrawTiled((int)(this.X + _overchargeTextureOffset), 0, _overchargePart.Width * 5, _overchargePart.Texture.Height);
            _overchargePart.X += this.Width - 5 - _overchargePart.Width;
            _overchargePart.DrawTiled((int)(this.X + _overchargeTextureOffset) + _overchargePart.Width, 0, _overchargePart.Width * 5, _overchargePart.Texture.Height);

            UpdateOverchargeTextureOffset();

        }

        private void UpdateOverchargeTextureOffset()
        {
            var delta = (OVERCHARGE_FLOW_SPEED*TextureManager.LastDrawnPhraseDiff);

            delta *= (1 + (Parent.Players.Count(e => e.IsBlazing && e.Playing)));
            _overchargeTextureOffset = (_overchargeTextureOffset + delta )%
                                       _overchargeOffsetClip;
        }

        private void DrawBlocks( double gameTime)
        {
            var beatFraction = GetBeatFraction(gameTime);

            int[] blockAssignments = AssignBlocks(beatFraction);

            float posX = this.X + 3;

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
                _frontPart.Draw(blockAssignments[y], BLOCK_WIDTH, this.Height - 6, posX + (BLOCK_WIDTH * y), this.Y + 3);
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

                var displayedLife = _displayedLife[x];
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

        private void DrawGrid()
        {
            _gridPart.X = this.X + 2;
            _gridPart.Y = this.Y + 3;
            _gridPart.Width = this.Width - 4;
            _gridPart.Height = this.Height - 4;
            _gridPart.DrawTiled(0, 0, this.Width - 4, this.Height - 4);
        }

        private void DrawBlazingEffect(double gameTime)
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
            _blazingPart.Position = this.Position;
            _blazingPart.Draw();
            _blazingSidePart.ColorShading = Color.White;
            _blazingSidePart.ColorShading.A = opacity;

            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].IsBlazing)
                {
                    continue;
                }
                _blazingSidePart.Position = SidePositions[x];
                _blazingSidePart.Draw();
            }

            _blazingMiddlePart.ColorShading.A = opacity;
            _blazingMiddlePart.Draw();
        }


        private void DrawFullEffect()
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
                _blazingSidePart.Draw();

            }
        }


        public override void Draw()
        {
            Draw( 0.0);
        }

        private void DrawBase()
        {
            _basePart.Position = this.Position;
            _basePart.Draw();
        }

        private void DrawText( int player, Vector2 position)
        {
            var textPosition = position.Clone();
            textPosition.X += 25;

            FontManager.DrawString(String.Format("{0:D3}", (int)_displayedLife[player]),
                    "DefaultFont", textPosition, Color.Black, FontAlign.Center);

            textPosition.X += 60;
            FontManager.DrawString(String.Format("{0:P0}", _displayedLife[player] / TrueCapacity),
        "DefaultFont", textPosition, Color.Black, FontAlign.Center);

        }

        private void DrawSides()
        {

            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].Playing)
                {
                    continue;
                }

                _sidePart.Position = SidePositions[x];
                _sidePart.Draw();

            }
            _middlePart.Draw();

            var position = MiddlePosition.Clone();
            position.X += 35;
            FontManager.DrawString(String.Format("{0:D3}", (int)TotalLife()), "LargeFont",
                    position, Color.Black, FontAlign.Center);
            position.X += 50;
            FontManager.DrawString(string.Format("{0:D3}", (int)TrueCapacity), "DefaultFont", position, Color.Black, FontAlign.Center);
        }

        #region Helper Methods

        public double TotalLife()
        {
            double sum = 0;
            for (int index = 0; index < Parent.Players.Length; index++)
            {
                if (Parent.Players[index].Playing)
                {
                    sum += _displayedLife[index];
                }

            }
            return sum;
        }

        private double TotalPositive()
        {
            double sum = 0;
            for (int index = 0; index < Parent.Players.Length; index++)
            {

                if ((Parent.Players[index].Playing && Parent.Players[index].Life >= 0))
                {
                    sum += _displayedLife[index];
                }
            }
            return sum;
        }

        public override void Reset()
        {
            for (int x = 0; x < 4; x++)
            {
                _displayedLife[x] = Parent.Players[x].Life;
            }
        }

        public int GetHighestBlockLevel(int player, double capacity)
        {
            double penaltyMx = Math.Max(0, TotalLife() / TotalPositive());

            for (int x = _blocksCount - 1; x >= 0; x--)
            {
                var minLife = capacity / _blocksCount * x;

                if (_displayedLife[player] * penaltyMx > minLife)
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

        private const double LIFE_CHANGE_SPEED = 8;
        public void UpdateDisplayedLife()
        {
            for (var x = 0; x < 4; x++)
            {
                var diff = Parent.Players[x].Life - _displayedLife[x];
                if (Math.Abs(diff) < 0.01)
                {
                    _displayedLife[x] = Parent.Players[x].Life;
                }
                else
                {
                    var changeMx = Math.Min(1,
                                            TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * LIFE_CHANGE_SPEED);
                    _displayedLife[x] += diff * (changeMx);
                }
            }
        }
        #endregion
    }
}
