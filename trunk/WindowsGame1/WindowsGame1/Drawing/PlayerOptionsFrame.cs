using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class PlayerOptionsFrame : DrawableObject
    {

        public int PlayerIndex { get; set; }
        private Player _player;
        public Player Player
        {
            get { return _player; }
            set
            {
                _player = value;
            }
        }

        private SpriteMap _backgrounds;
        private SpriteMap _speedBlocks;
        private SpriteMap _difficultyIcons;
        private SpriteMap _indicatorArrows;

        public bool SpeedChangeActive;
        private byte _speedTextOpacity;

        private Vector2 _nameTextPosition;
        private Vector2 _speedTextPosition;
        private Vector2 _difficultyTextPosition;

        public PlayerOptionsFrame()
        {
            InitSprites();
            this.Width = 375;
            this.Height = 38;
            _speedTextOpacity = 0;
        }

        private void InitSprites()
        {
            _backgrounds = new SpriteMap
                               {
                                   Columns = 1,
                                   Rows = 4,
                                   SpriteTexture = TextureManager.Textures["PlayerOptionsFrame"]
                               };
            _speedBlocks = new SpriteMap
                               {
                                   Columns = 1,
                                   Rows = 7,
                                   SpriteTexture = TextureManager.Textures["PlayerOptionsSpeedBlocks"]
                               };
            _difficultyIcons = new SpriteMap
                                {
                                    Columns = 1,
                                    Rows = (int)Difficulty.COUNT + 1,
                                    SpriteTexture = TextureManager.Textures["playerDifficulties"]
                                };
            _indicatorArrows = new SpriteMap
                               {
                                   Columns = 4,
                                   Rows = 1,
                                   SpriteTexture = TextureManager.Textures["IndicatorArrows"]
                               };

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _backgrounds.Draw(spriteBatch, PlayerIndex, this.Width, this.Height, this.X, this.Y);
            _difficultyIcons.Draw(spriteBatch, (int)Player.PlayDifficulty + 1, 32, 32, this.X + 65, this.Y + 2);

            DrawText(spriteBatch);

            DrawSpeedBlocks(spriteBatch);
        }

        private Color _textColor = Color.Black;
        private void DrawText(SpriteBatch spriteBatch)
        {
            CalculateTextPositions();
            if (SpeedChangeActive)
            {
                _speedTextOpacity = (byte)Math.Min(_speedTextOpacity + 10, 255);
            }
            else
            {
                _speedTextOpacity = (byte)Math.Max(_speedTextOpacity - 10, 0);
            }

            var playerName = string.IsNullOrEmpty(Player.Name) ? "Guest" : this.Player.Name;
            //this.X + PlayerID.Width + Difficulty Icon width + half of available name space.

            DrawChangeControls(spriteBatch);
            _textColor.A = (byte)(255 - _speedTextOpacity);
            //  TextureManager.SetClipRectangle(this.X + 100, this.Y, this.X + this.Width - 160, this.Height);
            TextureManager.DrawString(spriteBatch, playerName, "TwoTechLarge", _nameTextPosition, _textColor,
                                      FontAlign.CENTER);
            //  TextureManager.ResetClipRectangle();


        }

        private void DrawChangeControls(SpriteBatch spriteBatch)
        {
            _indicatorArrows.ColorShading.A = _speedTextOpacity;
            _indicatorArrows.Draw(spriteBatch,1,15,15,(int) _difficultyTextPosition.X,(int) _difficultyTextPosition.Y + 25);
            _indicatorArrows.Draw(spriteBatch, 0, 15, 15, (int)_difficultyTextPosition.X + 15, (int)_difficultyTextPosition.Y + 25);
            _indicatorArrows.Draw(spriteBatch, 2, 15, 15, (int)_speedTextPosition.X - 32, (int)_speedTextPosition.Y - 8);
            _indicatorArrows.Draw(spriteBatch, 3, 15, 15, (int)_speedTextPosition.X - 17, (int)_speedTextPosition.Y - 8);
            var speedText = string.Format("{0:0.0}x", Player.BeatlineSpeed);
            _textColor.A = _speedTextOpacity;
            TextureManager.DrawString(spriteBatch, speedText, "TwoTech", _speedTextPosition, _textColor,
                                      FontAlign.RIGHT);

            TextureManager.DrawString(spriteBatch, "" + Player.PlayDifficulty, "TwoTech", _difficultyTextPosition, _textColor,
                                      FontAlign.LEFT);
        }

        private void CalculateTextPositions()
        {
            _nameTextPosition.X = this.X + 70 + 35 + 110;
            _nameTextPosition.Y = this.Y - 10;
            _speedTextPosition.X = this.X + this.Width - 35;
            _speedTextPosition.Y = this.Y + 10;
            _difficultyTextPosition.X = this.X + this.X + 70 + 35;
            _difficultyTextPosition.Y = this.Y - 5;
        }

        private readonly double[] _speedOptions = { 0.5, 1.0, 1.5, 2.0, 3.0, 4.0, 6.0 };

        private const int BLOCK_HEIGHT = 5;
        private void DrawSpeedBlocks(SpriteBatch spriteBatch)
        {
            var posY = this.Y + this.Height - BLOCK_HEIGHT;
            for (int x = 0; x < _speedOptions.Length; x++)
            {
                if (Player.BeatlineSpeed >= _speedOptions[x])
                {
                    _speedBlocks.Draw(spriteBatch, 6 - x, 30, BLOCK_HEIGHT, this.X + this.Width - 31, posY);
                    posY -= BLOCK_HEIGHT;
                }
            }
        }

        public void AdjustSpeed(int amount)
        {
            var idx = Array.IndexOf(_speedOptions, Player.BeatlineSpeed);
            idx += amount;
            idx = Math.Min(_speedOptions.Count() - 1, Math.Max(0, idx));
            Player.BeatlineSpeed = _speedOptions[idx];
        }

        public void AdjustDifficulty(int amount)
        {
            var idx = (int)Player.PlayDifficulty;
            idx += amount;
            idx = Math.Min((int)Difficulty.COUNT - 1, Math.Max(0, idx));
            Player.PlayDifficulty = (Difficulty)idx;
        }
    }
}
