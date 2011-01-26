using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Drawing.Sets;
using WGiBeat.Notes;
using Action = WGiBeat.Managers.Action;
using LogLevel = WGiBeat.Managers.LogLevel;

namespace WGiBeat.Screens
{
    public class SongEditorScreen : GameScreen
    {
        #region Fields
        private readonly Dictionary<string, Menu> _menus = new Dictionary<string, Menu>();
        private readonly FileSelectDialog _fileSelect = new FileSelectDialog();
        private readonly TextEntry _textEntry = new TextEntry { Width = 800, Height = 600 };
        private BeatlineSet _beatlineSet;
        private NoteJudgementSet _noteJudgementSet;
        private CountdownSet _countdownSet;

        private bool _keyInput;
        private bool _ignoreNextKey;
        private bool _songValid;
        private bool _editMode;
        private string _errorMessage = "";
        private string _validityMessage = "";
        private int _audioChannel;
        private EditorCursorPosition _cursorPosition;

        private string _audioFilePath = "";
        private string _destinationFileName = "";
        private string _destinationFolderName = "";
        private TimeSpan? _startTime;

        private string _activeMenu;
        private int _editProgress;
        public GameSong NewGameSong;
        private string _textEntryDestination;
        private string _wgibeatSongsFolder;

        private Sprite _editProgressBaseSprite;
        private Sprite _backgroundSprite;
        private SpriteMap _editProgressSpriteMap;
        private SpriteMap _validitySpriteMap;
        private Sprite _validityTextBaseSprite;
        private BpmMeter _bpmMeter;

        private double _phraseNumber = -1;
        private double _debugLastHitOffset;
        private double _guessedBPM;

        private readonly double[] _hitOffsets = new double[10];
        private int _numHits;
        private readonly double[] _beatTimings = new double[25];
        private int _numBeats = -1;
        private double? _lastHitTime;

        private const string VERSION = "v1.1";

        #endregion

        #region Initialization Code

