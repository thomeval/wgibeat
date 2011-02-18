﻿using System;
using System.Linq;
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
        private Sprite _nameBackground;
        private Sprite _levelBaseSprite;
        private Sprite _levelFrontSprite;

        public bool OptionChangeActive;
        private byte _optionControlOpacity;

        private Vector2 _nameTextPosition;
        private Vector2 _speedTextPosition;
        private Vector2 _difficultyTextPosition;
        private Vector2 _levelTextPosition;
        private Vector2 _expTextPosition;

        public PlayerOptionsFrame()
        {
            InitSprites();
            this.Width = 450;
            this.Height = 38;
            _optionControlOpacity = 0;
        }

        private void InitSprites()
        {
            _backgrounds = new SpriteMap
                               {
                                   Columns = 1,
                                   Rows = 5,
                                   SpriteTexture = TextureManager.Textures("PlayerOptionsFrame")
                               };
            _speedBlocks = new SpriteMap
                               {
                                   Columns = 1,
                                   Rows = 7,
                                   SpriteTexture = TextureManager.Textures("PlayerOptionsSpeedBlocks")
                               };
            _difficultyIcons = new SpriteMap
                                {
                                    Columns = 1,
                                    Rows = (int)Difficulty.COUNT + 1,
                                    SpriteTexture = TextureManager.Textures("PlayerDifficulties")
                                };
            _indicatorArrows = new SpriteMap
                               {
                                   Columns = 4,
                                   Rows = 1,
                                   SpriteTexture = TextureManager.Textures("IndicatorArrows")
                               };
            _levelBaseSprite = new Sprite
                                   {
                                       SpriteTexture = TextureManager.Textures("PlayerLevelBarBase"),
        
                                   };
            _levelFrontSprite = new Sprite {SpriteTexture = TextureManager.Textures("PlayerLevelBarFront"),
                                            
            };
            _nameBackground = new Sprite {SpriteTexture = TextureManager.BlankTexture()};

        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            var idx = Player.IsCPUPlayer ? 4 : PlayerIndex;
            _backgrounds.Draw(spriteBatch, idx, this.Width, this.Height, this.X, this.Y);
            _difficultyIcons.Draw(spriteBatch, (int)Player.PlayDifficulty + 1, 32, 32, this.X + 65, this.Y + 3);

            DrawNameBackgrounds(spriteBatch);
            DrawText(spriteBatch);
            DrawSpeedBlocks(spriteBatch);
            DrawLevelBar(spriteBatch);
        }

        private const int LEVELBAR_WIDTH = 318;
   
        private void DrawLevelBar(SpriteBatch spriteBatch)
        {
            if (Player.Profile == null)
            {
                return;
            }
            _levelBaseSprite.Position = new Vector2(this.X + 100, this.Y + 18);
            _levelFrontSprite.Position = new Vector2(this.X + 100, this.Y + 30);
            _levelBaseSprite.ColorShading.A = (byte)(255 - _optionControlOpacity);
            _levelFrontSprite.ColorShading.A = (byte)(255 - _optionControlOpacity);
            var progress = Player.GetLevelProgressSafe();
            progress = Math.Min(1, progress);
            _levelBaseSprite.Draw(spriteBatch);
            _levelFrontSprite.Width = (int) (LEVELBAR_WIDTH * progress);
            
            _levelFrontSprite.DrawTiled(spriteBatch,0,0, (int) (LEVELBAR_WIDTH * progress), 5 );

            //Draw level text.
            var playerlevel = String.Format("{0:00}", Player.GetLevelSafe());
            var scale = TextureManager.ScaleTextToFit(playerlevel, "TwoTech36", 34, 40);
            TextureManager.DrawString(spriteBatch,playerlevel, "TwoTech36", _levelTextPosition, scale, _textColor,
   FontAlign.RIGHT);

        }

        private readonly Color _blueBackground = new Color(128,128,255,128);
        private readonly Color _redBackground = new Color(255,128,128,128);

        private void DrawNameBackgrounds(SpriteBatch spriteBatch)
        {
            if (Player.Team == 1)
            {
                _nameBackground.ColorShading = _blueBackground;
            }
            else if (Player.Team == 2)
            {
                _nameBackground.ColorShading = _redBackground;
            }
            else
            {
                _nameBackground.ColorShading = Color.TransparentWhite;
            }
            _nameBackground.Width = this.Width - 133;
            _nameBackground.Height = this.Height - 2;
            _nameBackground.X = this.X + 100;
            _nameBackground.Y = this.Y + 1;
            _nameBackground.Draw(spriteBatch);
        }

        private Color _textColor = Color.Black;
        private void DrawText(SpriteBatch spriteBatch)
        {
            CalculateTextPositions();
            if (OptionChangeActive)
            {
                _optionControlOpacity = (byte) Math.Min(255, _optionControlOpacity + 10);
            }
            else
            {
                _optionControlOpacity = (byte) Math.Max(0, _optionControlOpacity - 10);
            }

      
            var playerName = (Player.Profile == null) ? "Guest" : this.Player.Profile.Name;
            if (Player.CPU)
            {
                playerName = "CPU";
            }
            
            DrawChangeControls(spriteBatch);
            _textColor.A = (byte)(255 - _optionControlOpacity);
            var scale = TextureManager.ScaleTextToFit(playerName, "TwoTech36", 310, 75);
            TextureManager.DrawString(spriteBatch, playerName, "TwoTech36", _nameTextPosition, scale, _textColor,
                                      FontAlign.CENTER);


        }

        private void DrawChangeControls(SpriteBatch spriteBatch)
        {
            _indicatorArrows.ColorShading.A = _optionControlOpacity;
            _indicatorArrows.Draw(spriteBatch, 1, 15, 15, (int)_difficultyTextPosition.X, (int)_difficultyTextPosition.Y + 25);
            _indicatorArrows.Draw(spriteBatch, 0, 15, 15, (int)_difficultyTextPosition.X + 15, (int)_difficultyTextPosition.Y + 25);
            _indicatorArrows.Draw(spriteBatch, 2, 15, 15, (int)_speedTextPosition.X - 32, (int)_speedTextPosition.Y - 8);
            _indicatorArrows.Draw(spriteBatch, 3, 15, 15, (int)_speedTextPosition.X - 17, (int)_speedTextPosition.Y - 8);
            var speedText = string.Format("{0:0.0}x", Player.BeatlineSpeed);
            _textColor.A = _optionControlOpacity;
            TextureManager.DrawString(spriteBatch, speedText, "TwoTech", _speedTextPosition, _textColor,
                                      FontAlign.RIGHT);

            TextureManager.DrawString(spriteBatch, "" + Player.PlayDifficulty, "TwoTech", _difficultyTextPosition, _textColor,
                                      FontAlign.LEFT);

            if (Player.Profile != null)
            {
                TextureManager.DrawString(spriteBatch,
                                          String.Format("{0}/{1}", Player.GetEXPSafe(), Player.GetNextEXPSafe()),
                                          "TwoTech", _expTextPosition, _textColor,
                                          FontAlign.CENTER);
            }
        }

        private void CalculateTextPositions()
        {
            //this.X + PlayerID.Width + Difficulty Icon width + half of available name space.
            _nameTextPosition.X = this.X + 260;
            _nameTextPosition.Y = this.Y - 8;
            _speedTextPosition.X = this.X + this.Width - 35;
            _speedTextPosition.Y = this.Y + 10;
            _difficultyTextPosition.X = this.X + this.X + 70 + 35;
            _difficultyTextPosition.Y = this.Y - 5;

            _levelTextPosition.X = this.X + this.Width - 35;
            _levelTextPosition.Y = this.Y;
            _expTextPosition.X = this.X + 260;
            _expTextPosition.Y = this.Y + 13;
        }

        private readonly double[] _speedOptions = { 0.5, 1.0, 1.5, 2.0, 3.0, 4.0, 6.0 };

        private const int BLOCK_HEIGHT = 5;
        private readonly Color _blockColor = new Color(64, 64, 64, 255);


        private void DrawSpeedBlocks(SpriteBatch spriteBatch)
        {
            var posY = this.Y + this.Height - BLOCK_HEIGHT - 1;
            for (int x = 0; x < _speedOptions.Length; x++)
            {

                var drawColor = Player.BeatlineSpeed >= _speedOptions[x] ? Color.White : _blockColor;
                _speedBlocks.ColorShading = drawColor;
                _speedBlocks.Draw(spriteBatch, 6 - x, 30, BLOCK_HEIGHT, this.X + this.Width - 31, posY);

                posY -= BLOCK_HEIGHT;
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
