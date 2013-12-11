using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using Microsoft.Xna.Framework.Input;
using WGiBeat.Managers;


/* Instructions
 * 
 * 1. Switch to a player by pressing any of his keys.
 * 2. Move to action you wish to change.
 * 3. Press any player's START action button.
 * 4. Press key that you wish to use to perform that action.
 * 5. Voila.
 * 
 */

namespace WGiBeat.Screens
{
    public class KeyOptionScreen : GameScreen
    {
        private int _currentPlayer = 1;
        private int _selectedAction;

        private Sprite3D _backgroundSprite;
        private Sprite3D _headerSprite;
        private SpriteMap3D _gridInsideSpriteMap;
        private SpriteMap3D _gridTopSpriteMap;
        private SpriteMap3D _gridSideSpriteMap;
        private Sprite3D _gridBorderSprite;
        private Sprite3D _instructionBaseSprite;

        private readonly List<ActionBinding> _actionBindings = new List<ActionBinding>();

        private readonly string[] _actions = { "LEFT", "RIGHT", "UP", "DOWN", "BEATLINE", "START", "SELECT", "Reset Defaults" };
        private const int _normalActionCount = 7;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private Vector2 _bindingPosition;

        public KeyOptionScreen(GameCore core)
            : base(core)
        {
        }

        #region Initialization

        public override void Initialize()
        {
            InitSprites();
            CreateBindingList();
        }

        private void InitSprites()
        {

            _backgroundSprite = new Sprite3D
            {
                Size = Core.Metrics["ScreenBackground.Size", 0],
                Position = Core.Metrics["ScreenBackground", 0],
                Texture = TextureManager.Textures("AllBackground"),
            };

            _headerSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("KeyOptionHeader"),
                Position = Core.Metrics["ScreenHeader", 0],
                Size = Core.Metrics["ScreenHeader.Size", 0]
            };

            _gridTopSpriteMap = new SpriteMap3D
                                    {
                                        Columns = 4,
                                        Rows = 2,
                                        Texture = TextureManager.Textures("KeyOptionGridTop")
                                    };
            _gridSideSpriteMap = new SpriteMap3D
                                     {
                                         Columns = 4,
                                         Rows = 2,
                                         Texture = TextureManager.Textures("KeyOptionGridSide")
                                     };
            _gridInsideSpriteMap = new SpriteMap3D
                                       {
                                           Columns = 1,
                                           Rows = 4,
                                           Texture = TextureManager.Textures("KeyOptionGridInside"),
                                       };
            _gridBorderSprite = new Sprite3D
                                    {
                                        Texture = TextureManager.Textures("KeyOptionGridBorder"),
                                        Position = Core.Metrics["KeyOptionGridBorder",0]
                                    };
            _instructionBaseSprite = Core.Metrics.SetupFromMetrics("KeyOptionInstructionBase", 0);

        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(gameTime);
            DrawOverlay();
            DrawText();
        }

        private void DrawBackground(GameTime gameTime)
        {
            _backgroundSprite.Draw();
            _field.Draw(gameTime);
            _instructionBaseSprite.Draw();
        }

        private Vector2 _actionPosition;
        private Vector2 _textPosition;
        private void DrawOverlay()
        {
            _headerSprite.Draw();

            //Draw Grid top
            for (int x = 0; x < 4; x++)
            {
                var idx = x;
                if (x != (_currentPlayer - 1))
                {
                    idx += 4;
                }
                _gridTopSpriteMap.Draw( idx,Core.Metrics["KeyOptionGridTop.Size",0], Core.Metrics["KeyOptionGridTop", x]);
            }

            //Draw Grid Side

            _actionPosition = Core.Metrics["KeyOptionGridSide", 0].Clone();
            var actionSize = Core.Metrics["KeyOptionGridSide.Size", 0];
            _textPosition = Core.Metrics["KeyOptionGridSideText", 0].Clone();
            for (int x = 0; x < _actions.Length; x++)
            {
                var idx = _currentPlayer - 1;
                if (x != _selectedAction)
                {
                    idx += 4;
                }
                _gridSideSpriteMap.Draw( idx, actionSize,_actionPosition);
                FontManager.DrawString(_actions[x], "LargeFont", _textPosition, Color.Black, FontAlign.Right);
                _actionPosition.Y += actionSize.Y;
                _textPosition.Y += actionSize.Y;
            }
            //Draw Grid Inside
            var size = Core.Metrics["KeyOptionGridInsideSize", 0];
            _gridInsideSpriteMap.Draw( _currentPlayer - 1, size.X, size.Y, Core.Metrics["KeyOptionGridInside", 0]);

            //Draw Border
            _gridBorderSprite.Draw();

            //Draw Bindings.
            _bindingPosition = Core.Metrics["KeyOptionGridBindings", 0].Clone();
            var bindingSize = Core.Metrics["KeyOptionGridBindings.Size", 0];
            foreach (ActionBinding ab in _actionBindings)
            {
                ab.Position = _bindingPosition;
                ab.Size = bindingSize;
                ab.Draw();
                _bindingPosition.Y += bindingSize.Y + 5;
            }
        }

