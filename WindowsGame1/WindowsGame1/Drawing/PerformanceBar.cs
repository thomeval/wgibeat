using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    public class PerformanceBar : DrawableObject
    {

        public byte Opacity { get; set; }
        public Player[] Players { get; set; }

        private SpriteMap _partsSpriteMap;
        private SpriteMap _leftSpriteMap;
        private Sprite _middleSprite;
        private Sprite _rightSprite;

        public PerformanceBar()
        {
            InitSprites();
        }

        private void InitSprites()
        {
            _partsSpriteMap = new SpriteMap
                                  {
                                      SpriteTexture = TextureManager.Textures["PerformanceBarParts"],
                                      Columns = 1,
                                      Rows = 6
                                  };
            _leftSpriteMap = new SpriteMap
                                 {
                                     SpriteTexture = TextureManager.Textures["PerformanceBarLeft"],
                                     Columns = 1,
                                     Rows = 5
                                 };
            _middleSprite = new Sprite {SpriteTexture = TextureManager.Textures["PerformanceBarMiddle"]};
            _rightSprite = new Sprite() {SpriteTexture = TextureManager.Textures["PerformanceBarRight"]};
        }

        
        public override void Draw(SpriteBatch spriteBatch)
        {
            var position = this.Position.Clone();
            var barWidth = this.Width - 120;
            var percentageText = "";
            //TODO: Draw heading
            position.Y += 25;


            for (int x = 0; x < Players.Length; x++)
            {
                if (!Players[x].Playing)
                    continue;

                _rightSprite.Position = position.Clone();
                _rightSprite.X += this.Width - 70;
                var totalBeatlines = (from e in Players[x].Judgements select e).Take(6).Sum();

                var idx = (Players[x].IsCPUPlayer) ? 4 : x;

                _leftSpriteMap.Draw(spriteBatch,idx,50,30,position);
                position.X += 50;
                _middleSprite.Width = barWidth;
                _middleSprite.Height = 30;
                _middleSprite.Position = position.Clone();

                var maxWidth = barWidth;
                if (totalBeatlines >= 5)
                {
                    _partsSpriteMap.ColorShading.A = Opacity;
                    for (int y = 0; y < (int) BeatlineNoteJudgement.COUNT; y++)
                    {
                        var width = (int) Math.Ceiling((double) (barWidth) *Players[x].Judgements[y]/totalBeatlines);
                        width = Math.Min(width, maxWidth);
                        maxWidth -= width;
                        _partsSpriteMap.Draw(spriteBatch, y, width, 30, position);
                        position.X += width;
                    }
                    Opacity = (byte) Math.Min(255, Opacity + 1);
                    percentageText = String.Format("{0:F1}%", Players[x].CalculatePercentage());
                }
                else
                {
                    percentageText = " -----";
                    Opacity = 0;
                }
                _middleSprite.Draw(spriteBatch);

                _rightSprite.Draw(spriteBatch);
                TextureManager.DrawString(spriteBatch, percentageText, "DefaultFont", _rightSprite.Position, Color.Black, FontAlign.LEFT);

                position.X = this.Position.X;
                position.Y += 30;
            }
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
