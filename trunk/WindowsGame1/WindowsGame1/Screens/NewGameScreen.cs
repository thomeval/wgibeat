using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.NetSystem;
using WGiBeat.Players;

namespace WGiBeat.Screens
{
    public class NewGameScreen : GameScreen
    {
        private SineSwayParticleField _field = new SineSwayParticleField();

        private readonly CursorPosition[] _cursorPositions = new CursorPosition[4];
        private readonly Menu[] _playerMenus = new Menu[4];
        private readonly Menu[] _profileMenus = new Menu[4];
        private readonly OnScreenKeyboard[] _keyboards = new OnScreenKeyboard[4];
        private Sprite _background;
        private Sprite _messageBackground;
        private readonly string[] _errorMessages = new string[4];
        private readonly string[] _infoMessages = new string[4];

        private readonly Color[] _backgroundColors = {
                                                new Color(255, 128, 128), new Color(128, 128, 255),
                                                new Color(128, 255, 128), new Color(255, 255, 128)
                                            };
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
                Core.Players[x].Remote = false;
                Core.Players[x].Profile = null;
                CreatePlayerMenu(x);
                CreateProfileMenu(x);
                Core.Players[x].Team = 0;
                _errorMessages[x] = "";
                _infoMessages[x] = "";

                _keyboards[x] = new OnScreenKeyboard
                                    {
                                        MaxLength = 10,
                                        Id = x,
                                        Position = (Core.Metrics["OnScreenKeyboard", x]),
                                        Width = 400
                                    };
                _keyboards[x].EntryCancelled += Keyboard_EntryCancelled;
                _keyboards[x].EntryComplete += Keyboard_EntryComplete;
            }