        private void DrawText()
        {
            string instructionText = "";

            if (State.CurrentState == 3)
                instructionText += "Press the key to use for the selected action, or Escape to cancel.";
            else
            {
                instructionText += "START: Add binding. SELECT: Remove bindings. Press Escape when done.";
            }
            var scale = FontManager.ScaleTextToFit(instructionText, "LargeFont", GameCore.INTERNAL_WIDTH - 20, 100);
            FontManager.DrawString(instructionText, "LargeFont", Core.Metrics["KeyOptionInstructionText",0], scale, Color.White,FontAlign.Center);
        }

        #endregion

        #region Input

        public override void PerformKey(Keys key)
        {
            switch (State.CurrentState)
            {
                case 2:
                    State.CurrentState = 3;
                    break;
                case 3:
                    if (key == Keys.Escape)
                    {
                        State.CurrentState = 1;
                        RaiseSoundTriggered(SoundEvent.KEY_CHANGE_CANCEL);
                        return;
                    }

                    Core.KeyMappings.SetKey(key, _currentPlayer, _actions[_selectedAction]);
                    SaveMappingChanges();
                    break;
            }
        }

        public override void PerformButton(Buttons buttons, int controllerNumber)
        {
            switch (State.CurrentState)
            {
                case 2:
                    State.CurrentState = 3;
                    break;
                case 3:
                    Core.KeyMappings.SetButton(buttons, controllerNumber, _currentPlayer, _actions[_selectedAction]);
                    SaveMappingChanges();
                    break;
            }
        }

        private void SaveMappingChanges()
        {
            Core.KeyMappings.SaveToFile("Keys.conf");
            State.CurrentState = 1;
            CreateBindingList();
            RaiseSoundTriggered(SoundEvent.KEY_CHANGE_COMPLETE);
        }
 
        public override void PerformAction(InputAction inputAction)
        {
            if (State.CurrentState != 1)
            {
                return;
            }

            switch (inputAction.Action)
            {
                case "UP":
                    _selectedAction--;
                    if (_selectedAction < 0)
                        _selectedAction = _actions.Length - 1;

                    RaiseSoundTriggered(SoundEvent.MENU_SELECT_UP);
                    break;

                case "DOWN":
                    _selectedAction++;

                    if (_selectedAction >= _actions.Length)
                        _selectedAction = 0;

                    RaiseSoundTriggered(SoundEvent.MENU_SELECT_DOWN);
                    break;

                case "LEFT":
                    _currentPlayer--;

                    if (_currentPlayer < 1)
                        _currentPlayer = 4;

                    RaiseSoundTriggered(SoundEvent.MENU_OPTION_SELECT_LEFT);
                    break;

                case "RIGHT":
                    _currentPlayer++;

                    if (_currentPlayer > 4)
                        _currentPlayer = 1;

                    RaiseSoundTriggered(SoundEvent.MENU_OPTION_SELECT_RIGHT);
                    break;

                case "START":
                    StartPressed();
                    break;
                case "SELECT":
                    RemoveBindings();
                    break;
                case "BACK":
                    Core.ScreenTransition("MainMenu");
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;
            }
            CreateBindingList();

        }

        private void RemoveBindings()
        {
            foreach (ActionBinding ab in _actionBindings)
            {
                if (ab.ControllerNumber == 0)
                {
                    Core.KeyMappings.Unset(ab.Key);
                }
                else
                {
                    Core.KeyMappings.Unset(ab.Button, ab.ControllerNumber);
                }
            }
            CreateBindingList();
            Core.KeyMappings.SaveToFile("Keys.conf");
        }

        private void StartPressed()
        {
            if (_actions[_selectedAction] == "Reset Defaults")
            {
                Core.KeyMappings.LoadDefault();
                Core.KeyMappings.SaveToFile("Keys.conf");
                RaiseSoundTriggered(SoundEvent.KEY_CHANGE_COMPLETE);
            }
            else
            {
                State.CurrentState = 2;
                RaiseSoundTriggered(SoundEvent.KEY_CHANGE_START);
            }
        }

        #endregion

        #region Helpers
        private void CreateBindingList()
        {
            _actionBindings.Clear();
            if (_selectedAction >= _normalActionCount)
            {
                return;
            }

            var inputAction = new InputAction {Action = _actions[_selectedAction], Player = _currentPlayer};
            var keys = Core.KeyMappings.GetKeys(inputAction);

            foreach (Keys key in keys)
            {
                var newBinding = new ActionBinding { ControllerNumber = 0, Height = 45, Width = 230, Key = key };
                _actionBindings.Add(newBinding);
            }
            for (int num = 1; num < 5; num++)
            {
                var buttons = Core.KeyMappings.GetButtons(inputAction, num);
                foreach (Buttons button in buttons)
                {
                    var newBinding = new ActionBinding { ControllerNumber = num, Height = 45, Width = 230, Button = button };
                    _actionBindings.Add(newBinding);
                }
            }

        }
        #endregion

    }
}
