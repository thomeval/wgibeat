using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class HighScoreFrame : DrawableObject
    {
        private SpriteMap _gradeSpriteMap;
        private SpriteMap _difficultySpriteMap;
        private Sprite _baseSprite;

        public HighScoreEntry HighScoreEntry { get; set; }

        private const int NUM_EVALUATIONS = 19;

        private Vector2 _gradePosition;
        private Vector2 _scorePosition;
        private Vector2 _namePosition;
        private Vector2 _difficultyPosition;

        private Color _textColor = Color.Black;

        private byte _opacity;
        public HighScoreFrame()
        {
            this.Width = 165;
            this.Height = 110;
        }

        public void InitSprites()
        {
            _difficultySpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = (int)Difficulty.COUNT + 1,
                SpriteTexture = TextureManager.Textures["playerDifficulties"]
            };
            _gradeSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = NUM_EVALUATIONS,
                SpriteTexture = TextureManager.Textures["evaluationGrades"]
            };
            _baseSprite = new Sprite { SpriteTexture = TextureManager.Textures["HighScoreFrame"] };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            CalculatePositions();
            SetOpacity();
            _baseSprite.Draw(spriteBatch);
            if (HighScoreEntry != null)
            {
                var displayedName = String.IsNullOrEmpty(HighScoreEntry.Name) ? "GUEST" : HighScoreEntry.Name;
                _gradeSpriteMap.Draw(spriteBatch, HighScoreEntry.Grade, 71, 25, _gradePosition);
                _difficultySpriteMap.Draw(spriteBatch, (int)HighScoreEntry.Difficulty + 1, 25, 25, _difficultyPosition);
                TextureManager.DrawString(spriteBatch, "" + HighScoreEntry.Score, "TwoTech", _scorePosition, _textColor, FontAlign.CENTER);
                TextureManager.DrawString(spriteBatch, "" + displayedName, "TwoTech", _namePosition, _textColor, FontAlign.CENTER);
            }
            else
            {
                TextureManager.DrawString(spriteBatch, "No score", "TwoTech", _namePosition, _textColor, FontAlign.CENTER);
            }
        }

        private void SetOpacity()
        {
            if (HighScoreEntry != null)
            {
                _opacity = (byte) Math.Min(_opacity + 10, 255);
            }
            else
            {
                _opacity = (byte)Math.Max(_opacity - 10, 0);
            }
            _gradeSpriteMap.ColorShading.A = _opacity;
            _difficultySpriteMap.ColorShading.A = _opacity;
            _baseSprite.ColorShading.A = _opacity;
            _textColor.A = _opacity;
        }

        private void CalculatePositions()
        {
            _baseSprite.SetPosition(this.X, this.Y);
            _baseSprite.Height = this.Height;
            _baseSprite.Width = this.Width;
            _gradePosition.X = this.X + this.Width - 74;
            _gradePosition.Y = this.Y + this.Height - 26;
            _difficultyPosition.X = this.X + 40;
            _difficultyPosition.Y = this.Y + this.Height - 28;
            _scorePosition.X = this.X + (this.Width/2);
            _scorePosition.Y = this.Y + 27;
            _namePosition.X = this.X + (this.Width/2);
            _namePosition.Y = this.Y + 52;

        }
    }
}
