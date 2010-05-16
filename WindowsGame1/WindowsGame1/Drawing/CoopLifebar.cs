using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class CoopLifebar : Lifebar
    {
        public readonly bool[] Playing;
        public bool SideLocationTop = false;
        private readonly double[] _life;
        private readonly double[] _displayedLife;

        private Sprite _basePart;
        private Sprite _sidePart;
 
        public CoopLifebar()
        {
            Playing = new bool[4];
            _life = new double[4];
            _displayedLife = new double[4];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            DrawBase(spriteBatch);
            //TODO: Implement leeching.

            var frontSpriteMap = new SpriteMap
                                     {Columns = 1, Rows = 4, SpriteTexture = TextureManager.Textures["lifebarFront"]};

            int posX = this.X + 3;
            int capacity = 125*Participants();
            for (int x = 0; x < 4; x++)
            {
                if (!Playing[x])
                {
                    continue;
                }

                var pieceWidth = (int) ((this.Width - 6) *(_life[x]/capacity));
                if (pieceWidth > 0)
                {
                    frontSpriteMap.Draw(spriteBatch, x, pieceWidth, this.Height - 6, posX, this.Y + 3);
                    posX += pieceWidth;
                }
            }

            DrawSides(spriteBatch);
        }

        private void DrawBase(SpriteBatch spriteBatch)
        {
            if (_basePart == null)
            {
                _basePart = new Sprite
                {
                    Height = this.Height,
                    Width = this.Width,
                    SpriteTexture = TextureManager.Textures["lifebarBase"]
                };
            }
            _basePart.X = this.X;
            _basePart.Y = this.Y;
            _basePart.Draw(spriteBatch);
        }

        private void DrawSides(SpriteBatch spriteBatch)
        {


            if (_sidePart == null)
            {
                _sidePart = new Sprite();
            }
            _sidePart.Y = this.Y + this.Height;
            _sidePart.SpriteTexture = TextureManager.Textures["lifebarBaseSide"];
            //SideLocation appears on top for Player 3 and 4.
            if (SideLocationTop)
            {
                _sidePart.SpriteTexture = TextureManager.Textures["lifebarBaseSideUp"];
                _sidePart.Y = this.Y - 25;
            }

            //Draw on the right side.
            _sidePart.X = this.X + this.Width - 50;
            _sidePart.Draw(spriteBatch);
            //Draw on the left.
            _sidePart.X = this.X;
            _sidePart.Draw(spriteBatch);

        }

        public override void SetLife(double amount)
        {
            throw new InvalidOperationException("Invalid operation for a CoopLifebar. Specifiy the player as well as amount.");
        }

        public void SetLife(double amount, int player)
        {
            _life[player] = amount;
        }

        public void AdjustLife(double amount, int player)
        {
            _life[player] += amount;
        }

        public int Participants()
        {
            return (from e in Playing where e select e).Count();
        }

        public double TotalLife()
        {
            return (from e in _life select e).Sum();
        }
        public override void Reset()
        {
            for (int x = 0; x < 4; x++)
            {
                _life[x] = 0;
                _displayedLife[x] = 0;
            }
        }
    }
}
