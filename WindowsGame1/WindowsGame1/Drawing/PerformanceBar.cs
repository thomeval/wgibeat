using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Drawing
{
    public class PerformanceBar : DrawableObject
    {

        public double Opacity { get; set; }
        private double _barOpacity { get; set; }
        public Player[] Players { get; set; }
        public GameType GameType { get; set; }

        public MetricsManager Metrics { get; set; }

        private SpriteMap _partsSpriteMap;
        private SpriteMap _leftSpriteMap;
        private Sprite _middleSprite;
        private Sprite _rightSprite;
        private Sprite _headerSprite;
        private const double BAR_SHOW_SPEED = 180;


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
                                     Rows = 6
                                 };
            _middleSprite = new Sprite {SpriteTexture = TextureManager.Textures("PerformanceBarMiddle")};
            _rightSprite = new Sprite {SpriteTexture = TextureManager.Textures("PerformanceBarRight")};
            _headerSprite = new Sprite
                                {
                                    SpriteTexture = TextureManager.Textures("PerformanceBarHeader"),
                                    Size = Metrics["PerformanceBarHeader.Size", 0]
                                };
        }
      
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_headerSprite == null)
            {
                InitSprites();
            }
            var position = this.Position.Clone();
            
            DrawHeader(spriteBatch, position);
            position.Y += _headerSprite.Height;

            if (GameType == GameType.SYNC_PRO || GameType == GameType.SYNC_PLUS)
            {
                DrawSingleBar(spriteBatch, position, 0,true);
                return;
            }
            for (int x = 0; x < Players.Length; x++)
            {

                if ((!Players[x].Playing))
                {
                    continue;
                }
                DrawSingleBar(spriteBatch, position, x, false);
                position.Y += this.Height;
            }
        }

        private void DrawHeader(SpriteBatch spriteBatch, Vector2 position)
        {
            var headerOffset = (this.Width/2.0f) - (_headerSprite.Width/2.0f);
            position.X += headerOffset;
            _headerSprite.Position = position;
            _headerSprite.ColorShading.A = (byte) Opacity;
            _headerSprite.Draw(spriteBatch);
            position.X -= headerOffset;   
        }

        private const int LEFT_SIDE_WIDTH = 50;
        private const int RIGHT_SIDE_WIDTH = 70;
        private void DrawSingleBar(SpriteBatch spriteBatch, Vector2 position, int player, bool allPlayers)
        {
            _rightSprite.ColorShading.A =
                _leftSpriteMap.ColorShading.A =
                _middleSprite.ColorShading.A = (byte) Opacity;
            _rightSprite.Size = new Vector2(RIGHT_SIDE_WIDTH, this.Height);
            _rightSprite.Position = position.Clone();
            _rightSprite.X += this.Width - RIGHT_SIDE_WIDTH;
            var barWidth = this.Width - RIGHT_SIDE_WIDTH - LEFT_SIDE_WIDTH;
            var totalBeatlines = (from e in Players[player].Judgements select e).Take(6).Sum();

            var idx = (Players[player].IsCPUPlayer) ? 4 : player;
            if (allPlayers)
            {
                idx = 5;
            }
            _leftSpriteMap.Draw(spriteBatch, idx, LEFT_SIDE_WIDTH, this.Height, position);
            position.X += LEFT_SIDE_WIDTH;
            _middleSprite.Width = barWidth;
            _middleSprite.Height = this.Height;
            _middleSprite.Position = position.Clone();

            var maxWidth = barWidth;
            var percentageText = " -----";

            if (totalBeatlines >= 5)
            {
                _partsSpriteMap.ColorShading.A = (byte) (_barOpacity * Opacity / 255);
                for (int y = 0; y < (int) BeatlineNoteJudgement.COUNT; y++)
                {
                    var width = (int) Math.Ceiling((double) (barWidth)*Players[player].Judgements[y]/totalBeatlines);
                    width = Math.Min(width, maxWidth);
                    maxWidth -= width;
                    _partsSpriteMap.Draw(spriteBatch, y, width, this.Height, position);
                    position.X += width;
                }
                _barOpacity = Math.Min(255, _barOpacity + (TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * BAR_SHOW_SPEED));
                percentageText = String.Format("{0:F1}%", Players[player].CalculatePercentage());
            }
            else
            {
                _barOpacity = 0;
            }
            _middleSprite.Draw(spriteBatch);

            _rightSprite.Draw(spriteBatch);
            _rightSprite.X += 35;
            _rightSprite.Y += (this.Height / 2) - 10;
            var textColour = Color.Black;
            textColour.A = (byte) Opacity;
            TextureManager.DrawString(spriteBatch, percentageText, "DefaultFont", _rightSprite.Position, textColour,
                                      FontAlign.CENTER);

            position.X = this.Position.X;
        }

        public int GetFreeLocation()
        {
            
            var result = 4;
            for (int x = 0; x < 4; x++)
            {
                if (!Players[x].Playing)
                {
                    result = x;
                }
            }

           return result;
            
        }

        public void Reset()
        {
            Opacity = 0;
        }

        public void SetPosition()
        {
            if (Metrics == null)
            {
                return;
            }

            var freeLocation = GetFreeLocation();
            var metric = (GameType == GameType.SYNC_PLUS || GameType == GameType.SYNC_PRO)
                             ? "SyncPerformanceBar"
                             : "PerformanceBar";
            this.Position = Metrics[metric, freeLocation];
            this.Size = Metrics[metric + ".Size", 0];
        }
    }
}
