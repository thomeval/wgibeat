using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class CoopLifebar : Lifebar
    {

        public bool SideLocationTop = false;
        private readonly double[] _displayedLife;

        private Sprite _basePart;
        private Sprite _sidePart;
        private SpriteMap _middlePart;

        public CoopLifebar()
        {
            _displayedLife = new double[4];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            DrawBase(spriteBatch);
            double penaltyMx = Math.Max(0,TotalLife()/TotalPositive());
            var frontSpriteMap = new SpriteMap
                                     {Columns = 1, Rows = 4, SpriteTexture = TextureManager.Textures["lifebarFront"]};

            int posX = this.X + 3;
            int capacity = 100*Participants();
            for (int x = 0; x < 4; x++)
            {
                if (!Parent.Players[x].Playing)
                {
                    continue;
                }

                var pieceWidth = (int) ((this.Width - 6) *(Parent.Players[x].Life/capacity));
                pieceWidth = (int) (penaltyMx * pieceWidth);
                if (pieceWidth > 0)
                {
                    frontSpriteMap.Draw(spriteBatch, x, pieceWidth, this.Height - 6, posX, this.Y + 3);
                    posX += pieceWidth;
                }
            }

            DrawSides(spriteBatch);
        }

        private double TotalPositive()
        {
            return (from e in Parent.Players where (e.Playing && e.Life >= 0) select e.Life).Sum();
        }

        private void DrawBase(SpriteBatch spriteBatch)
        {
            if (_basePart == null)
            {
                _basePart = new Sprite
                {
                    Height = this.Height,
                    Width = this.Width,
                    SpriteTexture = TextureManager.Textures["coopLifebarBase"]
                };
            }
            _basePart.X = this.X;
            _basePart.Y = this.Y;
            _basePart.Draw(spriteBatch);
        }

        private void DrawText(SpriteBatch spriteBatch, int player, int x, int y)
        {
            var position = new Vector2(x, y);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("{0:D3}", (int)Parent.Players[player].Life),
                    position, Color.Black);
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
            if (_sidePart == null)
            {
                _sidePart = new Sprite();
            }
            if (_middlePart == null)
            {
                _middlePart = new SpriteMap
                                  {Columns = 1, Rows = 2, SpriteTexture = TextureManager.Textures["coopLifebarMiddle"]};
            }

            _sidePart.Y = this.Y + this.Height;
            _sidePart.SpriteTexture = TextureManager.Textures["lifebarBaseSide"];
            //PlayerID appears on top for Player 3 and 4.
            if (SideLocationTop)
            {
                _sidePart.SpriteTexture = TextureManager.Textures["lifebarBaseSideUp"];
                _sidePart.Y = this.Y - 25;
                playerIdx += 2;
            }

            //Draw on the right side.

            if (Parent.Players[playerIdx + 1].Playing)
            {
                _sidePart.X = this.X + this.Width - 50;
                _sidePart.Draw(spriteBatch);
                DrawText(spriteBatch, playerIdx + 1, _sidePart.X + 5, _sidePart.Y);
            }
            //Draw on the left.
            if (Parent.Players[playerIdx].Playing)
            {
                _sidePart.X = this.X;
                _sidePart.Draw(spriteBatch);
                DrawText(spriteBatch, playerIdx, _sidePart.X + 5, _sidePart.Y);
            }

            _middlePart.Draw(spriteBatch, playerIdx/2, 139, 25, (this.X + this.Width - 134)/2, _sidePart.Y);
            DrawTotal(spriteBatch, (this.X + this.Width - 40)/2,_sidePart.Y);


        }


        public int Participants()
        {
            return (from e in Parent.Players where e.Playing select e).Count();
        }

        public double TotalLife()
        {
            return (from e in Parent.Players where e.Playing select e.Life).Sum();
        }
        public override void Reset()
        {
            for (int x = 0; x < 4; x++)
            {
                _displayedLife[x] = 0;
            }
        }
    }
}
