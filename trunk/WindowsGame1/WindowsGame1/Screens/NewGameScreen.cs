using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Notes;
using Action = WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class NewGameScreen : GameScreen
    {
        private SineSwayParticleField _field = new SineSwayParticleField();

        private readonly CursorPosition[] _cursorPositions = new CursorPosition[4];
        private readonly Menu[] _playerMenus = new Menu[4];
        private readonly OnScreenKeyboard[] _keyboards = new OnScreenKeyboard[4];
        public NewGameScreen(GameCore core)
            : base(core)
        {

        }

        public override void Initialize()
        {
            for (int x = 0; x < Core.Players.Count(); x++)
            {
                _cursorPositions[x] = CursorPosition.NOT_JOINED;
                Core.Players[x].Playing = false;

                _playerMenus[x] = new Menu();
                _playerMenus[x].AddItem(new MenuItem { ItemText = "Decision" });
                var difficulty = new MenuItem { ItemText = "Difficulty" };
                difficulty.AddOption("Beginner", 0);
                difficulty.AddOption("Easy", 1);
                difficulty.AddOption("Medium", 2);
                difficulty.AddOption("Hard", 3);
                difficulty.AddOption("Insane", 4);
                _playerMenus[x].AddItem(difficulty);

                var noteSpeed = new MenuItem { ItemText = "Beatline Speed" };
                noteSpeed.AddOption("0.5x", 0.5);
                noteSpeed.AddOption("1x", 1.0);
                noteSpeed.AddOption("1.5x", 1.5);
                noteSpeed.AddOption("2x", 2.0);
                noteSpeed.AddOption("3x", 3.0);
                noteSpeed.AddOption("4x", 4.0);
                noteSpeed.AddOption("6x", 6.0);
                noteSpeed.SetSelectedByValue(1.0);
                _playerMenus[x].AddItem(noteSpeed);

                _playerMenus[x].SetPosition(Core.Metrics["NewGameMenuStart", x]);

                _playerMenus[x].AddItem(new MenuItem { ItemText = "Name Entry" });

                _playerMenus[x].AddItem(new MenuItem { ItemText = "Leave" });
                Core.Players[x].Team = 0;
            }

            for (int x = 0; x < 4; x++)
            {
                _keyboards[x] = new OnScreenKeyboard {MaxLength = 8, Id = x};
                _keyboards[x].SetPosition(Core.Metrics["OnScreenKeyboard", x]);
                _keyboards[x].EnteredTextPosition = Core.Metrics["OnScreenKeyboardDisplay", x];
                _keyboards[x].EntryCancelled += Keyboard_EntryCancelled;
                _keyboards[x].EntryComplete += Keyboard_EntryComplete;
            }
            base.Initialize();
        }

        private void Keyboard_EntryComplete(object sender, EventArgs e)
        {
            var senderKeyboard = ((OnScreenKeyboard) sender);
            var player = senderKeyboard.Id;
            Core.Players[player].Name = senderKeyboard.EnteredText;
            _cursorPositions[player] = CursorPosition.MAIN_MENU;
        }

        private void Keyboard_EntryCancelled(object sender, EventArgs e)
        {
            var player = ((OnScreenKeyboard) sender).Id;
            _cursorPositions[player] = CursorPosition.MAIN_MENU;
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            var background = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["allBackground"],
                Width = Core.Window.ClientBounds.Width,
                X = 0,
                Y = 0
            };

            background.Draw(spriteBatch);
            _field.Draw(spriteBatch);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawBorders(spriteBatch);
            DrawMenus(spriteBatch);
        }

        private void DrawMenus(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 4; x++)
            {

                switch (_cursorPositions[x])
                {
                    case CursorPosition.NOT_JOINED:
                        TextureManager.DrawString(spriteBatch,"Press Start to Join...", "LargeFont", 
                        Core.Metrics["NewGameJoinNotification", x], Color.Black,FontAlign.LEFT);
                        break;
                    case CursorPosition.MAIN_MENU:
                        _playerMenus[x].Draw(spriteBatch);
                        break;
                    case CursorPosition.KEYBOARD:
                        _keyboards[x].Draw(spriteBatch);
                        break;
                    case CursorPosition.READY:
                        TextureManager.DrawString(spriteBatch, "Ready", "LargeFont",
                        Core.Metrics["NewGameJoinNotification", x], Color.Black, FontAlign.LEFT);
                        break;
                }
            }
        }

        private void DrawBorders(SpriteBatch spriteBatch)
        {
            var brush = new PrimitiveLine(Core.GraphicsDevice) { Colour = Color.Black };
            brush.AddVector(new Vector2(400, 0));
            brush.AddVector(new Vector2(400, 600));
            brush.Render(spriteBatch);
            brush.ClearVectors();
            brush.AddVector(new Vector2(0, 275));
            brush.AddVector(new Vector2(800, 275));
            brush.Render(spriteBatch);
            brush.ClearVectors();
            brush.AddVector(new Vector2(0, 325));
            brush.AddVector(new Vector2(800, 325));
            brush.Render(spriteBatch);
            brush.ClearVectors();
        }

        public override void PerformAction(Action action)
        {
            int player;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            switch (paction)
            {
                case "START":
                    StartPressed(player);
                    break;
                case "UP":
                    if (_cursorPositions[player] == CursorPosition.KEYBOARD)
                    {
                        _keyboards[player].MoveSelection(NoteDirection.UP);
                    }
                    else
                    {
                        _playerMenus[player].DecrementSelected();
                    }

                    break;
                case "DOWN":
                    if (_cursorPositions[player] == CursorPosition.KEYBOARD)
                    {
                        _keyboards[player].MoveSelection(NoteDirection.DOWN);
                    }
                    else
                    {
                        _playerMenus[player].IncrementSelected();
                    }
                    break;
                case "RIGHT":
                    if (_cursorPositions[player] == CursorPosition.KEYBOARD)
                    {
                        _keyboards[player].MoveSelection(NoteDirection.RIGHT);
                    }
                    else
                    {
                        _playerMenus[player].IncrementOption();
                    }
                    break;
                case "LEFT":
                    if (_cursorPositions[player] == CursorPosition.KEYBOARD)
                    {
                        _keyboards[player].MoveSelection(NoteDirection.LEFT);
                    }
                    else
                    {
                        _playerMenus[player].DecrementOption();
                    }
                    break;
                case "BACK":
                    Core.ScreenTransition("MainMenu");
                    break;
            }
        }

        private void StartPressed(int number)
        {
            switch (_cursorPositions[number])
            {
                case CursorPosition.NOT_JOINED:
                    _cursorPositions[number] = CursorPosition.MAIN_MENU;
                    Core.Players[number].Playing = true;
                    break;
                case CursorPosition.MAIN_MENU:
                    SelectMainMenuItem(number);
                    break;
                case CursorPosition.KEYBOARD:
                    _keyboards[number].PickSelection();
                    break;
                case CursorPosition.READY:
                    //Player is already ready.
                    return;
            }
        }

        private void SelectMainMenuItem(int number)
        {
            switch (_playerMenus[number].SelectedItem().ItemText)
            {
                case "Leave":
                    _cursorPositions[number] = CursorPosition.NOT_JOINED;
                    Core.Players[number].Playing = false;
                    TryToStart();
                    break;
                case "Decision":
                    _cursorPositions[number] = CursorPosition.READY;
                    TryToStart();
                    break;
                case "Name Entry":
                    _cursorPositions[number] = CursorPosition.KEYBOARD;
                    break;
            }
        }

        private void TryToStart()
        {
            bool noPlayers = true;
            for (int x = 0; x < 4; x++)
            {
                noPlayers = noPlayers && (_cursorPositions[x] == CursorPosition.NOT_JOINED);
            }
            if (noPlayers)
            {
                return;
            }

            bool everyoneReady = true;
            for (int x = 0; x < 4; x++)
            {
                everyoneReady = everyoneReady && (!(Core.Players[x].Playing ^ _cursorPositions[x] == CursorPosition.READY));
            }

            if (everyoneReady)
            {
                StartGame();
            }
        }


        private void StartGame()
        {
            for (int x = 0; x < 4; x++)
            {
                Core.Players[x].PlayDifficulty =
                    (Difficulty)(int)_playerMenus[x].GetByItemText("Difficulty").SelectedValue();
                Core.Players[x].BeatlineSpeed = (double)_playerMenus[x].GetByItemText("Beatline Speed").SelectedValue();
            }
            Core.ScreenTransition("ModeSelect");
        }
    }

    enum CursorPosition
    {
        NOT_JOINED,
        MAIN_MENU,
        READY,
        KEYBOARD,
    }
}