        public SongEditorScreen(GameCore core)
            : base(core)
        {
            CreateMenus();
            InitSprites();
            InitObjects();
            
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

        private void InitObjects()
        {
            _bpmMeter = new BpmMeter {Position = (Core.Metrics["EditorBPMMeter", 0])};
            _beatlineSet = new BeatlineSet(Core.Metrics, Core.Players, GameType.NORMAL) {Large = true};
            _noteJudgementSet = new NoteJudgementSet(Core.Metrics,Core.Players,GameType.NORMAL);
            _countdownSet = new CountdownSet(Core.Metrics, Core.Players, GameType.NORMAL);

        }

        public void InitSprites()
        {
            _backgroundSprite = new Sprite { SpriteTexture = TextureManager.Textures("AllBackground"), Height = 800 };
            _editProgressBaseSprite = new Sprite { SpriteTexture = TextureManager.Textures("SongEditProgressBase") };
            _editProgressBaseSprite.Position = (Core.Metrics["SongEditProgress", 0]);
            _editProgressSpriteMap = new SpriteMap
                                         {
                                             Columns = 4,
                                             Rows = 2,
                                             SpriteTexture = TextureManager.Textures("SongEditProgress")
                                         };
            _validitySpriteMap = new SpriteMap
                                     {
                                         Columns = 1,
                                         Rows = 2,
                                         SpriteTexture = TextureManager.Textures("EditorSongValidity")
                                     };
            _validityTextBaseSprite = new Sprite
                                          {
                                              SpriteTexture = TextureManager.Textures("EditorValidityTextBase"),
                                              Position = (Core.Metrics["EditorSongValidityMessageBase", 0])
                                          };
        }

        private void CreateMenus()
        {
            var mainMenu = new Menu { Width = 800, Position = Core.Metrics["EditorMenuStart", 0] };
            mainMenu.AddItem(new MenuItem { ItemText = "Create New Song", ItemValue = 0 });
            mainMenu.AddItem(new MenuItem { ItemText = "Edit Existing Song", ItemValue = 1 });
            mainMenu.AddItem(new MenuItem { ItemText = "Delete Song", ItemValue = 2 });
            mainMenu.AddItem(new MenuItem { ItemText = "Exit", ItemValue = 3 });
            _menus.Add("Main", mainMenu);

            var basicsMenu = new Menu { Width = 800, Position = Core.Metrics["EditorMenuStart", 0] };
            basicsMenu.AddItem(new MenuItem { ItemText = "Select Audio File", ItemValue = 0 });
            basicsMenu.AddItem(new MenuItem { ItemText = "Enter Folder Name", ItemValue = 1 });
            basicsMenu.AddItem(new MenuItem { ItemText = "Enter Destination File Name", ItemValue = 2 });
            basicsMenu.AddItem(new MenuItem { ItemText = "Next Step", ItemValue = 3, Enabled = false });
            basicsMenu.AddItem(new MenuItem { ItemText = "Back", ItemValue = 4 });
            _menus.Add("Basics", basicsMenu);

            var detailsMenu = new Menu { Width = 400, Position = Core.Metrics["EditorMenuStart", 0] };
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

            var deleteDetailsMenu = new Menu { Width = 400, Position = Core.Metrics["EditorMenuStart", 0] };
            deleteDetailsMenu.AddItem(new MenuItem { ItemText = "Delete Song File", ItemValue = 0 });
            deleteDetailsMenu.AddItem(new MenuItem { ItemText = "Delete Song And Audio", ItemValue = 1 });
            deleteDetailsMenu.AddItem(new MenuItem { ItemText = "Cancel", ItemValue = 2 });
            deleteDetailsMenu.SelectedIndex = 2;
            _menus.Add("DeleteDetails",deleteDetailsMenu);

            var doneMenu = new Menu {Width = 400, Position = Core.Metrics["EditorMenuStart", 1]};
            doneMenu.AddItem(new MenuItem{ItemText = "OK", ItemValue = 0});
            _menus.Add("Done",doneMenu);

            _wgibeatSongsFolder = Core.WgibeatRootFolder + "\\Songs";

            _fileSelect.Width = 800;
            _fileSelect.Position = Core.Metrics["EditorMenuStart", 0];

            _textEntry.EntryComplete += TextEntryEntryComplete;
            _textEntry.EntryCancelled += TextEntryEntryCancelled;

        }

        #endregion

        #region Text Entry Handlers
        private void TextEntryEntryCancelled(object sender, EventArgs e)
        {
            switch (_textEntryDestination)
            {
                case "AudioFile":
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
            double temp;
            switch (_textEntryDestination)
            {
                case "AudioFile":
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

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            _backgroundSprite.Draw(spriteBatch);
            DrawHeading(spriteBatch);

            switch (_cursorPosition)
            {
                case EditorCursorPosition.SELECT_AUDIO:
                case EditorCursorPosition.SELECT_SONGFILE:
                case EditorCursorPosition.SELECT_SONGFILE_DELETE:
                    _fileSelect.Draw(spriteBatch);
                    break;
                case EditorCursorPosition.KEY_ENTRY:
                    _textEntry.Draw(spriteBatch);
                    break;
                case EditorCursorPosition.SONG_BASICS:
                    _editProgress = 1;
                    TextureManager.DrawString(spriteBatch,_errorMessage,"LargeFont",Core.Metrics["EditorErrorMessage",0],Color.Red,FontAlign.CENTER);
                    break;
                case EditorCursorPosition.SONG_DETAILS:
                case EditorCursorPosition.SONG_DETAILS_DELETE:
                    _editProgress = 2;
                    var validIdx = _songValid ? 1 : 0;
                    _validitySpriteMap.Draw(spriteBatch, validIdx, 195, 42, Core.Metrics["EditorSongValidity",0]);
                    _bpmMeter.Draw(spriteBatch);
                    DrawBPMMeterExtras(spriteBatch);
                    if (!String.IsNullOrEmpty(_errorMessage))
                    {
                        var scale = TextureManager.ScaleTextToFit(_errorMessage, "DefaultFont", 790, 100);
                        TextureManager.DrawString(spriteBatch, "An error has occurred:", "LargeFont", Core.Metrics["EditorErrorMessage", 0], Color.Red, FontAlign.CENTER);
                        TextureManager.DrawString(spriteBatch, _errorMessage, "DefaultFont", Core.Metrics["EditorErrorMessage", 1],scale, Color.Red, FontAlign.CENTER);

                    }
                    break;
                case EditorCursorPosition.SONG_TUNING:
                    _editProgress = 3;
                    _beatlineSet.Draw(spriteBatch, _phraseNumber);
                    _noteJudgementSet.Draw(spriteBatch, _phraseNumber);
                    _countdownSet.Draw(spriteBatch,_phraseNumber);
                    break;
                    case EditorCursorPosition.MEASURE_BPM:
                    DrawBPMMeasurement(spriteBatch);
                    break;
                    case EditorCursorPosition.MEASURE_OFFSET:
                    case EditorCursorPosition.MEASURE_LENGTH:
                    TextureManager.DrawString(spriteBatch, String.Format("{0:0.00}", _timeElapsed / 1000), "LargeFont", Core.Metrics["EditorMeasurementDisplay", 0], Color.Black, FontAlign.CENTER);

                    break;
                case EditorCursorPosition.DONE:
                    _editProgress = 4;
                    break;
                case EditorCursorPosition.MAIN_MENU:
                    _editProgress = 0;
                    TextureManager.DrawString(spriteBatch,VERSION,"DefaultFont",Core.Metrics["EditorVersion",0],Color.Black,FontAlign.RIGHT);
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

        private void DrawBPMMeterExtras(SpriteBatch spriteBatch)
        {
            _validityTextBaseSprite.Draw(spriteBatch);
        }

        private void DrawBPMMeasurement(SpriteBatch spriteBatch)
        {
            
            TextureManager.DrawString(spriteBatch, String.Format("Last hit: {0:F1}", 60 / _beatTimings[0] * 1000), "DefaultFont", Core.Metrics["EditorBPMMeasurements", 0], Color.Black, FontAlign.LEFT);
            var avgAmount = (_numBeats >= 5) ? String.Format("{0:F1}", 60 / _beatTimings.Take(5).Average() * 1000) : "-----" ;

            TextureManager.DrawString(spriteBatch, "Last 5 avg: " + avgAmount, "DefaultFont", Core.Metrics["EditorBPMMeasurements", 1], Color.Black, FontAlign.LEFT);
            
            avgAmount = (_numBeats >= 10) ? String.Format("{0:F1}", 60 / _beatTimings.Take(10).Average() * 1000) : "-----";
            TextureManager.DrawString(spriteBatch, "Last 10 avg: " + avgAmount, "DefaultFont", Core.Metrics["EditorBPMMeasurements", 2], Color.Black, FontAlign.LEFT);

            avgAmount = (_numBeats > 0)
                            ? String.Format("{0:F1}", 60/_beatTimings.Take(_numBeats).Average()*1000)
                            : "-----";
            TextureManager.DrawString(spriteBatch, "Last 25 avg: "+ avgAmount, "DefaultFont", Core.Metrics["EditorBPMMeasurements", 3], Color.Black, FontAlign.LEFT);

            var roundedAmount = (_numBeats > 0) ? "" + Math.Round(60/ _beatTimings.Take(_numBeats).Average() * 1000) : "-----";
            TextureManager.DrawString(spriteBatch, "Estimated BPM: " + roundedAmount, "TwoTech36", Core.Metrics["EditorMeasurementDisplay", 0], Color.Black, FontAlign.CENTER);

        }

        private void DrawText(SpriteBatch spriteBatch)
        {
            string instructions = "";
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
                case EditorCursorPosition.SONG_DETAILS_DELETE:

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
                    TextureManager.DrawString(spriteBatch, _validityMessage, "DefaultFont", Core.Metrics["EditorSongValidityMessage",0],Color.Black, FontAlign.CENTER);
                    break;
                case EditorCursorPosition.SONG_TUNING:
                    DrawDebugText(spriteBatch);
                    DrawTweakControlsText(spriteBatch);
                    DrawHitOffsetText(spriteBatch);
                    break;
                case EditorCursorPosition.MEASURE_OFFSET:
                    instructions = "Press START or BEATLINE to mark the offset position in the song.";
                    instructions +=
                        "\nThis is used to record where the actual gameplay should begin,\n and should ideally be on-beat.";
                    instructions += "\nPress Escape to cancel.";

                    break;
                case EditorCursorPosition.MEASURE_LENGTH:
                    instructions = "Press START or BEATLINE to mark the end of the playable area of the song.";
                    instructions +=
                        "\nThis does not need to be on-beat, as the last beatline is calculated automatically.";
                    instructions += "\nPress Escape to cancel.";

                    break;
                case EditorCursorPosition.MEASURE_BPM:
                    instructions = "Use BEATLINE to tap the beats of the song.";
                    instructions +=
                        "\nThe BPM will be calculated based on the average time between taps.";
                    instructions += "\nNote that most songs have a BPM that is a whole number.";
                    instructions += "\nPress START to use the estimated BPM, or press Escape to cancel.";

                    break;
                    case EditorCursorPosition.DONE:
                    instructions = "The song has been created successfully, and saved in the designated folder.";
                    instructions +=
                        "\nIt is now playable by selecting it from the Song Select Screen.";
                    instructions += "\nThe song can be edited later by selecting 'Edit Existing Song'";
                    instructions += "\nfrom the WGiEdit main menu.";
                    instructions += "\nPress START to return to the main menu.";
                    break;
                    case EditorCursorPosition.DONE_DELETE:
                    instructions = "The song has been deleted successfully.";
                    instructions += "\nPress START to return to the main menu.";
                    break;
            }
            TextureManager.DrawString(spriteBatch, instructions, "DefaultFont", Core.Metrics["EditorMeasureInstructions", 0], Color.Black, FontAlign.CENTER);

        }

        private void DrawHitOffsetText(SpriteBatch spriteBatch)
        {

            TextureManager.DrawString(spriteBatch, String.Format("Last hit: {0:F3}", _hitOffsets[0]), "DefaultFont", Core.Metrics["EditorHitOffset", 0], Color.Black, FontAlign.LEFT);
            var avg = _hitOffsets.Take(3).Average();
            if (_numHits < 3)
                return;

            TextureManager.DrawString(spriteBatch, String.Format("Last 3 avg: {0:F3}", avg), "DefaultFont", Core.Metrics["EditorHitOffset", 1], Color.Black, FontAlign.LEFT);
            if (_numHits < 5)
                return;
            avg = _hitOffsets.Take(5).Average();
            TextureManager.DrawString(spriteBatch, String.Format("Last 5 avg: {0:F3}", avg), "DefaultFont", Core.Metrics["EditorHitOffset", 2], Color.Black, FontAlign.LEFT);
            if (_numHits < 10)
                return;
            avg = _hitOffsets.Average();
            TextureManager.DrawString(spriteBatch, String.Format("Last 10 avg: {0:F3}", avg), "DefaultFont", Core.Metrics["EditorHitOffset", 3], Color.Black, FontAlign.LEFT);

        }

        private void DrawTweakControlsText(SpriteBatch spriteBatch)
        {
            var textPosition = Core.Metrics["EditorTweakControls", 0].Clone();

            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "F5 - Decrease BPM", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "F6 - Increase BPM", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "F7 - Decrease Offset (0.1)", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "F8 - Increase Offset (0.1)", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "F9 - Decrease Offset (0.01)", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "F10 - Increase Offset (0.01)", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "F11 - Decrease Length", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "F12 - Increase Length", textPosition, Color.Black);

            textPosition = Core.Metrics["EditorTweakControls", 1].Clone();

            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "LEFT - Decrease Beatline Speed", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "RIGHT - Increase Beatline Speed", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "BEATLINE - Hit beatline", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "SELECT - Restart Song", textPosition, Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), "START - Complete Tuning", textPosition, Color.Black);
        }


        private void DrawDebugText(SpriteBatch spriteBatch)
        {
            TextureManager.DrawString(spriteBatch, String.Format("BPM: {0:F2}", NewGameSong.Bpm),
                   "DefaultFont", Core.Metrics["SongDebugBPM", 0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("Offset: {0:F3}", NewGameSong.Offset),
                    "DefaultFont", Core.Metrics["SongDebugOffset", 0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + String.Format("{0:F3}", _phraseNumber),
                "DefaultFont", Core.Metrics["EditorSongPhraseNumber", 0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("Speed: {0}x", Core.Players[0].BeatlineSpeed),
                "DefaultFont", Core.Metrics["SongDebugHitOffset", 0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("Length: {0:F3}", NewGameSong.Length),
                "DefaultFont", Core.Metrics["SongDebugLength", 0], Color.Black, FontAlign.LEFT);       
        }

        private void DrawHeading(SpriteBatch spriteBatch)
        {
            var headingText = "";
            switch (_cursorPosition)
            {
                case EditorCursorPosition.MAIN_MENU:
                    headingText = "WGiEdit";
                    break;
                case EditorCursorPosition.SELECT_AUDIO:
                    headingText = "Select Audio File";
                    break;
                case EditorCursorPosition.SELECT_SONGFILE:
                    headingText = "Select Song To Edit";
                    break;
                case EditorCursorPosition.SONG_BASICS:
                    headingText = "Song Basics";
                    break;
                case EditorCursorPosition.SONG_DETAILS:
                    headingText = "Song Details";
                    break;
                case EditorCursorPosition.SONG_DETAILS_DELETE:
                case EditorCursorPosition.SELECT_SONGFILE_DELETE:
                    headingText = "Delete Song";
                    break;
                    case EditorCursorPosition.MEASURE_BPM:
                    headingText = "Measure BPM";
                    break;
                    case EditorCursorPosition.MEASURE_LENGTH:
                    headingText = "Measure Length";
                    break;
                case EditorCursorPosition.MEASURE_OFFSET:
                    headingText = "Measure Offset";
                    break;
                case EditorCursorPosition.SONG_TUNING:
                    headingText = "Song Tuning";
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

        #endregion

        #region Action Performers

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

        public override void PerformAction(Action action)
        {
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SELECT_AUDIO:
                case EditorCursorPosition.SELECT_SONGFILE:
                case EditorCursorPosition.SELECT_SONGFILE_DELETE:
                    _fileSelect.PerformAction(action);
                    return;
                case EditorCursorPosition.SONG_TUNING:
                    PerformActionTweak(action);
                    break;
                    case EditorCursorPosition.MEASURE_BPM:
                    case EditorCursorPosition.MEASURE_LENGTH:
                    case EditorCursorPosition.MEASURE_OFFSET:
                    PerformActionMeasurement(action);
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

        private void PerformActionMeasurement(Action action)
        {
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);
            switch (paction)
            {
                case "BEATLINE":
                case "START":
                    switch (_cursorPosition)
                    {
                        case EditorCursorPosition.MEASURE_BPM:
                            if (paction == "BEATLINE")
                            {
                                UpdateBPMMeasurement();

                            }
                            else if (paction == "START")
                            {
                                Core.Audio.StopChannel(_audioChannel);
                                _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                                _songPlaying = false;
                                if (_guessedBPM > 0)
                                {
                                    NewGameSong.Bpm = _guessedBPM;
                                }
                            }
                          

                            break;
                            case EditorCursorPosition.MEASURE_LENGTH:
                            NewGameSong.Length = Math.Round(_timeElapsed / 1000, 2);
                            Core.Audio.StopChannel(_audioChannel);
                            _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                            _songPlaying = false;
                            break;
                            case EditorCursorPosition.MEASURE_OFFSET:
                            NewGameSong.Offset = Math.Round(_timeElapsed /1000,2);
                            Core.Audio.StopChannel(_audioChannel);
                            _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                            _songPlaying = false;
                            break;
                    }
                    break;

                case "BACK":
                    Core.Audio.StopChannel(_audioChannel);
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    _songPlaying = false;
                    break;
            }
        }

        private void PerformActionTweak(Action action)
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
                    _songPlaying = false;
                    break;
                case "START":
                    Core.Log.AddMessage(String.Format("Attempting to save song tweaks to: {0}\\{1}",NewGameSong.Path,NewGameSong.DefinitionFile),LogLevel.DEBUG);
                    Core.Songs.StopCurrentSong();
                    Core.Songs.SaveToFile(NewGameSong);
                    var oldSongFile = Core.Songs.GetBySongFile(NewGameSong.Path, NewGameSong.AudioFile);
                    if (oldSongFile != null)
                    {
                        Core.Songs.RemoveSong(oldSongFile);
                    }
                    Core.Songs.AddSong(NewGameSong);
                    _cursorPosition = EditorCursorPosition.DONE;
                    _songPlaying = false;
                    Core.Log.AddMessage(String.Format("SongFile '{0}' was saved successfully!", NewGameSong.DefinitionFile),LogLevel.DEBUG);
                    break;
                case "LEFT":
                    AdjustSpeed(-1);
                    break;
                case "RIGHT":
                    AdjustSpeed(1);
                    break;
            }
        }

        private void UpdateBPMMeasurement()
        {
            _numBeats = Math.Min(_beatTimings.Length, _numBeats + 1);

            if (_lastHitTime != null)
            {
                for (int x = _numBeats; x > 0; x--)
                {
                    if (x == _beatTimings.Length)
                        continue;

                    _beatTimings[x] = _beatTimings[x - 1];
                }
                _beatTimings[0] = _timeElapsed - _lastHitTime.Value;
            }
            _lastHitTime = _timeElapsed;

            if (_numBeats > 0)
            {
                _guessedBPM = Math.Round(60 / _beatTimings.Take(_numBeats).Average() * 1000);
            }

        }

        #region Tweak Action Helpers

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
            _startTime = null;
            _songPlaying = false;
            _confidence = 0;
            Core.Songs.StopCurrentSong();
            _beatlineSet.Reset();
            _noteJudgementSet.Reset();
        }

        private void HitBeatline()
        {

            _debugLastHitOffset = _beatlineSet.CalculateHitOffset(0, _phraseNumber);
            _numHits = Math.Min(_hitOffsets.Length, _numHits+ 1);
            
            for (int x = _numHits; x > 0; x--)
            {
                if (x == _hitOffsets.Length)
                    continue;

                _hitOffsets[x] = _hitOffsets[x - 1];
            }
            _hitOffsets[0] = _debugLastHitOffset;
            var judgement = _beatlineSet.AwardJudgement(_phraseNumber, 0, true);
            if (judgement == BeatlineNoteJudgement.COUNT)
            {
                return;
            }

            _noteJudgementSet.AwardJudgement(judgement, 0, 1,0);
        }

        #endregion
#endregion

        #region Menu Action Handlers

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
                case "DeleteDetails":
                    DoMenuActionDeleteDetails(_menus[_activeMenu]);
                    break;
                case "SongTweak":
                    break;
                case "Done":
                    DoMenuActionDone(_menus[_activeMenu]);
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
                    _editMode = false;
                    break;
                case "1":
                    _cursorPosition = EditorCursorPosition.SELECT_SONGFILE;
                    ActivateEditMode(EditorCursorPosition.SONG_DETAILS);
                    break;
                case "2":
                    _cursorPosition = EditorCursorPosition.SELECT_SONGFILE_DELETE;
                    ActivateEditMode(EditorCursorPosition.SONG_DETAILS_DELETE);
                    break;
                case "3":
                    Core.ScreenTransition("MainMenu");
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
                    _fileSelect.CurrentFolder = Path.GetFullPath(_wgibeatSongsFolder + "\\..");
                    _fileSelect.ResetEvents();
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
                    _textEntryDestination = "AudioFile";
                    break;
                case "3":
                    if (_menus["Basics"].GetByItemText("Next Step").Enabled)
                    {
                        Core.Log.AddMessage(String.Format("Attempting to create a basic new GameSong at: {0}\\{1}", _destinationFolderName, _destinationFileName),LogLevel.DEBUG);
                        _errorMessage = "";
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
                    ActivateMeasureMode();
                    _numBeats = -1;
                    _guessedBPM = 0;
                    Core.Players[0].Streak = 0;
                    _lastHitTime = null;
                    _cursorPosition = EditorCursorPosition.MEASURE_BPM;
                    break;
                case 5:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongOffset";
                    _textEntry.DescriptionText = "Enter the offset, in seconds, of the song.\n This is used to record where the actual gameplay should begin,\n and should ideally be on-beat. Decimal numbers are allowed.";
                    
                    break;
                case 6:
                    ActivateMeasureMode();
                    _cursorPosition = EditorCursorPosition.MEASURE_OFFSET;
                    break;
                case 7:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongLength";
                    _textEntry.DescriptionText = "Enter the length, in seconds, of the song.\n This is used to record where the end of the playable area of a song.\n This point is measured from the start of the audio file (not the offset), \nand does not need to be on-beat. Decimal numbers are allowed.";

                    break;
                case 8:
                    _cursorPosition = EditorCursorPosition.MEASURE_LENGTH;
                    ActivateMeasureMode();
                    break;
                case 9:
                    
                    if (Core.Songs.ValidateSongFile(NewGameSong))
                    {
                        Core.Log.AddMessage(String.Format("DEBUG: Attempting to save song details to: {0}\\{1}", NewGameSong.Path, NewGameSong.DefinitionFile), LogLevel.DEBUG);
                        Core.Songs.SaveToFile(NewGameSong);
                        _cursorPosition = EditorCursorPosition.SONG_TUNING;
                        _startTime = null;
                        _songPlaying = false;
                        _confidence = 0;
                        _beatlineSet.EndingPhrase = NewGameSong.GetEndingTimeInPhrase();
                        _beatlineSet.Bpm = NewGameSong.Bpm;
                        _beatlineSet.SetSpeeds();
                        _beatlineSet.Reset();
                        _noteJudgementSet.Reset();
                        _numHits = 0;
                    }
                    break;
                case 10:
                    if (_editMode)
                    {
                        _cursorPosition = EditorCursorPosition.MAIN_MENU;
                    }
                    else
                    {
                        _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    }
                    break;
            }
        }

        private void DoMenuActionDeleteDetails(Menu menu)
        {
            switch ((int) menu.SelectedItem().ItemValue)
            {
                case 0:
                    _errorMessage = Core.Songs.DeleteSongFile(NewGameSong, false);
                    if (String.IsNullOrEmpty(_errorMessage))
                    {
                        _cursorPosition = EditorCursorPosition.DONE_DELETE;
                    }
                    break;
                case 1:
                    _errorMessage = Core.Songs.DeleteSongFile(NewGameSong, true);
                    if (String.IsNullOrEmpty(_errorMessage))
                    {
                        _cursorPosition = EditorCursorPosition.DONE_DELETE;
                    }
                    break;
                case 2:
                    _cursorPosition = EditorCursorPosition.MAIN_MENU;
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

        #endregion

        #region Misc Helpers

        private void CreateNewBasicGameSong()
        {
            var audioFileName = Path.GetFileName(_audioFilePath);
            
            Core.Log.AddMessage("Creating folder for new song: " + _wgibeatSongsFolder + "\\" + _destinationFolderName, LogLevel.DEBUG);
            if (!Directory.Exists(_wgibeatSongsFolder + "\\" + _destinationFolderName))
            {
                Directory.CreateDirectory(_wgibeatSongsFolder + "\\" + _destinationFolderName);
            }
            Core.Log.AddMessage("Copying selected audio file from: " + _audioFilePath, LogLevel.DEBUG);
            Core.Log.AddMessage(" ... to: " + _wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName, LogLevel.DEBUG);

            if (!File.Exists(_wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName))
            {
                File.Copy(_audioFilePath, _wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName);
            }

            //Create gamesong file.
            //Calculate MD5 and save .sng file.
            NewGameSong = GameSong.LoadDefaults();
            NewGameSong.AudioFile = audioFileName;
            NewGameSong.DefinitionFile = _destinationFileName;
            NewGameSong.Path = _wgibeatSongsFolder + "\\" + _destinationFolderName;
            NewGameSong.SetMD5();
            Core.Songs.SaveToFile(NewGameSong);

            //Read audio metadata
            AddAudioMetaData();
            Core.Log.AddMessage("New basic game song created successfully.", LogLevel.INFO);
        }

        private void AddAudioMetaData()
        {

            var tags = Core.Audio.GetAudioFileMetadata(_audioFilePath);
            var titleTag = "";
            if (tags.ContainsKey("TITLE"))
            {
                titleTag = tags["TITLE"];
            }
            if (tags.ContainsKey("TIT2"))
            {
                titleTag = tags["TIT2"];
            }

            titleTag = titleTag.Replace("[", "(");
            titleTag = titleTag.Replace("]", ")");

            if (titleTag.IndexOf("(") < titleTag.IndexOf(")"))
            {
                var length = titleTag.LastIndexOf(")") - titleTag.IndexOf("(") + 1;
                NewGameSong.Subtitle = titleTag.Substring(titleTag.IndexOf("("), length);
                NewGameSong.Title = titleTag.Substring(0, titleTag.IndexOf("("));
            }
            else
            {
                NewGameSong.Title = tags["TITLE"];
            }

            if (tags.ContainsKey("ARTIST"))
            {
                NewGameSong.Artist = tags["ARTIST"];
            }
            if (tags.ContainsKey("TPE1"))
            {
                NewGameSong.Artist = tags["TPE1"];
            }
        }

        private void ValidateInputs()
        {
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SONG_BASICS:
                    var invalidFilename =
                        (from e in Path.GetInvalidFileNameChars() where _destinationFileName.Contains("" + e) select e).
                            Any();
                    var invalidDirname =
                        (from e in Path.GetInvalidPathChars() where _destinationFolderName.Contains("" + e) select e).
                            Any();
                    _menus["Basics"].GetByItemText("Next Step").Enabled =
                        (!String.IsNullOrEmpty(_destinationFileName)) &&
                        (!String.IsNullOrEmpty(_destinationFolderName)) &&
                        (!String.IsNullOrEmpty(_audioFilePath)) &&
                        (!invalidFilename) &&
                        (!invalidDirname);

                    _errorMessage = "";

                    if (invalidDirname)
                    {
                        _errorMessage = "Destination Folder name contains invalid characters.";
                    }
                    if (invalidFilename)
                    {
                        _errorMessage = "Destination File name contains invalid characters.";
                    }
                    break;
                case EditorCursorPosition.SONG_DETAILS:             
                    _songValid =
                        _menus["Details"].GetByItemText("Next Step").Enabled =
                        Core.Songs.ValidateSongFile(NewGameSong, out _validityMessage);
                    break;
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
                    case EditorCursorPosition.SONG_DETAILS_DELETE:
                    _activeMenu = "DeleteDetails";
                    break;
                case EditorCursorPosition.DONE:
                case EditorCursorPosition.DONE_DELETE:
                    _activeMenu = "Done";
                    break;
            }
        }

        #endregion

        #region Mode Activator Helpers

        private void ActivateTextEntryMode()
        {
            _keyInput = true;
            _ignoreNextKey = true;
            _textEntry.Clear();
            _cursorPosition = EditorCursorPosition.KEY_ENTRY;
        }

        private void ActivateMeasureMode()
        {
            _startTime = null;
            _songPlaying = true;
            _audioChannel = Core.Audio.PlaySoundEffect(NewGameSong.Path + "\\" + NewGameSong.AudioFile, false, false);
            _confidence = 0;
            //Only play the last 15 seconds of the song when measuring the length of the song.
            if (_cursorPosition == EditorCursorPosition.MEASURE_LENGTH)
            {
                var songLength = Core.Audio.GetChannelLength(_audioChannel);
                if (songLength > 15000)
                {
                    _timeElapsed += songLength - 15000;
                    Core.Audio.SetPosition(_audioChannel, songLength - 15000);
                }
            }
        }

        private void ActivateEditMode(EditorCursorPosition position)
        {
            _fileSelect.Patterns = new[] { "*.sng" };
            _fileSelect.CurrentFolder = _wgibeatSongsFolder;
            _fileSelect.ResetEvents();
            _fileSelect.FileSelected += delegate
            {
                NewGameSong = Core.Songs.LoadFromFile(_fileSelect.SelectedFile, false);
                _bpmMeter.DisplayedSong = NewGameSong;
                _cursorPosition = position;
                _editMode = true;
                NewGameSong.SetMD5();
            };
            _fileSelect.FileSelectCancelled += delegate
            {
                _cursorPosition = EditorCursorPosition.MAIN_MENU;
            };
        }

        #endregion

        #region Song Syncing

        private double _timeElapsed;
        private double _songLoadDelay;
        public override void Update(GameTime gameTime)
        {

            if (_startTime == null)
            {
                _startTime = new TimeSpan(gameTime.TotalRealTime.Ticks);
            }
            _timeElapsed = (int) (gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay);

            if (_cursorPosition == EditorCursorPosition.SONG_TUNING)
            {

                if (!_songPlaying)
                {
                    Core.Crossfader.SetNewChannel(-1);
                    _audioChannel = Core.Songs.PlaySong(NewGameSong);
                    _songPlaying = true;
                }

                _phraseNumber = NewGameSong.ConvertMSToPhrase(_timeElapsed);
                
                _beatlineSet.MaintainBeatlineNotes(_phraseNumber);
            }
            if (_songPlaying )
            {
                SyncSong();
            }
            base.Update(gameTime);
        }

        private int _confidence;
        private bool _songPlaying;

        private void SyncSong()
        {

            if (!_songPlaying)
            {
                return;
            }

            //FMOD cannot reliably determine the position of the song. Using GetCurrentSongProgress()
            //as the default timing mechanism makes it jerky and slows the game down, so we attempt
            //to match current time with the song time by periodically sampling it. A hill climbing method
            // is used here.
            var delay = Core.Audio.GetChannelPosition(_audioChannel) - _timeElapsed;
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

        #endregion

        private enum EditorCursorPosition
        {
            MAIN_MENU,
            KEY_ENTRY,
            SELECT_AUDIO,
            SELECT_SONGFILE,
            SELECT_SONGFILE_DELETE,
            SONG_BASICS,
            SONG_DETAILS,
            SONG_DETAILS_DELETE,
            SONG_TUNING,
            MEASURE_BPM,
            MEASURE_OFFSET,
            MEASURE_LENGTH,
            DONE,
            DONE_DELETE
        }
    }
}
