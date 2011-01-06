using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Drawing.Sets;
using WGiBeat.Notes;
using Action = WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class SongEditorScreen : GameScreen
    {

        private readonly Dictionary<string, Menu> _menus = new Dictionary<string, Menu>();
        private readonly FileSelectDialog _fileSelect = new FileSelectDialog();
        private readonly TextEntry _textEntry = new TextEntry { Width = 800, Height = 600 };
        private BeatlineSet _beatlineSet;
        private NoteJudgementSet _noteJudgementSet;
        private bool _keyInput;
        private bool _ignoreNextKey;
        private EditorCursorPosition _cursorPosition;

        private string _audioFilePath = "";
        private string _destinationFileName = "";
        private string _destinationFolderName = "";
        private TimeSpan? _startTime;

        private Sprite _backgroundSprite;

        private string _activeMenu;
        private int _editProgress;
        public GameSong NewGameSong;
        private string _textEntryDestination;
        private Sprite _editProgressBaseSprite;
        private SpriteMap _editProgressSpriteMap;
        private BpmMeter _bpmMeter;
        private double _phraseNumber;
        private double _debugLastHitOffset;

        public SongEditorScreen(GameCore core)
            : base(core)
        {
            CreateMenus();
            InitSprites();
            InitObjects();

        }

        private void InitObjects()
        {
            _bpmMeter = new BpmMeter {Position = (Core.Metrics["EditorBPMMeter", 0])};
            _beatlineSet = new BeatlineSet(Core.Metrics, Core.Players, GameType.NORMAL) {Large = true};
            _noteJudgementSet = new NoteJudgementSet(Core.Metrics,Core.Players,GameType.NORMAL);
        }

        public void InitSprites()
        {
            _backgroundSprite = new Sprite { SpriteTexture = TextureManager.Textures["allBackground"] };
            _editProgressBaseSprite = new Sprite { SpriteTexture = TextureManager.Textures["SongEditProgressBase"] };
            _editProgressBaseSprite.Position = (Core.Metrics["SongEditProgress", 0]);
            _editProgressSpriteMap = new SpriteMap
                                         {
                                             Columns = 4,
                                             Rows = 2,
                                             SpriteTexture = TextureManager.Textures["SongEditProgress"]
                                         };
        }

        private void CreateMenus()
        {
            var mainMenu = new Menu { Width = 800, Position = Core.Metrics["EditorMenuStart", 0] };
            mainMenu.AddItem(new MenuItem { ItemText = "Create New Song", ItemValue = 0 });
            mainMenu.AddItem(new MenuItem { ItemText = "Edit Existing Song", ItemValue = 1 });
            mainMenu.AddItem(new MenuItem { ItemText = "Exit", ItemValue = 2 });
            _menus.Add("Main", mainMenu);

            var basicsMenu = new Menu { Width = 800, Position = Core.Metrics["EditorMenuStart", 0] };
            basicsMenu.AddItem(new MenuItem { ItemText = "Select Audio File", ItemValue = 0 });
            basicsMenu.AddItem(new MenuItem { ItemText = "Enter Folder Name", ItemValue = 1 });
            basicsMenu.AddItem(new MenuItem { ItemText = "Enter Destination File Name", ItemValue = 2 });
            basicsMenu.AddItem(new MenuItem { ItemText = "Next Step", ItemValue = 3, Enabled = false });
            basicsMenu.AddItem(new MenuItem { ItemText = "Back", ItemValue = 4 });
            _menus.Add("Basics", basicsMenu);

            var detailsMenu = new Menu() { Width = 400, Position = Core.Metrics["EditorMenuStart", 0] };
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Title", ItemValue = 0 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Subtitle", ItemValue = 1 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Artist", ItemValue = 2 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter BPM Manually", ItemValue = 3 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure BPM", ItemValue = 4 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Offset Manually", ItemValue = 5 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure Offset", ItemValue = 6 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Length Manually", ItemValue = 7 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure Length", ItemValue = 8 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Next Step", ItemValue = 9, Enabled = false });
            detailsMenu.AddItem(new MenuItem { ItemText = "Back", ItemValue = 10 });
            _menus.Add("Details", detailsMenu);

            //TODO: Move this.
            var doneMenu = new Menu {Width = 400, Position = Core.Metrics["EditorMenuStart", 0]};
            doneMenu.AddItem(new MenuItem{ItemText = "OK", ItemValue = 0});
            _menus.Add("Done",doneMenu);

            _fileSelect.Width = 800;
            _fileSelect.Position = Core.Metrics["EditorMenuStart", 0];
            _fileSelect.CurrentFolder = Directory.GetCurrentDirectory();

            _textEntry.EntryComplete += TextEntryEntryComplete;
            _textEntry.EntryCancelled += TextEntryEntryCancelled;

           
        }



        private void TextEntryEntryCancelled(object sender, EventArgs e)
        {
            switch (_textEntryDestination)
            {
                case "SongFile":
                case "SongFolder":
                    _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    break;
                case "SongTitle":
                case "SongSubtitle":
                case "SongArtist":
                case "SongOffset":
                case "SongBPM":
                case "SongLength":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    break;
            }
            _keyInput = false;
        }

        private void TextEntryEntryComplete(object sender, EventArgs e)
        {
            //TODO: Check for invalid file / folder characters.
            double temp;
            switch (_textEntryDestination)
            {
                case "SongFile":
                    _destinationFileName = _textEntry.EnteredText;
                    if (!_destinationFileName.EndsWith(".sng"))
                    {
                        _destinationFileName += ".sng";
                    }
                    _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    break;
                case "SongFolder":
                    _destinationFolderName = _textEntry.EnteredText;
                    _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    break;
                case "SongTitle":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    NewGameSong.Title = _textEntry.EnteredText;
                    break;
                case "SongSubtitle":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    NewGameSong.Subtitle = _textEntry.EnteredText;
                    break;
                case "SongArtist":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    NewGameSong.Artist = _textEntry.EnteredText;
                    break;
                case "SongOffset":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    if (Double.TryParse(_textEntry.EnteredText, out temp))
                    {
                        NewGameSong.Offset = temp;
                    }
                    break;
                case "SongBPM":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    if (Double.TryParse(_textEntry.EnteredText, out temp))
                    {
                        NewGameSong.Bpm = temp;
                    }
                    break;
                case "SongLength":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;

                    if (Double.TryParse(_textEntry.EnteredText, out temp))
                    {
                        NewGameSong.Length = temp;
                    }
                    break;
            }
            _keyInput = false;
        }

        public override void Initialize()
        {
            _cursorPosition = EditorCursorPosition.MAIN_MENU;
            foreach (Player player in Core.Players)
            {
                player.Playing = false;
            }
            Core.Players[0].Playing = true;
            base.Initialize();
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _backgroundSprite.Draw(spriteBatch);
            DrawHeading(spriteBatch);

            switch (_cursorPosition)
            {
                case EditorCursorPosition.SELECT_AUDIO:
                    _fileSelect.Draw(spriteBatch);
                    break;
                case EditorCursorPosition.KEY_ENTRY:
                    _textEntry.Draw(spriteBatch);
                    break;
                case EditorCursorPosition.SONG_BASICS:
                    _editProgress = 1;

                    break;
                case EditorCursorPosition.SONG_DETAILS:
                    _editProgress = 2;
                    _bpmMeter.Draw(spriteBatch);
                    //TODO: Draw "valid" graphic.
                    break;
                case EditorCursorPosition.SONG_TWEAKING:
                    _editProgress = 3;
                    _beatlineSet.Draw(spriteBatch, _phraseNumber);
                    _noteJudgementSet.Draw(spriteBatch, _phraseNumber);
                    break;
                case EditorCursorPosition.DONE:
                    _editProgress = 4;
                    break;
                case EditorCursorPosition.MAIN_MENU:
                    _editProgress = 0;
                    break;
            }

            SetActiveMenu();
            ValidateInputs();
            DrawEditProgress(spriteBatch);
            DrawText(spriteBatch);
            if (_activeMenu != null)
            {
                _menus[_activeMenu].Draw(spriteBatch);
            }
        }

        private void DrawText(SpriteBatch spriteBatch)
        {
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SONG_BASICS:
                    var currentMenu = _menus["Basics"];
                    currentMenu.ClearMenuOptions();
                    currentMenu.GetByItemText("Select Audio File").AddOption(Path.GetFileName(_audioFilePath), null);
                    currentMenu.GetByItemText("Enter Folder Name").AddOption(_destinationFolderName.Substring(_destinationFolderName.LastIndexOf("\\") + 1), null);

                    if (!String.IsNullOrEmpty(_destinationFileName))
                    {
                        currentMenu.GetByItemText("Enter Destination File Name").AddOption(_destinationFileName, null);
                    }

                    break;
                case EditorCursorPosition.SONG_DETAILS:
                    var offsetPosition = _bpmMeter.Position.Clone();
                    offsetPosition.X += 320;
                    offsetPosition.Y += 120;
                    TextureManager.DrawString(spriteBatch, String.Format("Offset: {0:0.000}", NewGameSong.Offset), "TwoTech", offsetPosition, Color.Black, FontAlign.RIGHT);
                    if ((NewGameSong.Bpm > 0) && (NewGameSong.Length > 0))
                    {
                        var totalBeatlines = Math.Floor(NewGameSong.GetEndingTimeInPhrase());
                        offsetPosition.Y -= 25;
                        totalBeatlines = Math.Max(totalBeatlines + 1, 0);
                        TextureManager.DrawString(spriteBatch, "Total beatlines: " + totalBeatlines, "TwoTech",
                                                  offsetPosition, Color.Black, FontAlign.RIGHT);
                    }
                    break;
                case EditorCursorPosition.SONG_TWEAKING:
                    DrawDebugText(spriteBatch);
                    DrawTweakControlsText(spriteBatch);
                    break;
            }
        }

        private void DrawTweakControlsText(SpriteBatch spriteBatch)
        {
            var textPosition = Core.Metrics["EditorTweakControls", 0].Clone();

            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "F5 - Decrease BPM", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "F6 - Increase BPM", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "F7 - Decrease Offset (0.1)", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "F8 - Increase Offset (0.1)", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "F9 - Decrease Offset (0.01)", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "F10 - Decrease Offset (0.01)", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "F11 - Decrease Length", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "F12 - Increase Length", textPosition, Color.Black);

            textPosition = Core.Metrics["EditorTweakControls", 1].Clone();

            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "LEFT - Decrease Beatline Speed", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "RIGHT - Increase Beatline Speed", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "BEATLINE - Hit beatline", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "SELECT - Restart Song", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "START - Complete Tweaking", textPosition, Color.Black);
        }


        private void DrawDebugText(SpriteBatch spriteBatch)
        {
            //TODO: Draw average hit offsets.
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("BPM: {0:F2}", NewGameSong.Bpm),
                   Core.Metrics["SongDebugBPM", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Offset: {0:F3}", NewGameSong.Offset),
                    Core.Metrics["SongDebugOffset", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + String.Format("{0:F3}", _phraseNumber), Core.Metrics["SongDebugPhrase", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Speed: {0}x}", Core.Players[0].BeatlineSpeed),
           Core.Metrics["SongDebugHitOffset", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Length: {0:F3}", NewGameSong.Length),
           Core.Metrics["SongDebugLength", 0], Color.Black);

            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Last Hitoffset: {0:F3}", _debugLastHitOffset),
           Core.Metrics["EditorHitOffset", 0], Color.Black);
        
        }

        private void ValidateInputs()
        {
            _menus["Basics"].GetByItemText("Next Step").Enabled = (!String.IsNullOrEmpty(_destinationFileName)) &&
                                                                  (!String.IsNullOrEmpty(_destinationFolderName)) &&
                                                                  (!String.IsNullOrEmpty(_audioFilePath));

            if (_cursorPosition == EditorCursorPosition.SONG_DETAILS)
            {
                Core.Log.Enabled = false;
                _menus["Details"].GetByItemText("Next Step").Enabled = Core.Songs.ValidateSongFile(NewGameSong);
                Core.Log.Enabled = true;
            }
        }

        private void DrawHeading(SpriteBatch spriteBatch)
        {
            var headingText = "";
            switch (_cursorPosition)
            {
                case EditorCursorPosition.MAIN_MENU:
                    headingText = "WGiBeat Song Editor";
                    break;
                case EditorCursorPosition.SELECT_AUDIO:
                    headingText = "Select Audio File";
                    break;
                case EditorCursorPosition.SONG_BASICS:
                    headingText = "Song Basics";
                    break;
                case EditorCursorPosition.SONG_DETAILS:
                    headingText = "Song Details";
                    break;
                case EditorCursorPosition.SONG_TWEAKING:
                    headingText = "Song Tweaking";
                    break;
                case EditorCursorPosition.DONE:
                    headingText = "Completed";
                    break;
            }
            TextureManager.DrawString(spriteBatch, headingText, "TwoTechLarge", Core.Metrics["EditorHeading", 0], Color.Black, FontAlign.LEFT);
        }

        private void DrawEditProgress(SpriteBatch spriteBatch)
        {
            if (_editProgress == 0)
            {
                return;
            }
            _editProgressBaseSprite.Draw(spriteBatch);

            var position = _editProgressBaseSprite.Position.Clone();
            position.X += 10;
            position.Y += 24;

            for (int x = 1; x < 5; x++)
            {
                if (_editProgress >= x)
                {
                    _editProgressSpriteMap.Draw(spriteBatch, x + 3, 196, 55, position);
                }
                else
                {
                    _editProgressSpriteMap.Draw(spriteBatch, x - 1, 196, 55, position);
                }
                position.X += 196;
            }
        }

        private void SetActiveMenu()
        {
            _activeMenu = null;
            switch (_cursorPosition)
            {
                case EditorCursorPosition.MAIN_MENU:
                    _activeMenu = "Main";
                    break;
                case EditorCursorPosition.SONG_BASICS:
                    _activeMenu = "Basics";
                    break;
                case EditorCursorPosition.SONG_DETAILS:
                    _activeMenu = "Details";
                    break;
                    case EditorCursorPosition.DONE:
                    _activeMenu = "Done";
                    break;
            }
        }

        public override void PerformAction(Action action)
        {
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SELECT_AUDIO:
                    _fileSelect.PerformAction(action);
                    return;
                case EditorCursorPosition.SONG_TWEAKING:
                    PerformTweakAction(action);
                    break;
            }

            if (_activeMenu == null)
            {
                return;
            }
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            switch (paction)
            {
                case "LEFT":
                    _menus[_activeMenu].DecrementOption();
                    break;
                case "RIGHT":
                    _menus[_activeMenu].IncrementOption();
                    break;
                case "UP":
                    _menus[_activeMenu].DecrementSelected();
                    break;
                case "DOWN":
                    _menus[_activeMenu].IncrementSelected();
                    break;
                case "START":
                    DoMenuAction();
                    break;
            }
        }

        private void PerformTweakAction(Action action)
        {
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);
            switch (paction)
            {
                case "BEATLINE":
                    HitBeatline();
                    break;
                case "SELECT":
                    RestartSong();
                    break;
                case "BPM_DECREASE":
                        NewGameSong.Bpm -= 0.1;
                    break;
                case "BPM_INCREASE":
                        NewGameSong.Bpm += 0.1;
                    break;
                case "OFFSET_DECREASE_BIG":
                        NewGameSong.Offset -= 0.1;
                    break;
                case "OFFSET_INCREASE_BIG":
                        NewGameSong.Offset += 0.1;
                    break;
                case "OFFSET_DECREASE_SMALL":
                        NewGameSong.Offset -= 0.01;
                    break;
                case "OFFSET_INCREASE_SMALL":
                        NewGameSong.Offset += 0.01;
                    break;
                case "LENGTH_DECREASE":
                        NewGameSong.Length -= 0.1;
                    break;
                case "LENGTH_INCREASE":
                        NewGameSong.Length += 0.1;
                    break;
                case "BACK":
                    Core.Songs.StopCurrentSong();
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    break;
                case "START":
                    Core.Songs.StopCurrentSong();
                    Core.Songs.SaveToFile(NewGameSong);
                    Core.Songs.AddSong(NewGameSong);
                    _cursorPosition = EditorCursorPosition.DONE;
                    break;
                case "LEFT":
                    AdjustSpeed(-1);
                    break;
                case "RIGHT":
                    AdjustSpeed(1);
                    break;
            }
        }

        private readonly double[] _speedOptions = { 0.5, 1.0, 1.5, 2.0, 3.0, 4.0, 6.0 };
        public void AdjustSpeed(int amount)
        {
            var idx = Array.IndexOf(_speedOptions, Core.Players[0].BeatlineSpeed);
            idx += amount;
            idx = Math.Min(_speedOptions.Length - 1, Math.Max(0, idx));
            Core.Players[0].BeatlineSpeed = _speedOptions[idx];
            _beatlineSet.SetSpeeds();
        }

        private void RestartSong()
        {
            throw new NotImplementedException();
        }

        private void HitBeatline()
        {

            _debugLastHitOffset = _beatlineSet.CalculateHitOffset(0, _phraseNumber);

            var judgement = _beatlineSet.AwardJudgement(_phraseNumber, 0, true);
            if (judgement == BeatlineNoteJudgement.COUNT)
            {
                return;
            }

            _noteJudgementSet.AwardJudgement(judgement, 0, 1,0);
        }


        private void DoMenuAction()
        {
            switch (_activeMenu)
            {
                case "Main":
                    DoMenuActionMain(_menus[_activeMenu]);
                    break;
                case "Basics":
                    DoMenuActionBasics(_menus[_activeMenu]);
                    break;
                case "Details":
                    DoMenuActionDetails(_menus[_activeMenu]);
                    break;
                case "SongTweak":
                    break;
                case "Done":
                    DoMenuActionDone(_menus[_activeMenu]);
                    break;
            }
        }

        private void DoMenuActionBasics(Menu menu)
        {
            switch (menu.SelectedItem().ItemValue.ToString())
            {
                case "0":
                    _fileSelect.Patterns = new[] { "*.mp3", "*.ogg", "*.wav" };
                    _cursorPosition = EditorCursorPosition.SELECT_AUDIO;
                    _fileSelect.FileSelected += delegate
                                                    {
                                                        _audioFilePath = _fileSelect.SelectedFile;
                                                        _cursorPosition = EditorCursorPosition.SONG_BASICS;
                                                    };
                    _fileSelect.FileSelectCancelled += delegate { _cursorPosition = EditorCursorPosition.SONG_BASICS; };
                  
                    break;
                case "1":
                    ActivateTextEntryMode();
                    _textEntry.DescriptionText =
                        "Enter the name of the folder where this song will be stored.\n It will be created if it doesn't exist.";
                    _textEntryDestination = "SongFolder";
                    break;
                case "2":
                    ActivateTextEntryMode();
                    _textEntry.DescriptionText =
                        "Enter the name of the song file (.sng) that will be created.\n The name has no effect on gameplay.";
                    _textEntryDestination = "SongFile";
                    break;
                case "3":
                    if (_menus["Basics"].GetByItemText("Next Step").Enabled)
                    {
                        CreateNewBasicGameSong();
                        _bpmMeter.DisplayedSong = NewGameSong;
                        _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    }
                    break;
                case "4":
                    _cursorPosition = EditorCursorPosition.MAIN_MENU;
                    break;
            }
        }

        private void DoMenuActionDetails(Menu menu)
        {
            switch ((int)menu.SelectedItem().ItemValue)
            {
                case 0:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongTitle";
                    _textEntry.DescriptionText = "Enter the title of the song.";
                    break;
                case 1:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongSubtitle";
                    _textEntry.DescriptionText = "(Optional) Enter the second title line of the song.\nUse this for very long titles, or to denote different mixes of the same song.";
                    break;
                case 2:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongArtist";
                    _textEntry.DescriptionText = "Enter the name of the artist that created the audio of this song.";

                    break;
                case 3:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongBPM";
                    _textEntry.DescriptionText = "Enter the BPM (Beats per minute) of the song.\n This is used to record the speed of the song.\n Decimal numbers are allowed.";

                    break;
                case 4:
                    //TODO: Create Song BPM calculator.
                    break;
                case 5:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongOffset";
                    _textEntry.DescriptionText = "Enter the offset, in seconds, of the song.\n This is used to record where the actual gameplay should begin,\n and should ideally be on-beat. Decimal numbers are allowed.";

                    break;
                case 6:
                    //TODO: Create Song Offset calculator.
                    break;
                case 7:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongLength";
                    _textEntry.DescriptionText = "Enter the length, in seconds, of the song.\n This is used to record where the end of the playable area of a song.\n This point is measured from the start of the audio file (not the offset), \nand does not need to be on-beat. Decimal numbers are allowed.";

                    break;
                case 8:
                    //TODO: Create song length calculator.
                    break;
                case 9:
                    if (Core.Songs.ValidateSongFile(NewGameSong))
                    {
                        Core.Songs.SaveToFile(NewGameSong);
                        _cursorPosition = EditorCursorPosition.SONG_TWEAKING;
                        _startTime = null;
                        _beatlineSet.EndingPhrase = NewGameSong.GetEndingTimeInPhrase();
                        _beatlineSet.Bpm = NewGameSong.Bpm;
                        _beatlineSet.SetSpeeds();
                        _beatlineSet.Reset();
                    }
                    break;
                case 10:
                    _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    break;
            }
        }

        private void DoMenuActionMain(Menu menu)
        {
            switch (menu.SelectedItem().ItemValue.ToString())
            {
                case "0":
                    NewGameSong = new GameSong();
                    _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    _destinationFileName = "";
                    _destinationFolderName = "";
                    _audioFilePath = "";
                    break;
                case "1":
                    //TODO: Implement "Edit Existing Song"
                    _cursorPosition = EditorCursorPosition.SELECT_SONGFILE;
                    _fileSelect.Patterns = new[] {"*.sng"};
                    _fileSelect.FileSelected += delegate
                                                    {
                                                        NewGameSong = Core.Songs.LoadFromFile(_fileSelect.SelectedFile);
                                                        _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                                                    };
                    break;
                case "2":
                    Core.ScreenTransition("MainMenu");
                    break;
            }
        }

        private void DoMenuActionDone(Menu menu)
        {
           switch (menu.SelectedItem().ItemValue.ToString())
           {
               case "0":
                   Core.ScreenTransition("MainMenu");
                   break;
                   
           }

        }

        private void CreateNewBasicGameSong()
        {
            var audioFileName = Path.GetFileName(_audioFilePath);
            var wgibeatSongsFolder = Path.GetDirectoryName(
         Assembly.GetAssembly(typeof(GameCore)).CodeBase) + "\\Songs";
            wgibeatSongsFolder = wgibeatSongsFolder.Replace("file:\\", "");
            Core.Log.AddMessage("INFO: Creating folder for new song: " + wgibeatSongsFolder + "\\" + _destinationFolderName);
            if (!Directory.Exists(wgibeatSongsFolder + "\\" + _destinationFolderName))
            {
                Directory.CreateDirectory(wgibeatSongsFolder + "\\" + _destinationFolderName);
            }
            Core.Log.AddMessage("INFO: Copying selected audio file from: " + _audioFilePath);
            Core.Log.AddMessage("INFO: ... to: " + wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName);

            if (!File.Exists(wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName))
            {
                File.Copy(_audioFilePath, wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName);
            }

            //Create gamesong file.
            //Calculate MD5 and save .sng file.
            NewGameSong = GameSong.LoadDefaults();
            NewGameSong.SongFile = audioFileName;
            NewGameSong.DefinitionFile = _destinationFileName;
            NewGameSong.Path = wgibeatSongsFolder + "\\" + _destinationFolderName;
            NewGameSong.SetMD5();
            Core.Songs.SaveToFile(NewGameSong);
        }

        private void ActivateTextEntryMode()
        {
            _keyInput = true;
            _ignoreNextKey = true;
            _textEntry.Clear();
            _cursorPosition = EditorCursorPosition.KEY_ENTRY;
        }

        public override void PerformKey(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (!_keyInput)
            {
                return;
            }
            if (_ignoreNextKey)
            {
                _ignoreNextKey = false;
                return;
            }
            _textEntry.PerformKey(key);
            base.PerformKey(key);
        }

        private double _timeElapsed;
        private double _songLoadDelay;
        public override void Update(GameTime gameTime)
        {
            if (_cursorPosition == EditorCursorPosition.SONG_TWEAKING)
            {
                if (_startTime == null)
                {
                    Core.Crossfader.SetNewChannel(-1);
                    Core.Songs.PlaySong(NewGameSong);
                    _startTime = new TimeSpan(gameTime.TotalRealTime.Ticks);
                }
                _timeElapsed =
                    (int)
                    (gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay);
                _phraseNumber = NewGameSong.ConvertMSToPhrase(_timeElapsed);
                SyncSong();
                _beatlineSet.MaintainBeatlineNotes(_phraseNumber);
            }

            base.Update(gameTime);
        }

        private int _confidence;

        private void SyncSong()
        {

            if (_cursorPosition != EditorCursorPosition.SONG_TWEAKING)
            {
                return;
            }

            //FMOD cannot reliably determine the position of the song. Using GetCurrentSongProgress()
            //as the default timing mechanism makes it jerky and slows the game down, so we attempt
            //to match current time with the song time by periodically sampling it. A hill climbing method
            // is used here.
            var delay = Core.Songs.GetCurrentSongProgress() - _timeElapsed;
            if ((_confidence < 15) && (Math.Abs(delay) > 25))
            {
                _confidence = 0;
                _songLoadDelay += delay / 2.0;
            }
            else if (_confidence < 15)
            {
                _confidence++;
            }
        }

        private enum EditorCursorPosition
        {
            MAIN_MENU,
            KEY_ENTRY,
            SELECT_AUDIO,
            SONG_BASICS,
            SONG_DETAILS,
            SONG_TWEAKING,
            DONE,
            SELECT_SONGFILE
        }
    }
}
