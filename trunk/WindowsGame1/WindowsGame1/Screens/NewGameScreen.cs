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
        private readonly Menu[] _profileMenus = new Menu[4];
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
                Core.Players[x].Profile = null;
                CreatePlayerMenu(x);
                CreateProfileMenu(x);
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

        private void CreateProfileMenu(int x)
        {
            _profileMenus[x] = new Menu();
            _profileMenus[x].SetPosition(Core.Metrics["NewGameMenuStart", x]);
            _profileMenus[x].AddItem(new MenuItem() {ItemText = "[Create New]"});
            _profileMenus[x].AddItem(new MenuItem() { ItemText = "[Guest]" });
            _profileMenus[x].AddItem(new MenuItem {ItemText = "[Cancel]"});

            foreach (Profile profile in Core.Profiles.GetAll())
            {
                _profileMenus[x].AddItem(new MenuItem() {ItemText = profile.Name, ItemValue = profile});
            }

            _profileMenus[x].MaxVisibleItems = 8;
        }

        private void CreatePlayerMenu(int x)
        {
            _playerMenus[x] = new Menu();
            _playerMenus[x].AddItem(new MenuItem { ItemText = "Decision" });
            _playerMenus[x].AddItem(new MenuItem { ItemText = "Profile" });

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

            var awesomeness = new MenuItem {ItemText = "Awesomeness"};
            awesomeness.AddOption("Off",false);
            awesomeness.AddOption("On",true);
            _playerMenus[x].AddItem(awesomeness);

            _playerMenus[x].SetPosition(Core.Metrics["NewGameMenuStart", x]);
            _playerMenus[x].MaxVisibleItems = 8;

            _playerMenus[x].AddItem(new MenuItem { ItemText = "Leave" });
        }

        private void Keyboard_EntryComplete(object sender, EventArgs e)
        {
            var senderKeyboard = ((OnScreenKeyboard) sender);
            var player = senderKeyboard.Id;
            
            if (Core.Profiles[senderKeyboard.EnteredText] != null)
            {
                //TODO: Display message.
                return;
            }
            var newProfile = new Profile {Name = senderKeyboard.EnteredText};
            Core.Profiles.Add(newProfile);

            Core.Players[player].Profile = newProfile;

            for (int x = 0; x < 4; x++  )
            {
                CreateProfileMenu(x);
            }
            Core.Profiles.SaveToFolder(Core.Settings["ProfileFolder"] + "");
            _cursorPositions[player] = CursorPosition.MAIN_MENU;
        }

        private void Keyboard_EntryCancelled(object sender, EventArgs e)
        {
            var player = ((OnScreenKeyboard) sender).Id;
            _cursorPositions[player] = CursorPosition.PROFILE_LIST;
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
                    case CursorPosition.PROFILE_LIST:
                        _profileMenus[x].Draw(spriteBatch);
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
                        _profileMenus[player].DecrementSelected();
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
                        _profileMenus[player].IncrementSelected();
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
                    _cursorPositions[number] = CursorPosition.PROFILE_LIST;
                    Core.Players[number].Playing = true;
                    Core.Players[number].CPU = false;
                    break;
                case CursorPosition.MAIN_MENU:
                    SelectMainMenuItem(number);
                    break;
                    case CursorPosition.PROFILE_LIST:
                    SelectProfileListItem(number);
                    break;
                case CursorPosition.KEYBOARD:
                    _keyboards[number].PickSelection();
                    break;
                case CursorPosition.READY:
                    //Player is already ready.
                    return;
            }
        }

        private void SelectProfileListItem(int number)
        {
          
           if (_profileMenus[number].SelectedItem().ItemValue == null)
           {
               switch (_profileMenus[number].SelectedItem().ItemText)
               {
                   case "[Create New]":
                       _cursorPositions[number] = CursorPosition.KEYBOARD;
                       break;
                   case "[Guest]":
                       Core.Players[number].Profile = null;
                       _cursorPositions[number] = CursorPosition.MAIN_MENU;
                       break;
                   case "[Cancel]":
                       _cursorPositions[number] = CursorPosition.MAIN_MENU;
                       break;
               }
           }
           else
           {
               var newSelection = (Profile) _profileMenus[number].SelectedItem().ItemValue;
               bool okToChange = true;
               for (int x = 0; x < 4; x++ )
               {
                   if (number == x)
                   {
                       continue;
                   }
                   if (Core.Players[x].Profile == newSelection)
                   {
                       okToChange = false;
                       //TODO: Display Message
                   }
               }
               if (okToChange)
               {
                   Core.Players[number].Profile = newSelection;
                   Core.Players[number].LoadPreferences();
                   RefereshSelectedOptions(number);
                   _cursorPositions[number] = CursorPosition.MAIN_MENU;
               }
           }
        }

        private void RefereshSelectedOptions(int number)
        {
            _playerMenus[number].GetByItemText("Beatline Speed").SetSelectedByValue(Core.Players[number].BeatlineSpeed);
            _playerMenus[number].GetByItemText("Difficulty").SetSelectedByValue((int) Core.Players[number].PlayDifficulty);
        }

        private void SelectMainMenuItem(int number)
        {
            switch (_playerMenus[number].SelectedItem().ItemText)
            {
                case "Leave":
                    _cursorPositions[number] = CursorPosition.NOT_JOINED;
                    Core.Players[number].Playing = false;
                    Core.Players[number].Profile = null;
                    TryToStart();
                    break;
                case "Decision":
                    _cursorPositions[number] = CursorPosition.READY;
                    TryToStart();
                    break;
                case "Profile":
                    _cursorPositions[number] = CursorPosition.PROFILE_LIST;
                    _profileMenus[number].SelectedIndex = 0;
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
                SavePreferencesToProfiles();
                StartGame();
            }
        }

        private void SavePreferencesToProfiles()
        {
            for (int x = 0; x < 4; x++)
            {
                Core.Players[x].PlayDifficulty =
    (Difficulty)(int)_playerMenus[x].GetByItemText("Difficulty").SelectedValue();
                Core.Players[x].BeatlineSpeed = (double)_playerMenus[x].GetByItemText("Beatline Speed").SelectedValue();
                Core.Players[x].UpdatePreferences();
            }
            Core.Profiles.SaveToFolder(Core.Settings["ProfileFolder"] + "");
        }


        private void StartGame()
        {       
            Core.ScreenTransition("ModeSelect");
        }
    }

    enum CursorPosition
    {
        NOT_JOINED,
        MAIN_MENU,
        READY,
        KEYBOARD,
        PROFILE_LIST
    }
}
