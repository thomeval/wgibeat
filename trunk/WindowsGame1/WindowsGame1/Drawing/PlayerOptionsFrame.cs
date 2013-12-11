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

        private Sprite3D _background;
        private Sprite3D _speedBlocks;
        private SpriteMap3D _difficultyIcons;
        private SpriteMap3D _indicatorArrows;
        private SpriteMap3D _playerIdentifiers;

        private Sprite3D _nameBackground;

        private Vector2 _levelDisplayPosition;

        public bool OptionChangeActive;
        private double _optionControlOpacity;

        private Vector2 _nameTextPosition;
        private Vector2 _speedTextPosition;
        private Vector2 _difficultyTextPosition;
        private Vector2 _expTextPosition;

        private ProfileLevelDisplay _levelDisplay;

        private const int FADEIN_SPEED = 600;
        private const int FADEOUT_SPEED = 600;

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
            _background = new Sprite3D
                               {
                                   Texture = TextureManager.Textures("PlayerOptionsFrame")
                                   
                               };
            _speedBlocks = new Sprite3D
                               {
                                   Texture = TextureManager.Textures("PlayerOptionsSpeedBlocks")
                               };
            _difficultyIcons = new SpriteMap3D
                                {
                                    Columns = 1,
                                    Rows = (int)Difficulty.COUNT + 1,
                                    Texture = TextureManager.Textures("PlayerDifficulties")
                                };
            _indicatorArrows = new SpriteMap3D
                               {
                                   Columns = 4,
                                   Rows = 1,
                                   Texture = TextureManager.Textures("IndicatorArrows")
                               };
            _playerIdentifiers = new SpriteMap3D
                                     {
                                         Columns = 1,
                                         Rows = 5,
                                         Texture = TextureManager.Textures("PlayerIdentifiers")
                                     };
            _nameBackground = new Sprite3D {Texture = TextureManager.BlankTexture()};

        }

        public override void Draw()
        {

            var idx = Player.IsCPUPlayer ? 4 : PlayerIndex;
            _background.Size = this.Size;
            _background.Position = this.Position;
            _background.Draw();
            
            _playerIdentifiers.Draw(idx, 45, this.Height - 8,this.X + 3, this.Y + 4);
            _difficultyIcons.Draw( (int)Player.PlayerOptions.PlayDifficulty + 1, 30, 30, this.X + 53, this.Y + 3);

            DrawNameBackgrounds();
            DrawText();
            DrawSpeedBlocks();
            DrawLevelBar();
        }

        private void DrawLevelBar()
        {
        
            _levelDisplay.Position = _levelDisplayPosition;
            _levelDisplay.Opacity = (byte) (255 - _optionControlOpacity);
            _levelDisplay.Width = this.Width - 112;
            _levelDisplay.Draw();
        }


        private readonly Color _blueBackground = new Color(128,128,255,128);
        private readonly Color _redBackground = new Color(255,128,128,128);

        private void DrawNameBackgrounds()
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
            _nameBackground.Draw();
        }

        private Color _textColor = Color.Black;
        private void DrawText()
        {
            CalculateTextPositions();
            var timeDiff = TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds;
            if (OptionChangeActive)
            {
                _optionControlOpacity =  Math.Min(255, _optionControlOpacity + (FADEIN_SPEED *timeDiff));
            }
            else
            {
                _optionControlOpacity = Math.Max(0, _optionControlOpacity - (FADEOUT_SPEED * timeDiff));
            }

      
            var playerName = (Player.Profile == null) ? "Guest" : this.Player.Profile.Name;
            if (Player.CPU)
            {
                playerName = "CPU";
            }
            
            DrawChangeControls();
            _textColor.A = (byte)(255 - _optionControlOpacity);
            var scale = FontManager.ScaleTextToFit(playerName, "TwoTech36", this.Width - 170, 50);
            FontManager.DrawString(playerName, "TwoTech36", _nameTextPosition, scale, _textColor,
                                      FontAlign.Center);


        }

        private void DrawChangeControls()
        {
            _indicatorArrows.ColorShading.A = (byte) _optionControlOpacity;
            _indicatorArrows.Draw( 1, 15, 15, _difficultyTextPosition.X, _difficultyTextPosition.Y + 22);
            _indicatorArrows.Draw( 0, 15, 15, _difficultyTextPosition.X + 15, _difficultyTextPosition.Y + 22);
            _indicatorArrows.Draw( 2, 15, 15, _speedTextPosition.X - 32, _speedTextPosition.Y - 10);
            _indicatorArrows.Draw( 3, 15, 15, _speedTextPosition.X - 17, _speedTextPosition.Y - 10);
            var speedText = string.Format("{0:0.0}x", Player.PlayerOptions.BeatlineSpeed);
            _textColor.A = (byte) _optionControlOpacity;
            FontManager.DrawString(speedText, "TwoTech20", _speedTextPosition, _textColor,
                                      FontAlign.Right);

            FontManager.DrawString("" + Player.PlayerOptions.PlayDifficulty, "TwoTech20", _difficultyTextPosition, _textColor,
                                      FontAlign.Left);

            if (Player.Profile != null)
            {
                FontManager.DrawString(String.Format("{0}/{1}", Player.GetEXP(), Player.GetNextEXPSafe()),
                                          "TwoTech20", _expTextPosition, _textColor,
                                          FontAlign.Center);
            }
        }

        private void CalculateTextPositions()
        {
            //this.X + PlayerID.Width + Difficulty Icon width + half of available name space.
            _nameTextPosition.X = this.X + 200;
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

        private void DrawSpeedBlocks()
        {

            const int DRAW_HEIGHT = 35;
            _speedBlocks.Y = this.Y + 1;
            _speedBlocks.X = this.X + this.Width - _speedBlocks.Width - 2;
            _speedBlocks.ColorShading = _blockColor;
            _speedBlocks.Height = DRAW_HEIGHT;
            _speedBlocks.Width = 25;
            _speedBlocks.Draw();

            _speedBlocks.ColorShading = Color.White;
            var numberLit = (from e in _speedOptions where Player.PlayerOptions.BeatlineSpeed >= e select e).Count();
            var litHeight = 1.0 * DRAW_HEIGHT/_speedOptions.Count()*numberLit;
            _speedBlocks.Height = (int) litHeight;
            _speedBlocks.Y = this.Y +1 + DRAW_HEIGHT - _speedBlocks.Height;

            var texV2 = _speedBlocks.Texture.Height /_speedOptions.Count()*numberLit;
            _speedBlocks.DrawTiled(0, _speedBlocks.Texture.Height - texV2, _speedBlocks.Texture.Width, texV2);

          
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