            if (Core.Cookies.ContainsKey("JoiningPlayer"))
            {
                var player = (int) Core.Cookies["JoiningPlayer"];
                StartPressed(player);
                Core.Cookies.Remove("JoiningPlayer");
            }
            InitSprites();
            base.Initialize();
        }

        private void InitSprites()
        {
            _background = new Sprite
            {
                Height = 600,
                Width = 800,
                SpriteTexture = TextureManager.Textures("AllBackground"),
            };
            _messageBackground = new Sprite
                                     {
                                         SpriteTexture = TextureManager.Textures("NewGameMessageBorder")
                                     };
        }

        private void CreateProfileMenu(int x)
        {
            _profileMenus[x] = new Menu {Width = 300, Position = (Core.Metrics["NewGameMenuStart", x]), MaxVisibleItems = 6};
            _profileMenus[x].AddItem(new MenuItem {ItemText = "[Create New]"});
            _profileMenus[x].AddItem(new MenuItem { ItemText = "[Guest]" });
            _profileMenus[x].AddItem(new MenuItem {ItemText = "[Cancel]"});

            _profileMenus[x].SelectedItemBackgroundColor = _backgroundColors[x];
            foreach (Profile profile in Core.Profiles.GetAll())
            {
                _profileMenus[x].AddItem(new MenuItem {ItemText = profile.Name, ItemValue = profile});
            }

        }

        private void CreatePlayerMenu(int x)
        {
            _playerMenus[x] = new Menu { Width = 300 };
            _playerMenus[x].AddItem(new MenuItem { ItemText = "Decision" });
            _playerMenus[x].AddItem(new MenuItem { ItemText = "Profile" });
            _playerMenus[x].SelectedItemBackgroundColor = _backgroundColors[x];
            var difficulty = new MenuItem { ItemText = "Difficulty" };
            difficulty.AddOption("Beginner", 0);
            difficulty.AddOption("Easy", 1);
            difficulty.AddOption("Medium", 2);
            difficulty.AddOption("Hard", 3);

            if (Core.Players[x].GetMaxDifficulty() >= 4)
            {
                difficulty.AddOption("Insane", 4);
            }
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

            var disableKO = new MenuItem {ItemText = "Disable KO"};
            disableKO.AddOption("Off",false);
            disableKO.AddOption("On",true);
            _playerMenus[x].AddItem(disableKO);

            _playerMenus[x].Position = (Core.Metrics["NewGameMenuStart", x]);
            _playerMenus[x].MaxVisibleItems = 6;

            _playerMenus[x].AddItem(new MenuItem { ItemText = "Leave" });
        }

        private void Keyboard_EntryComplete(object sender, EventArgs e)
        {
            var senderKeyboard = ((OnScreenKeyboard) sender);
            var player = senderKeyboard.Id;
            
            if (Core.Profiles[senderKeyboard.EnteredText] != null)
            {
                _errorMessages[player] = "This name is already in use.";
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
            _errorMessages[player] = "";
        }

        private void Keyboard_EntryCancelled(object sender, EventArgs e)
        {
            var player = ((OnScreenKeyboard) sender).Id;
            _cursorPositions[player] = CursorPosition.PROFILE_LIST;
        }

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawBorders(spriteBatch);
            DrawMenus(spriteBatch);
            DrawMessages(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {

            _background.Draw(spriteBatch);
            _field.Draw(spriteBatch);

            for (int x = 0; x < 4; x++)
            {
                if (Core.Players[x].Playing)
                {
                    _messageBackground.Position = (Core.Metrics["NewGameMessageBorder", x]);
                    _messageBackground.Draw(spriteBatch);
                }
            }
        }

        private void DrawMessages(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 4; x++)
            {
                var textPosition = Core.Metrics["NewGameMessageBorder", x].Clone();
                textPosition.X += 200;
                textPosition.Y += 5;
                TextureManager.DrawString(spriteBatch, _infoMessages[x], "DefaultFont", textPosition, Color.White,
                                          FontAlign.CENTER);
                textPosition.Y += 25;
                TextureManager.DrawString(spriteBatch,_errorMessages[x],"DefaultFont", textPosition, Color.Yellow, FontAlign.CENTER);
            }

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
                        _infoMessages[x] = "";
                        break;
                    case CursorPosition.MAIN_MENU:

                        _playerMenus[x].Draw(spriteBatch);
                        _infoMessages[x] = (Core.Players[x].Profile == null) ? "No profile" : "Current profile: " + Core.Players[x].SafeName;
                        break;
                    case CursorPosition.PROFILE_LIST:
                        if (Core.Players[x].Remote)
                        {
                            _infoMessages[x] = "Selecting a profile";
                        }
                        else
                        {
                            _profileMenus[x].Draw(spriteBatch);
                            _infoMessages[x] = "Select a profile.";
                        }
                        break;
                    case CursorPosition.KEYBOARD:
                        if (Core.Players[x].Remote)
                        {
                            _infoMessages[x] = "Creating a new profile";
                        }
                        else
                        {                         
                        _keyboards[x].Draw(spriteBatch);
                            _infoMessages[x] = "Enter a profile name.";
                        }
                        break;
                    case CursorPosition.READY:
                        TextureManager.DrawString(spriteBatch, "Ready", "LargeFont",
                        Core.Metrics["NewGameJoinNotification", x], Color.Black, FontAlign.LEFT);
                        _infoMessages[x] = "Waiting for other players...";
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

#endregion

        #region Input handling

        public override void PerformAction(InputAction inputAction)
        {
            var playerIdx = inputAction.Player - 1;
            if ((playerIdx > -1) && Core.Players[playerIdx].Remote)
            {
                return;
            }
            if ((playerIdx > -1) && (_cursorPositions[playerIdx] == CursorPosition.KEYBOARD))
            {
                _keyboards[playerIdx].MoveSelection(inputAction.Action);
                return;
            }

            switch (inputAction.Action)
            {
                case "START":
                    StartPressed(playerIdx);
                    break;
                case "UP":
                        _playerMenus[playerIdx].DecrementSelected();
                        _profileMenus[playerIdx].DecrementSelected();
                    break;
                case "DOWN":
                        _playerMenus[playerIdx].IncrementSelected();
                        _profileMenus[playerIdx].IncrementSelected();
                    break;
                case "RIGHT":
                        _playerMenus[playerIdx].IncrementOption();
                    break;
                case "LEFT":
                        _playerMenus[playerIdx].DecrementOption();
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
                    ChangeCursorPosition(number, CursorPosition.PROFILE_LIST);
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
            _playerMenus[number].SelectedIndex = 0;
           if (_profileMenus[number].SelectedItem().ItemValue == null)
           {
               switch (_profileMenus[number].SelectedItem().ItemText)
               {
                   case "[Create New]":
                       ChangeCursorPosition(number, CursorPosition.KEYBOARD);
                       break;
                   case "[Guest]":
                       Core.Players[number].Profile = null;
                       ChangeCursorPosition(number, CursorPosition.MAIN_MENU);
                       NetHelper.Instance.BroadcastProfileChange(number);
                       break;
                   case "[Cancel]":
                       ChangeCursorPosition(number, CursorPosition.MAIN_MENU);
                       break;
               }
               _errorMessages[number] = "";
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
                       _errorMessages[number] = "This profile is already in use.";
                   }
               }
               if (okToChange)
               {
                   Core.Players[number].Profile = newSelection;
                   Core.Players[number].LoadPreferences();
                   RefereshSelectedOptions(number);
                   ChangeCursorPosition(number,CursorPosition.MAIN_MENU);
                   _errorMessages[number] = "";
                   NetHelper.Instance.BroadcastProfileChange(number);
               }
           }

        }

        private void ChangeCursorPosition(int player, CursorPosition position)
        {
            _cursorPositions[player] = position;
            NetHelper.Instance.BroadcastCursorPosition(player, _cursorPositions[player]);

        }
        private void RefereshSelectedOptions(int number)
        {
            _playerMenus[number].GetByItemText("Beatline Speed").SetSelectedByValue(Core.Players[number].PlayerOptions.BeatlineSpeed);
            _playerMenus[number].GetByItemText("Difficulty").SetSelectedByValue((int)Core.Players[number].PlayerOptions.PlayDifficulty);
            _playerMenus[number].GetByItemText("Disable KO").SetSelectedByValue(Core.Players[number].PlayerOptions.DisableKO);
        }

        private void SelectMainMenuItem(int number)
        {
            switch (_playerMenus[number].SelectedItem().ItemText)
            {
                case "Leave":
                    ChangeCursorPosition(number, CursorPosition.NOT_JOINED);
                    Core.Players[number].Playing = false;
                    Core.Players[number].Profile = null;
                    TryToStart();
                    break;
                case "Decision":
                    ChangeCursorPosition(number,CursorPosition.READY);
                    TryToStart();
                    break;
                case "Profile":
                    ChangeCursorPosition(number, CursorPosition.PROFILE_LIST);
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
                Core.Players[x].PlayerOptions.PlayDifficulty =
    (Difficulty)(int)_playerMenus[x].GetByItemText("Difficulty").SelectedValue();
                Core.Players[x].PlayerOptions.BeatlineSpeed = (double)_playerMenus[x].GetByItemText("Beatline Speed").SelectedValue();
                Core.Players[x].PlayerOptions.DisableKO = (bool)_playerMenus[x].GetByItemText("Disable KO").SelectedValue();
                Core.Players[x].UpdatePreferences();
            }
            Core.Profiles.SaveToFolder(Core.Settings["ProfileFolder"] + "");
        }


        private void StartGame()
        {       
            Core.ScreenTransition("ModeSelect");
        }

        #endregion

        #region Netplay Code

        public override void NetMessageReceived(NetMessage message)
        {
            switch (message.MessageType)
            {
                case MessageType.CURSOR_POSITION:
                    var position = (CursorPosition)message.MessageData;

                    _cursorPositions[message.PlayerID] = position;
                    if (_cursorPositions[message.PlayerID] == CursorPosition.PROFILE_LIST)
                    {
                        Core.Players[message.PlayerID].Remote = true;
                        Core.Players[message.PlayerID].Playing = true;
                    }
                    if (_cursorPositions[message.PlayerID] == CursorPosition.NOT_JOINED)
                    {
                        Core.Players[message.PlayerID].Remote = false;
                        Core.Players[message.PlayerID].Playing = false;
                    }
                    break;
                case MessageType.PLAYER_PROFILE:
                    if (!Core.Players[message.PlayerID].Remote)
                    {
                        return;
                    }
                    var profile = (Profile)message.MessageData;
                    Core.Players[message.PlayerID].Profile = profile;
                    break;
                case MessageType.PLAYER_NO_PROFILE:
                    if (!Core.Players[message.PlayerID].Remote)
                    {
                        return;
                    }
                    Core.Players[message.PlayerID].Profile = null;
                    break;
                case MessageType.PLAYER_OPTIONS:
                    Core.Players[message.PlayerID].PlayerOptions = ((PlayerOptions)message.MessageData);
                    break;
            }
            base.NetMessageReceived(message);
        }


        #endregion
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
