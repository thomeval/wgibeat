using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Drawing
{
    public class PerformanceBar : DrawableObject
    {

        public double Opacity { get; set; }
        public Player[] Players { get; set; }
        public GameType GameType { get; set; }

        private SpriteMap _partsSpriteMap;
        private SpriteMap _leftSpriteMap;
        private Sprite _middleSprite;
        private Sprite _rightSprite;
        private Sprite _headerSprite;
        private const double BAR_SHOW_SPEED = 180;


        public PerformanceBar()
        {
            InitSprites();
        }

        private void InitSprites()
        {
            _partsSpriteMap = new SpriteMap
                                  {
                                      SpriteTexture = TextureManager.Textures("PerformanceBarParts"),
                                      Columns = 1,
                                      Rows = 6
                                  };
            _leftSpriteMap = new SpriteMap
                                 {
                                     SpriteTexture = TextureManager.Textures("PerformanceBarLeft"),
                                     Columns = 1,
                                     Rows = 5
                                 };
            _middleSprite = new Sprite {SpriteTexture = TextureManager.Textures("PerformanceBarMiddle")};
            _rightSprite = new Sprite {SpriteTexture = TextureManager.Textures("PerformanceBarRight")};
            _headerSprite = new Sprite {SpriteTexture = TextureManager.Textures("PerformanceBarHeader")};
        }
      
        public override void Draw(SpriteBatch spriteBatch)
        {
            var position = this.Position.Clone();
            
            DrawHeader(spriteBatch, position);
            position.Y += 30;

            if (GameType == GameType.SYNC_PRO || GameType == GameType.SYNC_PLUS)
            {
                DrawSingleBar(spriteBatch, position, 0);
                return;
            }
            for (int x = 0; x < Players.Length; x++)
            {

                if ((!Players[x].Playing))
                {
                    continue;
                }
                DrawSingleBar(spriteBatch, position, x);
                position.Y += 30;
            }
        }

        private void DrawHeader(SpriteBatch spriteBatch, Vector2 position)
        {
            var headerOffset = (this.Width/2.0f) - (_headerSprite.Width/2.0f);
            position.X += headerOffset;
            _headerSprite.Position = position;
            _headerSprite.Draw(spriteBatch);
            position.X -= headerOffset;   
        }

        private void DrawSingleBar(SpriteBatch spriteBatch, Vector2 position, int player)
        {
            _rightSprite.Position = position.Clone();
            _rightSprite.X += this.Width - 70;
            var barWidth = this.Width - 120;
            var totalBeatlines = (from e in Players[player].Judgements select e).Take(6).Sum();

            var idx = (Players[player].IsCPUPlayer) ? 4 : player;

            _leftSpriteMap.Draw(spriteBatch, idx, 50, 30, position);
            position.X += 50;
            _middleSprite.Width = barWidth;
            _middleSprite.Height = 30;
            _middleSprite.Position = position.Clone();

            var maxWidth = barWidth;
            var percentageText = " -----";

            if (totalBeatlines >= 5)
            {
                _partsSpriteMap.ColorShading.A = (byte) Opacity;
                for (int y = 0; y < (int) BeatlineNoteJudgement.COUNT; y++)
                {
                    var width = (int) Math.Ceiling((double) (barWidth)*Players[player].Judgements[y]/totalBeatlines);
                    width = Math.Min(width, maxWidth);
                    maxWidth -= width;
                    _partsSpriteMap.Draw(spriteBatch, y, width, 30, position);
                    position.X += width;
                }
                Opacity = Math.Min(255, Opacity + (TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * BAR_SHOW_SPEED));
                percentageText = String.Format("{0:F1}%", Players[player].CalculatePercentage());
            }
            else
            {
                Opacity = 0;
            }
            _middleSprite.Draw(spriteBatch);

            _rightSprite.Draw(spriteBatch);
            _rightSprite.X += 35;
            _rightSprite.Y += 5;
            TextureManager.DrawString(spriteBatch, percentageText, "DefaultFont", _rightSprite.Position, Color.Black,
                                      FontAlign.CENTER);

            position.X = this.Position.X;
        }

        public int GetFreeLocation(bool coop)
        {
            var result = 4;
            for (int x = 0; x < 4; x++)
            {
                if (!Players[x].Playing)
                {
                    result = x;
                }
            }

            if (coop)
            {
                if ((!Players[0].Playing) && (!Players[1].Playing))
                {
                    result = 0;
                }
                else if ((!Players[2].Playing) && (!Players[3].Playing))
                {
                    result = 2;
                }
                else
                {
                    result = 4;
                }
            }
            return result;
            
        }

        public void Reset()
        {
            Opacity = 0;
        }
    }
}
