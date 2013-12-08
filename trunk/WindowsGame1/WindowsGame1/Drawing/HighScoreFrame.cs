using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Players;

namespace WGiBeat.Drawing
{
    public class HighScoreFrame : DrawableObject
    {
        private SpriteMap3D _gradeSpriteMap;
        private SpriteMap3D _difficultySpriteMap;
        private Sprite3D _baseSprite;

        public HighScoreEntry HighScoreEntry { get; set; }
        public bool EnableFadeout { get; set;}

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
            _difficultySpriteMap = new SpriteMap3D
            {
                Columns = 1,
                Rows = (int)Difficulty.COUNT + 1,
                Texture = TextureManager.Textures("PlayerDifficulties")
            };
            _gradeSpriteMap = new SpriteMap3D
            {
                Columns = 1,
                Rows = NUM_EVALUATIONS,
                Texture = TextureManager.Textures("EvaluationGrades")
            };
            _baseSprite = new Sprite3D { Texture = TextureManager.Textures("HighScoreFrame") };
        }

        public override void Draw()
        {

            CalculatePositions();
            SetOpacity();
            _baseSprite.Draw();
            if (HighScoreEntry == null)
            {
                FontManager.DrawString("No score", "TwoTech24", _namePosition, _textColor,
                                          FontAlign.Center);
                return;
            }

            var displayedName = String.IsNullOrEmpty(HighScoreEntry.Name) ? "GUEST" : HighScoreEntry.Name;
            _gradeSpriteMap.Draw(HighScoreEntry.Grade, 71, 25, _gradePosition);
            _difficultySpriteMap.Draw((int) HighScoreEntry.Difficulty + 1, 25, 25, _difficultyPosition);
            var displayedScore = string.Format("{0:N0}", HighScoreEntry.Score).Replace((char) 160, ',');
            FontManager.DrawString(displayedScore, "TwoTech24", _scorePosition, _textColor,
                                      FontAlign.Center);
            FontManager.DrawString("" + displayedName, "TwoTech24", _namePosition, _textColor,
                                      FontAlign.Center);
        }

        private void SetOpacity()
        {
            if (!EnableFadeout)
            {
                _opacity = 255;
                return;
            }
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
            _baseSprite.Position = this.Position;
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
