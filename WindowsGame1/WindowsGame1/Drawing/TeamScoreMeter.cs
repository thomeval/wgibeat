using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class TeamScoreMeter:DrawableObject
    {

        public long BlueScore { get; set; }
        public long RedScore { get; set; }
        private Sprite3D _teamBaseSprite;
        private Sprite3D _teamGridSprite;
        private Sprite3D _tugBlueSprite;
        private Sprite3D _tugRedSprite;
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
            _teamBaseSprite = new Sprite3D { Texture = TextureManager.Textures("ScoreBaseTeam") };
            _teamGridSprite = new Sprite3D {Texture = TextureManager.Textures("ScoreGridTeam")};
            _tugBlueSprite = new Sprite3D {Texture = TextureManager.Textures("TugOfWarBlue")};
            _tugRedSprite = new Sprite3D {Texture = TextureManager.Textures("TugOfWarRed")};
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
        public override void Draw()
        {
            DrawBase();
            DrawBars();
            DrawGrid();
            DrawScoreText();
        }

        private void DrawGrid()
        {
            _teamGridSprite.Position = this.Position;
            _teamGridSprite.Draw();
        }

        private void DrawScoreText()
        {
            var blueTextPosition = new Vector2(this.X + 15, this.Y + 5);
            var redTextPosition = new Vector2(this.X + this.Width - 15, this.Y + 5);
                FontManager.DrawString("" + BlueScore, "LargeFont",
                                      blueTextPosition, Color.White, FontAlign.Left);
                FontManager.DrawString("" + RedScore, "LargeFont",
                      redTextPosition, Color.White, FontAlign.Right);

        }

        private void DrawBars()
        {
            var scoreDiff = CalculateBarDifference();
            var halfWidth = (this.Width/2 - 16);
            _tugBlueSprite.X = this.X + 16;
             _tugBlueSprite.Y = this.Y +3;
            _tugBlueSprite.Width = halfWidth + scoreDiff;
            _tugRedSprite.Width = halfWidth - scoreDiff;
            _tugRedSprite.X = _tugBlueSprite.X + _tugBlueSprite.Width;
            _tugRedSprite.Y = this.Y+3;
            _tugBlueSprite.DrawTiled((float) _blueBarTextureStart,0, _tugBlueSprite.Width, _tugBlueSprite.Texture.Height);
            _tugRedSprite.DrawTiled((float)_redBarTextureStart, 0, _tugRedSprite.Width, _tugRedSprite.Texture.Height);
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

        private void DrawBase()
        {
            _teamBaseSprite.Position = this.Position;
            _teamBaseSprite.Size = this.Size;
            _teamBaseSprite.Draw();

        }
    }
}
