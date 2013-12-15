using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoundLineCode;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Screens
{
    public class NewGameScreen : GameScreen
    {
        private readonly SineSwayParticleField _field = new SineSwayParticleField();

        private readonly CursorPosition[] _cursorPositions = new CursorPosition[4];
        private readonly Menu[] _playerMenus = new Menu[4];
        private readonly Menu[] _profileMenus = new Menu[4];
        private readonly OnScreenKeyboard[] _keyboards = new OnScreenKeyboard[4];
        private Sprite3D _background;
        private Sprite3D _messageBackground;
        private readonly string[] _infoMessages = new string[4];

        private readonly Color[] _backgroundColors = {
                                                new Color(255, 128, 128), new Color(128, 128, 255),
                                                new Color(128, 255, 128), new Color(255, 255, 128)
                                            };

        private PlayerOptionsSet _playerOptionsSet;
        private List<RoundLine> _lineList;

        public NewGameScreen(GameCore core)
            : base(core)
        {
        }

        public override void Initialize()
        {
            for (int x = 0; x < Core.Players.Count(); x++)
            {
                _cursorPositions[x] = CursorPosition.NotJoined;
                Core.Players[x].Playing = false;
                Core.Players[x].Remote = false;
                Core.Players[x].PlayerOptions = new PlayerOptions();
                Core.Players[x].Profile = null;
                CreatePlayerMenu(x);
                CreateProfileMenu(x);
                Core.Players[x].Team = 0;
                _infoMessages[x] = "";

                _keyboards[x] = new OnScreenKeyboard
                                    {
                                        MaxLength = 10,
                                        Id = x,
                                        Position = (Core.Metrics["OnScreenKeyboard", x]),
                                        Width = 640
                                    };
                _keyboards[x].EntryCancelled += Keyboard_EntryCancelled;
                _keyboards[x].EntryComplete += Keyboard_EntryComplete;
            }

            _playerOptionsSet = new PlayerOptionsSet
                                    {
                                        Players = Core.Players, 
                                        Positions = Core.Metrics["NewGamePlayerOptionsFrames"], 
                                        DrawAttract = false, 
                                        Size = Core.Metrics["PlayerOptionsFrame.Size",0]
                                    };
            _playerOptionsSet.CreatePlayerOptionsFrames();

            //Join the player that pressed START to enter this screen automatically.
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
            _background = new Sprite3D
            {
               Size = Core.Metrics["ScreenBackground.Size",0],
               Position = Core.Metrics["ScreenBackground",0],
                Texture = TextureManager.Textures("AllBackground"),
            };
            _messageBackground = new Sprite3D
                                     {
                                         Texture = TextureManager.Textures("NewGameMessageBorder"),
                                         Size = Core.Metrics["NewGameMessageBorder.Size",0]
                                     };
        }

        private void CreateProfileMenu(int x)
        {
            _profileMenus[x] = new Menu
                                   {
                                       Width = Core.Metrics["NewGameMenu.Size",0].X,
                                       Position = (Core.Metrics["NewGameMenuStart", x]),
                                       MaxVisibleItems = (int) Core.Metrics["NewGameMenu.Size",0].Y,
                                       SelectedItemBackgroundColor = _backgroundColors[x]
                                   };

            foreach (Profile profile in Core.Profiles.GetAll())
            {
                _profileMenus[x].AddItem(new MenuItem { ItemText = profile.Name, ItemValue = profile });
            }

            _profileMenus[x].AddItem(new MenuItem { ItemText = "[Guest]" });
            _profileMenus[x].AddItem(new MenuItem { ItemText = "[Create New]" });
            _profileMenus[x].AddItem(new MenuItem {ItemText = "[Cancel]", IsCancel = true});

        }

        private void CreatePlayerMenu(int x)
        {
            _playerMenus[x] = new Menu
                                  {
                                      Width = Core.Metrics["NewGameMenu.Size", 0].X,
                                      Position = (Core.Metrics["NewGameMenuStart", x]),
                                      MaxVisibleItems = (int)Core.Metrics["NewGameMenu.Size", 0].Y,
                                      SelectedItemBackgroundColor = _backgroundColors[x]
                                  };
            _playerMenus[x].AddItem(new MenuItem { ItemText = "Decision" });
            _playerMenus[x].AddItem(new MenuItem { ItemText = "Profile" });

            var difficulty = new MenuItem { ItemText = "Difficulty", IsSelectable = false };
            difficulty.AddOption("Beginner", 0);
            difficulty.AddOption("Easy", 1);
            difficulty.AddOption("Medium", 2);
            difficulty.AddOption("Hard", 3);


            if (Core.Players[x].GetMaxDifficulty() >= 4)
            {
                difficulty.AddOption("Insane", 4);
            }
            if (Core.Players[x].GetMaxDifficulty() >= 5)
            {
                difficulty.AddOption("Ruthless", 5);
            }
            _playerMenus[x].AddItem(difficulty);

            var noteSpeed = new MenuItem { ItemText = "Beatline Speed", IsSelectable = false};
            noteSpeed.AddOption("0.5x", 0.5);
            noteSpeed.AddOption("1x", 1.0);
            noteSpeed.AddOption("1.5x", 1.5);
            noteSpeed.AddOption("2x", 2.0);
            noteSpeed.AddOption("3x", 3.0);
            noteSpeed.AddOption("4x", 4.0);
            noteSpeed.AddOption("6x", 6.0);
            noteSpeed.SetSelectedByValue(1.0);
            _playerMenus[x].AddItem(noteSpeed);

            var disableKO = new MenuItem {ItemText = "Disable KO", IsSelectable = false};
            disableKO.AddOption("Off",false);
            disableKO.AddOption("On",true);
            _playerMenus[x].AddItem(disableKO);

            var disableLb = new MenuItem { ItemText = "Disable Extra Life", IsSelectable = false };
            disableLb.AddOption("Off", false);
            disableLb.AddOption("On", true);

            _playerMenus[x].AddItem(disableLb);

            _playerMenus[x].AddItem(new MenuItem { ItemText = "Leave", IsCancel = true});
        }

        private void Keyboard_EntryComplete(object sender, EventArgs e)
        {
            var senderKeyboard = ((OnScreenKeyboard) sender);
            var player = senderKeyboard.Id;
            
            if (Core.Profiles[senderKeyboard.EnteredText] != null)
            {
                _infoMessages[player] = "This name is already in use.";
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
            ChangeCursorPosition(player,CursorPosition.MainMenu);
            RaiseSoundTriggered(SoundEvent.MENU_DECIDE);
            _infoMessages[player] = "";
 
        }

        private void Keyboard_EntryCancelled(object sender, EventArgs e)
        {
            var player = ((OnScreenKeyboard) sender).Id;
            _cursorPositions[player] = CursorPosition.ProfileList;
            RaiseSoundTriggered(SoundEvent.MENU_BACK);
            _infoMessages[player] = "";
        }

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground( gameTime);
            DrawBorders();
            DrawMenus();
            DrawMessages();
        }

        private void DrawBackground( GameTime gameTime)
        {

            _background.Draw();
            _field.Draw(gameTime);
            
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                _messageBackground.Position = (Core.Metrics["NewGameMessageBorder", x]);
                _messageBackground.Draw();
            }
            _playerOptionsSet.Draw();
        }

        private void DrawMessages()
        {
            for (int x = 0; x < 4; x++)
            {
                var textPosition = Core.Metrics["NewGameMessageBorder", x].Clone();
                textPosition.X += Core.Metrics["NewGameMessageBorder.Size",0].X / 2;
                textPosition.Y += 5;
                FontManager.DrawString(_infoMessages[x], "DefaultFont", textPosition, Color.White,
                                          FontAlign.Center);
            }

        }

        private void DrawMenus()
        {
            for (int x = 0; x < 4; x++)
            {

                switch (_cursorPositions[x])
                {
                    case CursorPosition.NotJoined:
                        FontManager.DrawString("Press START to Join...", "LargeFont",
                        Core.Metrics["NewGameJoinNotification", x], Color.Black, FontAlign.Left);
                        _infoMessages[x] = "";
                        break;
                    case CursorPosition.MainMenu:

                        _playerMenus[x].Draw();
                        break;
                    case CursorPosition.ProfileList:
                        _profileMenus[x].Draw();
                        break;
                    case CursorPosition.Keyboard:
                        _keyboards[x].Draw();
                        break;
                    case CursorPosition.Ready:
                        FontManager.DrawString("Ready", "LargeFont",
                        Core.Metrics["NewGameJoinNotification", x], Color.Black, FontAlign.Left);
                        _infoMessages[x] = "Waiting for other players...";
                        break;
                }
            }
        }

        private void DrawBorders()
        {

            if (_lineList == null)
            {
                _lineList = new List<RoundLine>();

                _lineList.Add(new RoundLine(GameCore.INTERNAL_WIDTH / 2, 0, GameCore.INTERNAL_WIDTH / 2, GameCore.INTERNAL_HEIGHT));
                _lineList.Add(new RoundLine(0, GameCore.INTERNAL_HEIGHT / 2, GameCore.INTERNAL_WIDTH, GameCore.INTERNAL_HEIGHT / 2));
            }
            RoundLineManager.Instance.Draw(_lineList, 1, Color.Black);
        }

#endregion

        #region Input handling

        public override void PerformAction(InputAction inputAction)
        {
            if (inputAction.Action == "BACK")
            {
                Core.ScreenTransition("MainMenu");
                RaiseSoundTriggered(SoundEvent.MENU_BACK);
                return;
            }

            
            var playerIdx = inputAction.Player - 1;

            if (playerIdx == -1)
            {
                return;
            }
            if (Core.Players[playerIdx].Remote)
            {
                return;
            }
            if (_cursorPositions[playerIdx] == CursorPosition.Keyboard)
            {
                _keyboards[playerIdx].MoveSelection(inputAction.Action);
                RaiseSoundTriggered(SoundEvent.KEYBOARD_MOVE);
                return;
            }

            Menu relevantMenu;

            switch (_cursorPositions[playerIdx])
            {
                    case CursorPosition.ProfileList:
                    relevantMenu = _profileMenus[playerIdx];
                    break;
                    case CursorPosition.MainMenu:
                    relevantMenu = _playerMenus[playerIdx];
                    break;
                default:
                    relevantMenu = null;
                    break;
            }
            switch (inputAction.Action)
            {
                case "START":
                    StartPressed(playerIdx);
                    break;
                case "UP":
                case "DOWN":
                case "RIGHT":
                case "LEFT":
                    if (relevantMenu != null)
                    {
                        relevantMenu.HandleAction(inputAction);
                    }
                    break;
       
            }
        }

        private void StartPressed(int number)
        {
            switch (_cursorPositions[number])
            {
                case CursorPosition.NotJoined:
                    ChangeCursorPosition(number, CursorPosition.ProfileList);
                    _infoMessages[number] = "Select a profile.";
                    Core.Players[number].Playing = true;
                    Core.Players[number].CPU = false;
                    if (!Core.Cookies.ContainsKey("JoiningPlayer"))
                    {
                        RaiseSoundTriggered(SoundEvent.PLAYER_JOIN);
                    }
                    break;
                case CursorPosition.MainMenu:
                    SelectMainMenuItem(number);
                    break;
                    case CursorPosition.ProfileList:
                    SelectProfileListItem(number);
                    break;
                case CursorPosition.Keyboard:
                    _keyboards[number].PickSelection();
                    break;
                case CursorPosition.Ready:
                    //Player is already ready.
                    return;
                    
            }

        }

        private void SelectProfileListItem(int number)
        {
    
            _playerMenus[number].SelectedIndex = 0;
            _profileMenus[number].ConfirmSelection();
           if (_profileMenus[number].SelectedItem().ItemValue == null)
           {
               switch (_profileMenus[number].SelectedItem().ItemText)
               {
                   case "[Create New]":
                       ChangeCursorPosition(number, CursorPosition.Keyboard);
                       _infoMessages[number] = "Enter a profile name.";
                       break;
                   case "[Guest]":
                       Core.Players[number].Profile = null;
                       Core.Players[number].PlayerOptions = new PlayerOptions();
                       RefereshSelectedOptions(number);
                       ChangeCursorPosition(number, CursorPosition.MainMenu);
                       _infoMessages[number] = "";
                       //NetHelper.Instance.BroadcastProfileChange(number);
                       //NetHelper.Instance.BroadcastPlayerOptions(number);
                       break;
                   case "[Cancel]":
                       ChangeCursorPosition(number, CursorPosition.MainMenu);
                       _infoMessages[number] = "";
                       break;
               }

               _playerMenus[number].GetByItemText("Disable Extra Life").Enabled = Core.Players[number].Profile != null;
               if (Core.Players[number].Profile == null)
               {
                    _playerMenus[number].GetByItemText("Disable Extra Life").SetSelectedByValue(false);
               }
           }
           else
           {
               var newSelection = (Profile) _profileMenus[number].SelectedItem().ItemValue;
               LoadProfile(number, newSelection);
           }
        }

        private void LoadProfile(int player, Profile profile)
        {
            bool okToChange = true;

            //Check if the profile is in use.
            for (int x = 0; x < 4; x++)
            {
                if (player == x || Core.Players[x].Profile != profile)
                {
                    continue;
                }

                okToChange = false;
                _infoMessages[player] = "This profile is already in use.";
            }
            if (!okToChange)
            {
                return;
            }
            Core.Players[player].Profile = profile;
            Core.Players[player].LoadPreferences();
            RefereshSelectedOptions(player);
            ChangeCursorPosition(player, CursorPosition.MainMenu);
            _infoMessages[player] = "";

            //NetHelper.Instance.BroadcastProfileChange(number);
            // NetHelper.Instance.BroadcastPlayerOptions(number);
        }

        private void ChangeCursorPosition(int player, CursorPosition position)
        {
            _cursorPositions[player] = position;
           // NetHelper.Instance.BroadcastCursorPosition(player, _cursorPositions[player]);

        }
        private void RefereshSelectedOptions(int number)
        {
            _playerMenus[number].GetByItemText("Difficulty").RemoveOption("Insane");
            if (Core.Players[number].GetMaxDifficulty() >= 4)
            {
                _playerMenus[number].GetByItemText("Difficulty").AddOption("Insane", 4);
            }

            _playerMenus[number].GetByItemText("Beatline Speed").SetSelectedByValue(Core.Players[number].PlayerOptions.BeatlineSpeed);
            _playerMenus[number].GetByItemText("Difficulty").SetSelectedByValue((int)Core.Players[number].PlayerOptions.PlayDifficulty);
            _playerMenus[number].GetByItemText("Disable KO").SetSelectedByValue(Core.Players[number].PlayerOptions.DisableKO);
            _playerMenus[number].GetByItemText("Disable Extra Life").SetSelectedByValue(Core.Players[number].PlayerOptions.DisableExtraLife);
        }

        private void SelectMainMenuItem(int number)
        {
            switch (_playerMenus[number].SelectedItem().ItemText)
            {
                case "Leave":
                    ChangeCursorPosition(number, CursorPosition.NotJoined);
                    Core.Players[number].Playing = false;
                    Core.Players[number].Profile = null;
                    TryToStart();
                    break;
                case "Decision":
                    ChangeCursorPosition(number,CursorPosition.Ready);
                    TryToStart();
                    break;
                case "Profile":
                    ChangeCursorPosition(number, CursorPosition.ProfileList);
                    _profileMenus[number].SelectedIndex = 0;
                    _infoMessages[number] = "Select a profile.";
                    break;
            }
            _playerMenus[number].ConfirmSelection();
        }

        private void TryToStart()
        {
            
            var noPlayers = _cursorPositions.Any(e => e != CursorPosition.NotJoined);
            if (noPlayers)
            {
                return;
            }

            bool everyoneReady = true;
            for (int x = 0; x < 4; x++)
            {
                everyoneReady = everyoneReady && (!(Core.Players[x].Playing ^ _cursorPositions[x] == CursorPosition.Ready));
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
                if (Core.Players[x].Remote)
                {
                    continue;
                }
                Core.Players[x].PlayerOptions.PlayDifficulty =
    (Difficulty)(int)_playerMenus[x].GetByItemText("Difficulty").SelectedValue();
                Core.Players[x].PlayerOptions.BeatlineSpeed = (double)_playerMenus[x].GetByItemText("Beatline Speed").SelectedValue();
                Core.Players[x].PlayerOptions.DisableKO = (bool)_playerMenus[x].GetByItemText("Disable KO").SelectedValue();
                Core.Players[x].PlayerOptions.DisableExtraLife =
                    (bool) _playerMenus[x].GetByItemText("Disable Extra Life").SelectedValue();

                Core.Players[x].UpdatePreferences();
            }
            Core.Profiles.SaveToFolder(Core.Settings["ProfileFolder"] + "");
        }


        private void StartGame()
        {
           // NetHelper.Instance.BroadcastScreenTransition("ModeSelect");
            Core.ScreenTransition("ModeSelect");
        }

        #endregion

        #region Netplay Code
        /*
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
                    RefereshSelectedOptions(message.PlayerID);
                    break;
                case MessageType.SCREEN_TRANSITION:
                    var screen = message.MessageData.ToString();
                    if (screen == "MainMenu")
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            Core.Players[x].Remote = false;
                        }
                    }
                    Core.ScreenTransition(screen);
                    break;
            }

        }
        */

        #endregion
    }

    enum CursorPosition
    {
        NotJoined,
        MainMenu,
        Ready,
        Keyboard,
        ProfileList
    }
}
