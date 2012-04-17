using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class TeamScoreMeter:DrawableObject
    {

        public long BlueScore { get; set; }
        public long RedScore { get; set; }
        private Sprite _teamBaseSprite;
        private Sprite _teamGridSprite;
        private Sprite _tugBlueSprite;
        private Sprite _tugRedSprite;
        private double _blueBarTextureStart;
        private double _redBarTextureStart;
        private const double ANIMATION_SPEED = 35;

        public TeamScoreMeter()
        {
            this.Width = 280;
            this.Height = 40;
        }
        public void InitSprites()
        {
            _teamBaseSprite = new Sprite { SpriteTexture = TextureManager.Textures("ScoreBaseTeam") };
            _teamGridSprite = new Sprite {SpriteTexture = TextureManager.Textures("ScoreGridTeam")};
            _tugBlueSprite = new Sprite {SpriteTexture = TextureManager.Textures("TugOfWarBlue")};
            _tugRedSprite = new Sprite {SpriteTexture = TextureManager.Textures("TugOfWarRed")};
        }

        public void Update()
        {
            double amount = ANIMATION_SPEED * TextureManager.LastDrawnPhraseDiff;
            _redBarTextureStart = (_redBarTextureStart + amount * _tugRedSprite.Width / 109) % 109;
            _blueBarTextureStart = (_blueBarTextureStart - amount * _tugBlueSprite.Width / 109);
            if (_blueBarTextureStart < 0)
            {
                _blueBarTextureStart += 109;
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawBase(spriteBatch);
            DrawBars(spriteBatch);
            DrawGrid(spriteBatch);
            DrawScoreText(spriteBatch);
        }

        private void DrawGrid(SpriteBatch spriteBatch)
        {
            _teamGridSprite.Position = this.Position;
            _teamGridSprite.Draw(spriteBatch);
        }

        private void DrawScoreText(SpriteBatch spriteBatch)
        {
            var blueTextPosition = new Vector2(this.X + 10, this.Y + 5);
            var redTextPosition = new Vector2(this.X + this.Width - 10, this.Y + 5);
                TextureManager.DrawString(spriteBatch, "" + BlueScore, "LargeFont",
                                      blueTextPosition, Color.White, FontAlign.LEFT);
                TextureManager.DrawString(spriteBatch, "" + RedScore, "LargeFont",
                      redTextPosition, Color.White, FontAlign.RIGHT);

        }

        private void DrawBars(SpriteBatch spriteBatch)
        {
            var scoreDiff = CalculateBarDifference();
            _tugBlueSprite.X = this.X + 16;
             _tugBlueSprite.Y = this.Y +3;
            _tugBlueSprite.Width = 124 + scoreDiff;
            _tugRedSprite.Width = 124 - scoreDiff;
            _tugRedSprite.X = _tugBlueSprite.X + _tugBlueSprite.Width;
            _tugRedSprite.Y = this.Y+3;
            _tugBlueSprite.DrawTiled(spriteBatch,(int)_blueBarTextureStart,0, _tugBlueSprite.Width, _tugBlueSprite.Height);
            _tugRedSprite.DrawTiled(spriteBatch, (int)_redBarTextureStart, 0, _tugRedSprite.Width, _tugRedSprite.Height);
        }

        private const int BAR_MAX_LENGTH = 120;
        private int CalculateBarDifference()
        {
            if (BlueScore - RedScore == 0)
            {
                return 0;
            }

            var amount = (Math.Log(Math.Abs(BlueScore - RedScore),10) -2) * 30;
            if (RedScore > BlueScore)
            {
                amount *= -1;
            }
            amount = Math.Max(-BAR_MAX_LENGTH,Math.Min(BAR_MAX_LENGTH, amount));
            return (int) amount;
        }

        private void DrawBase(SpriteBatch spriteBatch)
        {
            _teamBaseSprite.Position = this.Position;
            _teamBaseSprite.Draw(spriteBatch);

        }
    }
}
