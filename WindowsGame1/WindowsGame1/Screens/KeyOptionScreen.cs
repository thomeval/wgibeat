using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Microsoft.Xna.Framework.Input;
using Action = WGiBeat.Managers.Action;


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

        private Sprite _backgroundSprite;
        private Sprite _headerSprite;
        private SpriteMap _gridInsideSpriteMap;
        private SpriteMap _gridTopSpriteMap;
        private SpriteMap _gridSideSpriteMap;
        private Sprite _gridBorderSprite;
        private Sprite _instructionBaseSprite;

        private readonly List<ActionBinding> _actionBindings = new List<ActionBinding>();

        private readonly string[] _actions = { "LEFT", "RIGHT", "UP", "DOWN", "BEATLINE", "START", "SELECT", "Reset Defaults" };
        private const int _normalActionCount = 7;

        private SineSwayParticleField _field = new SineSwayParticleField();
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

            _backgroundSprite = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["AllBackground"],
                Width = Core.Window.ClientBounds.Width,
            };

            _headerSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures["keyOptionHeader"],
            };

            _gridTopSpriteMap = new SpriteMap
                                    {
                                        Columns = 4,
                                        Rows = 2,
                                        SpriteTexture = TextureManager.Textures["KeyOptionGridTop"]
                                    };
            _gridSideSpriteMap = new SpriteMap
                                     {
                                         Columns = 4,
                                         Rows = 2,
                                         SpriteTexture = TextureManager.Textures["KeyOptionGridSide"]
                                     };
            _gridInsideSpriteMap = new SpriteMap
                                       {
                                           Columns = 1,
                                           Rows = 4,
                                           SpriteTexture = TextureManager.Textures["KeyOptionGridInside"],
                                           TrimX = 1,
                                           TrimY = 1
                                       };
            _gridBorderSprite = new Sprite
                                    {
                                        SpriteTexture = TextureManager.Textures["KeyOptionGridBorder"],
                                        Position = Core.Metrics["KeyOptionGridBorder",0]
                                    };
            _instructionBaseSprite = new Sprite
                                         {
                                             SpriteTexture = TextureManager.Textures["KeyOptionInstructionBase"],
                                             Position = Core.Metrics["KeyOptionInstructionBase",0]
                                         };
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawOverlay(spriteBatch);
            DrawText(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            _backgroundSprite.Draw(spriteBatch);
            _field.Draw(spriteBatch);
            _instructionBaseSprite.Draw(spriteBatch);
        }

        private Vector2 _actionPosition;
        private Vector2 _textPosition;
        private void DrawOverlay(SpriteBatch spriteBatch)
        {
            _headerSprite.Draw(spriteBatch);

            //Draw Grid top
            for (int x = 0; x < 4; x++)
            {
                var idx = x;
                if (x != (_currentPlayer - 1))
                {
                    idx += 4;
                }
                _gridTopSpriteMap.Draw(spriteBatch, idx, Core.Metrics["KeyOptionGridTop", x]);
            }

            //Draw Grid Side

            _actionPosition = Core.Metrics["KeyOptionGridSide", 0].Clone();
            _textPosition = Core.Metrics["KeyOptionGridSideText", 0].Clone();
            for (int x = 0; x < _actions.Length; x++)
            {
                var idx = _currentPlayer - 1;
                if (x != _selectedAction)
                {
                    idx += 4;
                }
                _gridSideSpriteMap.Draw(spriteBatch, idx, _actionPosition);
                TextureManager.DrawString(spriteBatch, _actions[x], "LargeFont", _textPosition, Color.Black, FontAlign.RIGHT);
                _actionPosition.Y += 40;
                _textPosition.Y += 40;
            }
            //Draw Grid Inside
            var size = Core.Metrics["KeyOptionGridInsideSize", 0];
            _gridInsideSpriteMap.Draw(spriteBatch, _currentPlayer - 1, (int)size.X, (int)size.Y, Core.Metrics["KeyOptionGridInside", 0]);

            //Draw Border
            _gridBorderSprite.Draw(spriteBatch);

            //Draw Bindings.
            _bindingPosition = Core.Metrics["KeyOptionGridBindings", 0].Clone();
            foreach (ActionBinding ab in _actionBindings)
            {
                ab.Position = _bindingPosition;
                ab.Draw(spriteBatch);
                _bindingPosition.Y += 50;
            }
        }

        private void DrawText(SpriteBatch spriteBatch)
        {
            string instructionText = "";

            if (State.CurrentState == 3)
                instructionText += "Press the key to use for the selected action, or Escape to cancel.";
            else
            {
                instructionText += "START: Add binding. SELECT: Remove bindings. Press Escape when done.";
            }
            var scale = TextureManager.ScaleTextToFit(instructionText, "LargeFont", 780, 100);
            TextureManager.DrawString(spriteBatch,instructionText, "LargeFont", Core.Metrics["KeyOptionInstructionText",0], scale, Color.White,FontAlign.CENTER);
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
                        return;
                    }
                    var actionStr = String.Format("P{0}_{1}", _currentPlayer, (_actions[_selectedAction]));
                    var convertedAction = (Action)Enum.Parse(typeof(Action), actionStr);

                    Core.KeyMappings.SetKey(key, convertedAction);
                    Core.KeyMappings.SaveToFile("Keys.conf");
                    State.CurrentState = 1;
                    CreateBindingList();
                    break;
            }
        }

        public override void PerformButton(Buttons buttons, int playerIndex)
        {
            switch (State.CurrentState)
            {
                case 2:
                    State.CurrentState = 3;
                    break;
                case 3:

                    var actionStr = String.Format("P{0}_{1}", _currentPlayer, (_actions[_selectedAction]));
                    var convertedAction = (Action)Enum.Parse(typeof(Action), actionStr);

                    Core.KeyMappings.SetButton(buttons, playerIndex, convertedAction);
                    Core.KeyMappings.SaveToFile("Keys.conf");
                    State.CurrentState = 1;
                    CreateBindingList();
                    break;
            }
        }

        public override void PerformAction(Action action)
        {
            if (State.CurrentState != 1)
            {
                return;
            }
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            switch (paction)
            {
                case "UP":
                    _selectedAction--;

                    if (_selectedAction < 0)
                        _selectedAction = _actions.Length - 1;

                    break;

                case "DOWN":
                    _selectedAction++;

                    if (_selectedAction >= _actions.Length)
                        _selectedAction = 0;

                    break;

                case "LEFT":
                    _currentPlayer--;

                    if (_currentPlayer < 1)
                        _currentPlayer = 4;
                    break;

                case "RIGHT":
                    _currentPlayer++;

                    if (_currentPlayer > 4)
                        _currentPlayer = 1;
                    break;

                case "START":
                    StartPressed();
                    break;
                case "SELECT":
                    RemoveBindings();
                    break;
                case "BACK":
                    Core.ScreenTransition("MainMenu");
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
            }
            else
            {
                State.CurrentState = 2;
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

            var actionStr = String.Format("P{0}_{1}", _currentPlayer, (_actions[_selectedAction]));
            var convertedAction = (Action)Enum.Parse(typeof(Action), actionStr);
            var keys = Core.KeyMappings.GetKeys(convertedAction);

            foreach (Keys key in keys)
            {
                var newBinding = new ActionBinding { ControllerNumber = 0, Height = 45, Width = 230, Key = key };
                _actionBindings.Add(newBinding);
            }
            for (int num = 1; num < 5; num++)
            {
                var buttons = Core.KeyMappings.GetButtons(convertedAction, num);
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
