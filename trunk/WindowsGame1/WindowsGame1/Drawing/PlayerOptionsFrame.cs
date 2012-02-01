using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Players;

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
                _levelDisplay.Player = value;
            }
        }

        private Sprite _background;
        private Sprite _speedBlocks;
        private SpriteMap _difficultyIcons;
        private SpriteMap _indicatorArrows;
        private SpriteMap _playerIdentifiers;

        private Sprite _nameBackground;

        private Vector2 _levelDisplayPosition;

        public bool OptionChangeActive;
        private byte _optionControlOpacity;

        private Vector2 _nameTextPosition;
        private Vector2 _speedTextPosition;
        private Vector2 _difficultyTextPosition;
        private Vector2 _expTextPosition;

        private ProfileLevelDisplay _levelDisplay;

        public PlayerOptionsFrame()
        {
            InitSprites();
            this.Width = 400;
            this.Height = 35;
            _optionControlOpacity = 0;
            _levelDisplay = new ProfileLevelDisplay();
        }

        private void InitSprites()
        {
            _background = new Sprite
                               {
                                   SpriteTexture = TextureManager.Textures("PlayerOptionsFrame")
                               };
            _speedBlocks = new Sprite
                               {
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
            _playerIdentifiers = new SpriteMap
                                     {
                                         Columns = 1,
                                         Rows = 5,
                                         SpriteTexture = TextureManager.Textures("PlayerIdentifiers")
                                     };
            _nameBackground = new Sprite {SpriteTexture = TextureManager.BlankTexture()};

        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            var idx = Player.IsCPUPlayer ? 4 : PlayerIndex;
            _background.Height = this.Height;
            _background.Width = this.Width;
            _background.Position = this.Position;
            _background.Draw(spriteBatch);
            
            _playerIdentifiers.Draw(spriteBatch,idx, 45, this.Height - 8,this.X + 3, this.Y + 4);
            _difficultyIcons.Draw(spriteBatch, (int)Player.PlayerOptions.PlayDifficulty + 1, 30, 30, this.X + 53, this.Y + 3);

            DrawNameBackgrounds(spriteBatch);
            DrawText(spriteBatch);
            DrawSpeedBlocks(spriteBatch);
            DrawLevelBar(spriteBatch);
        }

        private void DrawLevelBar(SpriteBatch spriteBatch)
        {
        
            _levelDisplay.Position = _levelDisplayPosition;
            _levelDisplay.Opacity = (byte) (255 - _optionControlOpacity);
            _levelDisplay.Width = 288;
            _levelDisplay.Draw(spriteBatch);
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
            _nameBackground.Width = this.Width - 113;
            _nameBackground.Height = this.Height - 2;
            _nameBackground.X = this.X + 85;
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
            var scale = TextureManager.ScaleTextToFit(playerName, "TwoTech36", 270, 50);
            TextureManager.DrawString(spriteBatch, playerName, "TwoTech36", _nameTextPosition, scale, _textColor,
                                      FontAlign.CENTER);


        }

        private void DrawChangeControls(SpriteBatch spriteBatch)
        {
            _indicatorArrows.ColorShading.A = _optionControlOpacity;
            _indicatorArrows.Draw(spriteBatch, 1, 15, 15, (int)_difficultyTextPosition.X, (int)_difficultyTextPosition.Y + 22);
            _indicatorArrows.Draw(spriteBatch, 0, 15, 15, (int)_difficultyTextPosition.X + 15, (int)_difficultyTextPosition.Y + 22);
            _indicatorArrows.Draw(spriteBatch, 2, 15, 15, (int)_speedTextPosition.X - 32, (int)_speedTextPosition.Y - 10);
            _indicatorArrows.Draw(spriteBatch, 3, 15, 15, (int)_speedTextPosition.X - 17, (int)_speedTextPosition.Y - 10);
            var speedText = string.Format("{0:0.0}x", Player.PlayerOptions.BeatlineSpeed);
            _textColor.A = _optionControlOpacity;
            TextureManager.DrawString(spriteBatch, speedText, "TwoTech20", _speedTextPosition, _textColor,
                                      FontAlign.RIGHT);

            TextureManager.DrawString(spriteBatch, "" + Player.PlayerOptions.PlayDifficulty, "TwoTech20", _difficultyTextPosition, _textColor,
                                      FontAlign.LEFT);

            if (Player.Profile != null)
            {
                TextureManager.DrawString(spriteBatch,
                                          String.Format("{0}/{1}", Player.GetEXP(), Player.GetNextEXPSafe()),
                                          "TwoTech20", _expTextPosition, _textColor,
                                          FontAlign.CENTER);
            }
        }

        private void CalculateTextPositions()
        {
            //this.X + PlayerID.Width + Difficulty Icon width + half of available name space.
            _nameTextPosition.X = this.X + 210;
            _nameTextPosition.Y = this.Y - 8;
            _speedTextPosition.X = this.X + this.Width - 29;
            _speedTextPosition.Y = this.Y + 12;
            _difficultyTextPosition.X = this.X + 87;
            _difficultyTextPosition.Y = this.Y - 5;


            _levelDisplayPosition.X = this.X + 85;
            _levelDisplayPosition.Y = this.Y + 16;
            _expTextPosition.X = this.X + 230;
            _expTextPosition.Y = this.Y + 13;
        }

        private readonly double[] _speedOptions = { 0.5, 1.0, 1.5, 2.0, 3.0, 4.0, 6.0 };

        private readonly Color _blockColor = new Color(64, 64, 64, 255);


        private void DrawSpeedBlocks(SpriteBatch spriteBatch)
        {
            const int TEXTURE_HEIGHT = 140;
            const int TEXTURE_WIDTH = 100;

            const int DRAW_HEIGHT = 35;
            _speedBlocks.Y = this.Y + 1;
            _speedBlocks.X = this.X + this.Width - _speedBlocks.Width - 2;
            _speedBlocks.ColorShading = _blockColor;
            _speedBlocks.Height = DRAW_HEIGHT;
            _speedBlocks.Width = 25;
            _speedBlocks.Draw(spriteBatch);

            _speedBlocks.ColorShading = Color.White;
            var numberLit = (from e in _speedOptions where Player.PlayerOptions.BeatlineSpeed >= e select e).Count();
            var litHeight = 1.0 * DRAW_HEIGHT/_speedOptions.Count()*numberLit;
            _speedBlocks.Height = (int) litHeight;
            _speedBlocks.Y = this.Y +1 + DRAW_HEIGHT - _speedBlocks.Height;

            var texV2 = TEXTURE_HEIGHT/_speedOptions.Count()*numberLit;
            _speedBlocks.DrawTiled(spriteBatch, 0, TEXTURE_HEIGHT - texV2, TEXTURE_WIDTH, texV2);

          
        }

        public void AdjustSpeed(int amount)
        {
            var idx = Array.IndexOf(_speedOptions, Player.PlayerOptions.BeatlineSpeed);
            idx += amount;
            idx = Math.Min(_speedOptions.Count() - 1, Math.Max(0, idx));
            Player.PlayerOptions.BeatlineSpeed = _speedOptions[idx];
        }

        public void AdjustDifficulty(int amount)
        {
            var idx = (int)Player.PlayerOptions.PlayDifficulty;
            idx += amount;
            idx = Math.Min(Player.GetMaxDifficulty(), Math.Max(0, idx));
            Player.PlayerOptions.PlayDifficulty = (Difficulty)idx;
        }
    }
}
