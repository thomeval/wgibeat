using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Drawing.Sets;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;
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
        private SongTimeLine _songTimeLine;

        private bool _keyInput;
        private bool _ignoreNextKey;
        private bool _songValid;
        private bool _editMode;
        private bool _importMode;
        private string _errorMessage = "";
        private string _validityMessage = "";
        private int _audioChannel;
        private EditorCursorPosition _cursorPosition;

        private string _sourceFilePath = "";
        private string _destinationFileName = "";
        private string _destinationFolderName = "";
        private TimeSpan? _startTime;

        private string _activeMenu;
        private int _editProgress;
        public GameSong NewGameSong;
        private string _textEntryDestination;
        private string _wgibeatSongsFolder;

        private Sprite3D _editProgressBaseSprite;
        private Sprite3D _backgroundSprite;
        private Sprite3D _songDetailsDisplaySprite;
        private Sprite3D _songBackgroundDisplaySprite;

        private SpriteMap3D _editProgressSpriteMap;
        private SpriteMap3D _validitySpriteMap;
        private Sprite3D _validityTextBaseSprite;
        private Sprite3D _textBackground;
        private BpmMeter _bpmMeter;
        private SongTypeDisplay _songTypeDisplay;

        private double _phraseNumber = -1;
        private double _debugLastHitOffset;
        private double _guessedBPM;

        private readonly double[] _hitOffsets = new double[10];
        private int _numHits;
        private readonly double[] _beatTimings = new double[25];
        private int _numBeats = -1;
        private double? _lastHitTime;

        private const string VERSION = "v1.4";

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
            _beatlineSet = new BeatlineSet(Core.Metrics, Core.Players, GameType.NORMAL);
            _noteJudgementSet = new NoteJudgementSet(Core.Metrics,Core.Players,GameType.NORMAL,null,null);
            _countdownSet = new CountdownSet(Core.Metrics, Core.Players, GameType.NORMAL);
            _songTimeLine = new SongTimeLine
                                {Size = Core.Metrics["EditorSongTimeLine.Size",0], Position = Core.Metrics["EditorSongTimeLine", 0], ShowLabels=true};
            _songTypeDisplay = new SongTypeDisplay { Position = Core.Metrics["EditorSongTypeDisplay", 0], Width = 155, Height = 40 };

        }

        public void InitSprites()
        {
            _backgroundSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("EditorBackground"),
                Size = Core.Metrics["ScreenBackground.Size", 0],
                Position = Core.Metrics["ScreenBackground", 0],
            };
            _editProgressBaseSprite = new Sprite3D
                                          {
                                              Texture = TextureManager.Textures("SongEditProgressBase"),
                                              Position = (Core.Metrics["SongEditProgress", 0]),
                                              Size = Core.Metrics["SongEditProgress.Size", 0]
                                          };
            _editProgressSpriteMap = new SpriteMap3D
                                         {
                                             Columns = 4,
                                             Rows = 2,
                                             Texture = TextureManager.Textures("SongEditProgress")
                                         };
            _validitySpriteMap = new SpriteMap3D
                                     {
                                         Columns = 1,
                                         Rows = 2,
                                         Texture = TextureManager.Textures("EditorSongValidity")
                                     };
            _validityTextBaseSprite = new Sprite3D
                                          {
                                              Texture = TextureManager.Textures("EditorValidityTextBase"),
                                              Position = (Core.Metrics["EditorSongValidityMessageBase", 0])
                                          };
            _songDetailsDisplaySprite = new Sprite3D
                                            {
                                                Position = Core.Metrics["EditorSongDetailsDisplay", 0],
                                                Texture = TextureManager.Textures("EditorDetailsDisplay")
                                            };
            _songBackgroundDisplaySprite = new Sprite3D
                                               {
                                                   Position = Core.Metrics["EditorSongBackgroundDisplay",0],
                                                   Texture =
                                                       TextureManager.Textures("EditorSongBackgroundDisplay")
                                               };
            _textBackground = new Sprite3D
            {
                Texture = TextureManager.Textures("MainGameTextBackground"),
                Position = Core.Metrics["EditorTextBackground", 0]
            };
        }

        private void CreateMenus()
        {
            var mainMenu = new Menu { Width = GameCore.INTERNAL_WIDTH / 2, Position = Core.Metrics["EditorMenuStart", 0] };
            mainMenu.AddItem(new MenuItem { ItemText = "Create New Song", ItemValue = 0 });
            mainMenu.AddItem(new MenuItem { ItemText = "Edit Existing Song", ItemValue = 1 });
            mainMenu.AddItem(new MenuItem{ItemText = "Import .sm or .dwi Song", ItemValue = 2});
            mainMenu.AddItem(new MenuItem { ItemText = "Delete Song", ItemValue = 3 });
            mainMenu.AddItem(new MenuItem { ItemText = "Exit", ItemValue = 4, IsCancel = true });
            _menus.Add("Main", mainMenu);

            var basicsMenu = new Menu { Width = GameCore.INTERNAL_WIDTH / 2, Position = Core.Metrics["EditorMenuStart", 0] };
            basicsMenu.AddItem(new MenuItem { ItemText = "Select Source File", ItemValue = 0 });
            basicsMenu.AddItem(new MenuItem { ItemText = "Enter Folder Name", ItemValue = 1 });
            basicsMenu.AddItem(new MenuItem { ItemText = "Enter Destination File Name", ItemValue = 2 });
            basicsMenu.AddItem(new MenuItem { ItemText = "Next Step", ItemValue = 3, Enabled = false });
            basicsMenu.AddItem(new MenuItem { ItemText = "Back", ItemValue = 4, IsCancel = true});
            _menus.Add("Basics", basicsMenu);

            var detailsMenu = new Menu { Width = GameCore.INTERNAL_WIDTH / 2, Position = Core.Metrics["EditorMenuStart", 0], MaxVisibleItems = 12 };
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Title", ItemValue = 0 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Subtitle", ItemValue = 1 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Artist", ItemValue = 2 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter BPM Manually", ItemValue = 3 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure BPM", ItemValue = 4 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Offset Manually", ItemValue = 5 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure Offset", ItemValue = 6 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Length Manually", ItemValue = 7 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure Length", ItemValue = 8 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Audio Start Manually", ItemValue = 9 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure Audio Start", ItemValue = 10 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Select Background Image", ItemValue = 11 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Remove Background Image", ItemValue = 12 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Next Step", ItemValue = 13, Enabled = false });
            detailsMenu.AddItem(new MenuItem { ItemText = "Back", ItemValue = 14, IsCancel = true });
            _menus.Add("Details", detailsMenu);

            var deleteDetailsMenu = new Menu { Width = GameCore.INTERNAL_WIDTH / 2, Position = Core.Metrics["EditorMenuStart", 0] };
            deleteDetailsMenu.AddItem(new MenuItem { ItemText = "Delete Song File", ItemValue = 0 });
            deleteDetailsMenu.AddItem(new MenuItem { ItemText = "Delete Song And Audio", ItemValue = 1 });
            deleteDetailsMenu.AddItem(new MenuItem { ItemText = "Cancel", ItemValue = 2, IsCancel = true });
            deleteDetailsMenu.SelectedIndex = 2;
            _menus.Add("DeleteDetails",deleteDetailsMenu);

            var doneMenu = new Menu {Width = 300, Position = Core.Metrics["EditorMenuStart", 1]};
            doneMenu.AddItem(new MenuItem{ItemText = "OK", ItemValue = 0});
            _menus.Add("Done",doneMenu);

            _wgibeatSongsFolder = Core.WgibeatRootFolder + "\\Songs";

            _fileSelect.Width = GameCore.INTERNAL_WIDTH;
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
                case "DefinitionFile":
                case "SongFolder":
                    _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    break;
                case "SongTitle":
                case "SongSubtitle":
                case "SongArtist":
                case "SongOffset":
                case "SongBPM":
                case "SongLength":
                case "SongAudioStart":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    break;
            }
            _keyInput = false;
            RaiseSoundTriggered(SoundEvent.MENU_BACK);
        }

        private void TextEntryEntryComplete(object sender, EventArgs e)
        {
            double temp;
            bool isDouble = Double.TryParse(_textEntry.EnteredText.Replace(',','.'), NumberStyles.Number,
                                            CultureInfo.InvariantCulture.NumberFormat, out temp);
            switch (_textEntryDestination)
            {
                case "DefinitionFile":
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
                    if (isDouble)
                    {
                        NewGameSong.Offset = temp;
                    }
                    break;
                case "SongBPM":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    if (isDouble)
                    {
                        NewGameSong.StartBPM = temp;
                        _bpmMeter.DisplayedSong = NewGameSong;
                    }
                    break;
                case "SongLength":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    if (isDouble)
                    {
                        NewGameSong.Length = temp;
                    }
                    break;
                case "SongAudioStart":
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    if (isDouble)
                    {
                        NewGameSong.AudioStart = temp;
                    }
                    break;
            }
            _keyInput = false;
            RaiseSoundTriggered(SoundEvent.MENU_DECIDE);
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
         
            _backgroundSprite.Draw();
            DrawHeading();

            switch (_cursorPosition)
            {
                case EditorCursorPosition.SELECT_AUDIO:
                case EditorCursorPosition.SELECT_SONGFILE:
                case EditorCursorPosition.SELECT_SONGFILE_DELETE:
                case EditorCursorPosition.SELECT_BACKGROUND_IMAGE:
                    _fileSelect.Draw();
                    break;
                case EditorCursorPosition.KEY_ENTRY:
                    _textEntry.Draw();
                    break;
                case EditorCursorPosition.SONG_BASICS:
                    _editProgress = 1;
                    FontManager.DrawString(_errorMessage,"LargeFont",Core.Metrics["EditorErrorMessage",0],Color.Red,FontAlign.Center);
                    break;
                case EditorCursorPosition.SONG_DETAILS:
                case EditorCursorPosition.SONG_DETAILS_DELETE:
                    _editProgress = 2;
                    var validIdx = _songValid ? 1 : 0;
                    _validitySpriteMap.Draw( validIdx, 195, 42, Core.Metrics["EditorSongValidity",0]);
                    _songBackgroundDisplaySprite.Draw();
                    _bpmMeter.Draw();
                    DrawBPMMeterExtras();
                    DrawSongTimeLine();
                    if (!String.IsNullOrEmpty(_errorMessage))
                    {
                        var scale = FontManager.ScaleTextToFit(_errorMessage, "DefaultFont", 790, 100);
                        FontManager.DrawString("An error has occurred:", "LargeFont", Core.Metrics["EditorErrorMessage", 0], Color.Red, FontAlign.Center);
                        FontManager.DrawString(_errorMessage, "DefaultFont", Core.Metrics["EditorErrorMessage", 1],scale, Color.Red, FontAlign.Center);

                    }
                    break;
                case EditorCursorPosition.SONG_TUNING:
                    _editProgress = 3;
                    TextureManager.LastDrawnPhraseNumber = _phraseNumber;
                    _beatlineSet.Draw( _phraseNumber);
                    _noteJudgementSet.Draw( _phraseNumber);
                    _countdownSet.Draw(_phraseNumber);
                    break;
                    case EditorCursorPosition.MEASURE_BPM:
                    DrawBPMMeasurement();
                    DrawSongTimeLine();
                    break;
                    case EditorCursorPosition.MEASURE_OFFSET:
                    case EditorCursorPosition.MEASURE_LENGTH:
                    case EditorCursorPosition.MEASURE_AUDIO_START:
                    FontManager.DrawString(String.Format("{0:0.00}", _timeElapsed / 1000), "LargeFont", Core.Metrics["EditorMeasurementDisplay", 0], Color.Black, FontAlign.Center);
                    DrawSongTimeLine();
                    break;
                case EditorCursorPosition.DONE:
                    _editProgress = 4;
                    break;
                case EditorCursorPosition.MAIN_MENU:
                    _editProgress = 0;
                    FontManager.DrawString(VERSION,"DefaultFont",Core.Metrics["EditorVersion",0],Color.Black,FontAlign.Right);
                    break;
            }

            SetActiveMenu();
            ValidateInputs();
            DrawEditProgress();
            DrawText();
            if (_activeMenu != null)
            {
                _menus[_activeMenu].Draw();
            }
         //   Core.ShiftSpriteBatch(true);
        }

        private void DrawSongTimeLine()
        {
            _songTimeLine.Song = NewGameSong;
            _songTimeLine.AudioStart = null;
            _songTimeLine.Length = null;
            _songTimeLine.Offset = null;
            _songTimeLine.CurrentPosition = null;
            switch (_cursorPosition)
            {
                case EditorCursorPosition.MEASURE_AUDIO_START:
                    _songTimeLine.AudioStart = _timeElapsed / 1000;
                    _songTimeLine.CurrentPosition = _timeElapsed / 1000;
                    break;
                case EditorCursorPosition.MEASURE_LENGTH:
                    _songTimeLine.Length = _timeElapsed / 1000;
                    _songTimeLine.CurrentPosition = _timeElapsed / 1000;
                    break;
                case EditorCursorPosition.MEASURE_OFFSET:
                    _songTimeLine.Offset = _timeElapsed / 1000;
                    _songTimeLine.CurrentPosition = _timeElapsed / 1000;
                    break;
            }
            _songTimeLine.AudioEnd = Core.Audio.GetFileLength(NewGameSong.Path + "\\" + NewGameSong.AudioFile)/1000.0;
           
            _songTimeLine.Draw();

        }

        private void DrawBPMMeterExtras()
        {
            _validityTextBaseSprite.Draw();
            _songDetailsDisplaySprite.Draw();
            _songTypeDisplay.Song = NewGameSong;
            _songTypeDisplay.Draw();
        }

        private void DrawBPMMeasurement()
        {

            var avgAmount = _beatTimings[0] == 0 ? "-----" : string.Format("{0:F1}",60/_beatTimings[0]*1000);
            FontManager.DrawString("Last Hit: " + avgAmount, "DefaultFont", Core.Metrics["EditorBPMMeasurements", 0], Color.Black, FontAlign.Left);
             avgAmount = (_numBeats >= 5) ? String.Format("{0:F1}", 60 / _beatTimings.Take(5).Average() * 1000) : "-----" ;

            FontManager.DrawString("Last 5 avg: " + avgAmount, "DefaultFont", Core.Metrics["EditorBPMMeasurements", 1], Color.Black, FontAlign.Left);
            
            avgAmount = (_numBeats >= 10) ? String.Format("{0:F1}", 60 / _beatTimings.Take(10).Average() * 1000) : "-----";
            FontManager.DrawString("Last 10 avg: " + avgAmount, "DefaultFont", Core.Metrics["EditorBPMMeasurements", 2], Color.Black, FontAlign.Left);

            avgAmount = (_numBeats > 0)
                            ? String.Format("{0:F1}", 60/_beatTimings.Take(_numBeats).Average()*1000)
                            : "-----";
            FontManager.DrawString("Last 25 avg: "+ avgAmount, "DefaultFont", Core.Metrics["EditorBPMMeasurements", 3], Color.Black, FontAlign.Left);

            var roundedAmount = (_numBeats > 0) ? "" + Math.Round(60/ _beatTimings.Take(_numBeats).Average() * 1000) : "-----";
            FontManager.DrawString("Estimated BPM: " + roundedAmount, "TwoTech36", Core.Metrics["EditorMeasurementDisplay", 0], Color.Black, FontAlign.Center);

        }

        private void DrawText()
        {
            string instructions = "";
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SONG_BASICS:
                    var currentMenu = _menus["Basics"];
                    currentMenu.ClearMenuOptions();

                    currentMenu.GetByItemText("Select Source File").AddOption(Path.GetFileName(_sourceFilePath), null);
                    currentMenu.GetByItemText("Enter Folder Name").AddOption(_destinationFolderName.Substring(_destinationFolderName.LastIndexOf("\\") + 1), null);

                    if (!String.IsNullOrEmpty(_destinationFileName))
                    {
                        currentMenu.GetByItemText("Enter Destination File Name").AddOption(_destinationFileName, null);
                    }

                    break;
                case EditorCursorPosition.SONG_DETAILS:
                case EditorCursorPosition.SONG_DETAILS_DELETE:

                    var offsetPosition = Core.Metrics["EditorSongDetailsDisplay",0].Clone();
                    offsetPosition.X += 2;
                    offsetPosition.Y += 12;
                    var scale = FontManager.ScaleTextToFit(String.Format("{0:0.00}", NewGameSong.AudioStart),
                                                              "TwoTech24", 70, 35);
                    FontManager.DrawString(String.Format("{0:0.00}", NewGameSong.AudioStart), "TwoTech24", offsetPosition,scale, Color.Black, FontAlign.Left);
                    offsetPosition.X += 160;
                    scale = FontManager.ScaleTextToFit(String.Format("{0:0.00}", NewGameSong.Offset),
                                                              "TwoTech24", 70, 35);
                    FontManager.DrawString(String.Format("{0:0.00}", NewGameSong.Offset), "TwoTech24", offsetPosition,scale, Color.Black, FontAlign.Right);

                    if ((NewGameSong.StartBPM > 0) && (NewGameSong.Length > 0))
                    {
                        var totalBeatlines = Math.Floor(NewGameSong.GetEndingTimeInPhrase());
                        offsetPosition.Y += 25;
                        totalBeatlines = Math.Max(totalBeatlines + 1, 0);
                        FontManager.DrawString("" +totalBeatlines, "TwoTechLarge",
                                                  offsetPosition, Color.Black, FontAlign.Right);
                    }
                    
                    FontManager.DrawString(_validityMessage.Replace(".",".\n"), "DefaultFont", Core.Metrics["EditorSongValidityMessage",0],Color.Black, FontAlign.Center);
                    var backgroundText = string.IsNullOrEmpty(NewGameSong.BackgroundFile) ? "None" : NewGameSong.BackgroundFile;
                    offsetPosition = Core.Metrics["EditorSongBackgroundDisplay", 0].Clone();
                    offsetPosition.X += _songBackgroundDisplaySprite.Width -8;
                    FontManager.DrawString(backgroundText, "DefaultFont", offsetPosition, Color.Black, FontAlign.Right);

                    break;
                case EditorCursorPosition.SONG_TUNING:
                    DrawDebugText();
                    DrawTweakControlsText();
                    DrawHitOffsetText();
                    break;
                case EditorCursorPosition.MEASURE_OFFSET:
                    instructions = Core.Text["EditorMeasureOffset"];
                    break;
                case EditorCursorPosition.MEASURE_LENGTH:
                    instructions = Core.Text["EditorMeasureLength"];
                    break;
                case EditorCursorPosition.MEASURE_BPM:
                    instructions = Core.Text["EditorMeasureBPM"];
                    break;
                    case EditorCursorPosition.MEASURE_AUDIO_START:
                    instructions = Core.Text["EditorMeasureAudioStart"];
                    break;
                    case EditorCursorPosition.DONE:
                    instructions = Core.Text["EditorDoneCreating"];
                    break;
                    case EditorCursorPosition.DONE_DELETE:
                    instructions = Core.Text["EditorDoneDeleting"];
                    break;
            }
            FontManager.DrawString(instructions, "DefaultFont", Core.Metrics["EditorMeasureInstructions", 0], Color.Black, FontAlign.Center);

        }

        private void DrawHitOffsetText()
        {

            FontManager.DrawString(String.Format("Last hit: {0:F2} ms", _hitOffsets[0]), "DefaultFont", Core.Metrics["EditorHitOffset", 0], Color.Black, FontAlign.Left);
            var avg = _hitOffsets.Take(3).Average();
            if (_numHits < 3)
                return;

            FontManager.DrawString(String.Format("Last 3 avg: {0:F2} ms", avg), "DefaultFont", Core.Metrics["EditorHitOffset", 1], Color.Black, FontAlign.Left);
            if (_numHits < 5)
                return;
            avg = _hitOffsets.Take(5).Average();
            FontManager.DrawString(String.Format("Last 5 avg: {0:F2} ms", avg), "DefaultFont", Core.Metrics["EditorHitOffset", 2], Color.Black, FontAlign.Left);
            if (_numHits < 10)
                return;
            avg = _hitOffsets.Average();
            FontManager.DrawString(String.Format("Last 10 avg: {0:F2} ms", avg), "DefaultFont", Core.Metrics["EditorHitOffset", 3], Color.Black, FontAlign.Left);

        }

        private void DrawTweakControlsText()
        {

            var lines = Core.Text["EditorTweakControlsLeft"].Split('\n');

            var textPosition = Core.Metrics["EditorTweakControls", 0].Clone();
            foreach (string line in lines)
            {
                FontManager.DrawString(line, "DefaultFont", textPosition, Color.Black, FontAlign.Left);
                textPosition.Y += 20;
            }

            lines = Core.Text["EditorTweakControlsRight"].Split('\n');
            textPosition = Core.Metrics["EditorTweakControls", 1].Clone();

            foreach (string line in lines)
            {
                FontManager.DrawString(line, "DefaultFont", textPosition, Color.Black, FontAlign.Left);
                textPosition.Y += 20;
            }
        }


        private void DrawDebugText()
        {
            _textBackground.Draw();
            FontManager.DrawString(String.Format("BPM: {0:F2}", NewGameSong.StartBPM),
                   "DefaultFont", Core.Metrics["EditorSongBPM", 0], Color.Black, FontAlign.Left);
            FontManager.DrawString(String.Format("Offset: {0:F3}s", NewGameSong.Offset),
                    "DefaultFont", Core.Metrics["EditorSongOffset", 0], Color.Black, FontAlign.Left);
            FontManager.DrawString("" + String.Format("{0:F3}", _phraseNumber),
                "DefaultFont", Core.Metrics["EditorSongPhraseNumber", 0], Color.Black, FontAlign.Left);
            FontManager.DrawString(CalculateTimeLeft(),
                "DefaultFont", Core.Metrics["EditorSongTimeLeft", 0], Color.Black, FontAlign.Left);
            FontManager.DrawString(String.Format("Speed: {0}x", Core.Players[0].PlayerOptions.BeatlineSpeed),
                "DefaultFont", Core.Metrics["EditorTweakBeatlineSpeed", 0], Color.Black, FontAlign.Left);
            FontManager.DrawString(String.Format("Length: {0:F3}s", NewGameSong.Length),
                "DefaultFont", Core.Metrics["EditorSongLength", 0], Color.Black, FontAlign.Left);       
        }

        private string CalculateTimeLeft()
        {
            var timeLeft = NewGameSong.Length * 1000 - _timeElapsed;
            var ts = new TimeSpan(0, 0, 0, 0, (int)timeLeft);

            return ts.Minutes + ":" + String.Format("{0:D2}", Math.Max(0, ts.Seconds));
        }

        private void DrawHeading()
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
                    case EditorCursorPosition.SELECT_BACKGROUND_IMAGE:
                    headingText = "Select Background Image";
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
                    case EditorCursorPosition.MEASURE_AUDIO_START:
                    headingText = "Measure Audio Start";
                    break;
                case EditorCursorPosition.SONG_TUNING:
                    headingText = "Song Tuning";
                    break;
                case EditorCursorPosition.DONE:
                    headingText = "Completed";
                    break;
            }
            FontManager.DrawString(headingText, "TwoTechLarge", Core.Metrics["EditorHeading", 0], Color.Black, FontAlign.Left);
        }

        private void DrawEditProgress()
        {
            if (_editProgress == 0)
            {
                return;
            }
            _editProgressBaseSprite.Draw();

            var position = _editProgressBaseSprite.Position.Clone();
            position.X += 10;
            position.Y += 24;
            var size = new Vector2(Core.Metrics["SongEditProgress.Size", 1].X / _editProgressSpriteMap.Columns, Core.Metrics["SongEditProgress.Size", 1].Y);
            for (int x = 1; x < 5; x++)
            {
                if (_editProgress >= x)
                {
                    _editProgressSpriteMap.Draw( x + 3, size, position);
                }
                else
                {
                    _editProgressSpriteMap.Draw( x - 1, size, position);
                }
                position.X += size.X;
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

        public override void PerformAction(InputAction inputAction)
        {
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SELECT_AUDIO:
                case EditorCursorPosition.SELECT_SONGFILE:
                case EditorCursorPosition.SELECT_SONGFILE_DELETE:
                case EditorCursorPosition.SELECT_BACKGROUND_IMAGE:
                    _fileSelect.PerformAction(inputAction);
                    
                    return;
                case EditorCursorPosition.SONG_TUNING:
                    PerformActionTweak(inputAction);
                    break;
                    case EditorCursorPosition.MEASURE_BPM:
                    case EditorCursorPosition.MEASURE_LENGTH:
                    case EditorCursorPosition.MEASURE_OFFSET:
                    case EditorCursorPosition.MEASURE_AUDIO_START:
                    PerformActionMeasurement(inputAction);
                    break;
            }

            if (_activeMenu == null)
            {
                return;
            }
            
            switch (inputAction.Action)
            {
                case "LEFT":               
                case "RIGHT":                 
                case "UP":
                case "DOWN":
                    _menus[_activeMenu].HandleAction(inputAction);
                    break;
                case "START":
                    DoMenuAction();
                    break;
                case "BACK":
                    DoMenuActionBack();
                    break;
            }
        }

        private void DoMenuActionBack()
        {
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SONG_DETAILS:
                    if (_editMode)
                    {
                        _cursorPosition = EditorCursorPosition.MAIN_MENU;
                    }
                    else
                    {
                        _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    }
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;
                case EditorCursorPosition.SONG_DETAILS_DELETE:
                    _cursorPosition = EditorCursorPosition.SELECT_SONGFILE_DELETE;
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;
                    case EditorCursorPosition.SONG_BASICS:
                    _cursorPosition = EditorCursorPosition.MAIN_MENU;
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;
                    case EditorCursorPosition.MAIN_MENU:
                    Core.ScreenTransition("MainMenu");
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;
            }
        }

        private void PerformActionMeasurement(InputAction inputAction)
        {

            switch (inputAction.Action)
            {
                case "BEATLINE":
                case "START":
                    //Normally these are both 'confirm' actions, but BPM measurement has different uses
                    //for BEATLINE and START.
                    switch (_cursorPosition)
                    {
                        case EditorCursorPosition.MEASURE_BPM:
                            if (inputAction.Action == "BEATLINE")
                            {
                                UpdateBPMMeasurement();
                                return;
                            }
                            
                            if (inputAction.Action == "START")
                            {
                                if (_guessedBPM > 0)
                                {
                                    NewGameSong.StartBPM = _guessedBPM;
                                    _bpmMeter.DisplayedSong = NewGameSong;
                                }
                            }
                            break;
                           
                            case EditorCursorPosition.MEASURE_LENGTH:
                            NewGameSong.Length = Math.Round(_timeElapsed / 1000, 2);
                            break;

                            case EditorCursorPosition.MEASURE_OFFSET:
                            NewGameSong.Offset = Math.Round(_timeElapsed /1000,2);
                            break;
                            case EditorCursorPosition.MEASURE_AUDIO_START:
                            NewGameSong.AudioStart = Math.Round(_timeElapsed / 1000, 2);
                            break;
                    }

                    Core.Audio.StopChannel(_audioChannel);
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    _songPlaying = false;
                    break;

                case "LEFT":
                    //Rewind the playing song.
                    
                    AdjustSongPosition(-3000);
                    break;
                case "RIGHT":
                    //Fast forward the playing song.
                    AdjustSongPosition(3000);
                    break;
                case "BACK":
                    Core.Audio.StopChannel(_audioChannel);
                    _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    _songPlaying = false;
                    break;
            }
        }

        private void AdjustSongPosition(int amount)
        {
            var position = (int) Core.Audio.GetChannelPosition(_audioChannel);
            var max = Core.Audio.GetChannelLength(_audioChannel) - 1;
            var newPosition = Math.Min(max, Math.Max(0, position + amount));

            if (!Core.Audio.IsChannelPlaying(_audioChannel))
            {
                Core.Audio.StopChannel(_audioChannel);
                _audioChannel = Core.Audio.PlaySoundEffect(NewGameSong.Path + "\\" + NewGameSong.AudioFile, false, false);
            }

            Core.Audio.SetPosition(_audioChannel, newPosition);
        }

        private void PerformActionTweak(InputAction inputAction)
        {

            switch (inputAction.Action)
            {
                case "BEATLINE":
                    HitBeatline();
                    break;
                case "SELECT":
                    RestartSong();
                    break;
                case "BPM_DECREASE_BIG":
                    NewGameSong.StartBPM -= 0.1;
                    break;
                case "BPM_INCREASE_BIG":
                    NewGameSong.StartBPM += 0.1;
                    break;
                case "BPM_DECREASE_SMALL":
                    NewGameSong.StartBPM -= 0.01;
                    break;
                case "BPM_INCREASE_SMALL":
                    NewGameSong.StartBPM += 0.01;
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
                    Core.Songs.AddSong(Core.Songs.LoadFromFile(NewGameSong.Path + "\\" + NewGameSong.DefinitionFile, true));
                    _cursorPosition = EditorCursorPosition.DONE;
                    _songPlaying = false;
                    Core.Log.AddMessage(String.Format("SongFile '{0}' was saved successfully!", NewGameSong.DefinitionFile),LogLevel.DEBUG);
                    break;
                case "DOWN":
                    AdjustSpeed(-1);
                    break;
                case "UP":
                    AdjustSpeed(1);
                    break;
                case "LEFT":
                    AdjustSongPosition(-3000);
                    SetupBeatline();  
                    _noteJudgementSet.Reset();
                    break;
                case "RIGHT":
                    AdjustSongPosition(3000);
                    SetupBeatline();                    
                    _noteJudgementSet.Reset();
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
            var idx = Array.IndexOf(_speedOptions, Core.Players[0].PlayerOptions.BeatlineSpeed);
            idx += amount;
            idx = Math.Min(_speedOptions.Length - 1, Math.Max(0, idx));
            Core.Players[0].PlayerOptions.BeatlineSpeed = _speedOptions[idx];
            _beatlineSet.SetSpeeds();
        }

        private void RestartSong()
        {
            _startTime = null;
            _songPlaying = false;
            Core.Players[0].Streak = 0;
            Core.Songs.StopCurrentSong();
            SetupBeatline();
            _noteJudgementSet.Reset();
        }

        private void HitBeatline()
        {
            if (_beatlineSet.CalculateHitOffset(0, _phraseNumber) > 750)
            {
                return;
            }
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

            _noteJudgementSet.AwardJudgement(judgement, 0,1, 1);
        }

        #endregion
#endregion

        #region Menu Action Handlers

        private void DoMenuAction()
        {
            _menus[_activeMenu].HandleAction(new InputAction {Action = "START"});
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
                    _sourceFilePath = "";
                    _editMode = false;
                    _importMode = false;
                    break;
                case "1":
                    _cursorPosition = EditorCursorPosition.SELECT_SONGFILE;
                    ActivateEditMode(EditorCursorPosition.SONG_DETAILS);
                    break;
                case "2":
                    _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    _destinationFolderName = "";
                    _destinationFileName = "";
                    _sourceFilePath = "";
                    _editMode = false;
                    _importMode = true;
 
                    break;
                case "3":
                    _cursorPosition = EditorCursorPosition.SELECT_SONGFILE_DELETE;
                    ActivateEditMode(EditorCursorPosition.SONG_DETAILS_DELETE);
                    break;
                case "4":
                    Core.ScreenTransition("MainMenu");
                    break;
            }
        }

        private void DoMenuActionBasics(Menu menu)
        {
            switch (menu.SelectedItem().ItemValue.ToString())
            {
                case "0":
                    if (_importMode)
                    {
                        BasicsSelectImportFile();
                    }
                    else
                    {
                        BasicsSelectAudioFile();
                    }
                    break;
                case "1":
                    ActivateTextEntryMode();
                    _textEntry.DescriptionText = Core.Text["EditorEnterDestinationFolder"];
                        
                    _textEntryDestination = "SongFolder";
                    break;
                case "2":
                    ActivateTextEntryMode();
                    _textEntry.DescriptionText = Core.Text["EditorEnterDestinationFile"];                  
                    _textEntryDestination = "DefinitionFile";
                    break;
                case "3":
                    if (!_menus["Basics"].GetByItemText("Next Step").Enabled)
                    {
                        return;
                    }
                    if (!_importMode)
                    {
                        Core.Log.AddMessage(
                            String.Format("Attempting to create a basic new GameSong at: {0}\\{1}",
                                          _destinationFolderName, _destinationFileName), LogLevel.DEBUG);
                        CreateNewBasicGameSong();
                    }
                    else
                    {
                        Core.Log.AddMessage(String.Format("Attempting to import song file at: {0}",
                                          _sourceFilePath), LogLevel.DEBUG);
                        ImportGameSong();
                    }
                    _errorMessage = "";
                        _bpmMeter.DisplayedSong = NewGameSong;
                        _songTimeLine.AudioEnd = Core.Audio.GetFileLength(NewGameSong.Path + "\\" + NewGameSong.AudioFile) / 1000.0;
                        _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                    break;
                case "4":
                    _cursorPosition = EditorCursorPosition.MAIN_MENU;
                    break;
            }
        }

        private void ImportGameSong()
        {
            Core.Songs.SetProblematicOveride(true);
            NewGameSong = Core.Songs.LoadFromFile(_sourceFilePath,false);
            Core.Songs.SetProblematicOveride(false);

            var audioFileName = Path.GetFileName(NewGameSong.AudioFile);

            Core.Log.AddMessage("Creating folder for imported song: " + _wgibeatSongsFolder + "\\" + _destinationFolderName, LogLevel.DEBUG);
            if (!Directory.Exists(_wgibeatSongsFolder + "\\" + _destinationFolderName))
            {
                Directory.CreateDirectory(_wgibeatSongsFolder + "\\" + _destinationFolderName);
            }
            Core.Log.AddMessage("Copying selected audio file from: " + audioFileName, LogLevel.DEBUG);
            Core.Log.AddMessage(" ... to: " + _wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName, LogLevel.DEBUG);

            if (!File.Exists(_wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName))
            {
                File.Copy(NewGameSong.Path + "\\" + audioFileName, _wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName);
            }

            //Create gamesong file.
            //Calculate MD5 and save .sng file.
            NewGameSong.DefinitionFile = _destinationFileName;
            NewGameSong.Path = _wgibeatSongsFolder + "\\" + _destinationFolderName;
            NewGameSong.SetMD5();
            NewGameSong.ReadOnly = false;
            Core.Songs.SaveToFile(NewGameSong);

            Core.Log.AddMessage("New basic game song created successfully.", LogLevel.INFO);

        }

        private void BasicsSelectAudioFile()
        {
            _fileSelect.Patterns = new[] { "*.mp3", "*.ogg", "*.wav" };
            BasicsSelectSourceFile();

        }
        private void BasicsSelectImportFile()
        {
            _fileSelect.Patterns = new[] { "*.sm", "*.dwi"  };
            BasicsSelectSourceFile();
        }

        private void SelectSongBackground()
        {
            _fileSelect.Patterns = new[] {"*.bmp", "*.jpg", "*.jpeg", "*.png"};
            _fileSelect.ResetEvents();
            _fileSelect.FileSelected += delegate
                                            {
                                                ApplySongBackground(_fileSelect.SelectedFile);
                                                _cursorPosition = EditorCursorPosition.SONG_DETAILS;
                                            };
            _fileSelect.FileSelectCancelled += delegate { _cursorPosition = EditorCursorPosition.SONG_DETAILS; };
        }

        private void ApplySongBackground(string selectedFile)
        {
            if (selectedFile == null)
            {
                NewGameSong.BackgroundFile = null;
                return;
            }
            try
            {
            
                if (selectedFile != NewGameSong.Path + "\\" + Path.GetFileName(selectedFile))
                {
                    File.Copy(selectedFile, NewGameSong.Path + "\\" + Path.GetFileName(selectedFile));
                }
                NewGameSong.BackgroundFile = Path.GetFileName(selectedFile);
            }
            catch (Exception)
            {
                Core.Log.AddMessage("Failed to copy background image: " + selectedFile,LogLevel.WARN);

            }
           

        }

        private void BasicsSelectSourceFile()
        {
            _cursorPosition = EditorCursorPosition.SELECT_AUDIO;
            _fileSelect.CurrentFolder = Path.GetFullPath(_wgibeatSongsFolder + "\\..");
            _fileSelect.ResetEvents();
            _fileSelect.FileSelected += delegate
            {
                _sourceFilePath = _fileSelect.SelectedFile;
                _cursorPosition = EditorCursorPosition.SONG_BASICS;
            };
            _fileSelect.FileSelectCancelled += delegate { _cursorPosition = EditorCursorPosition.SONG_BASICS; };
        }

        private void DoMenuActionDetails(Menu menu)
        {
            switch ((int)menu.SelectedItem().ItemValue)
            {
                case 0:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongTitle";
                    _textEntry.DescriptionText = Core.Text["EditorEnterSongTitle"];
                    break;
                case 1:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongSubtitle";
                    _textEntry.DescriptionText = Core.Text["EditorEnterSongSubtitle"];
                    break;
                case 2:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongArtist";
                    _textEntry.DescriptionText = Core.Text["EditorEnterSongArtist"];
                    break;
                case 3:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongBPM";
                    _textEntry.DescriptionText = Core.Text["EditorEnterSongBPM"];
                    break;
                case 4:
                    _cursorPosition = EditorCursorPosition.MEASURE_BPM;
                    ActivateMeasureMode();
                    _numBeats = -1;
                    _guessedBPM = 0;
                    Core.Players[0].Streak = 0;
                    _lastHitTime = null;                 
                    break;
                case 5:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongOffset";
                    _textEntry.DescriptionText = Core.Text["EditorEnterSongOffset"];
                    break;
                case 6:
                    _cursorPosition = EditorCursorPosition.MEASURE_OFFSET;
                    ActivateMeasureMode();               
                    break;
                case 7:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongLength";
                    _textEntry.DescriptionText = Core.Text["EditorEnterSongLength"];
                    break;
                case 8:
                    _cursorPosition = EditorCursorPosition.MEASURE_LENGTH;
                    ActivateMeasureMode();
                    break;
                case 9:
                    ActivateTextEntryMode();
                    _textEntryDestination = "SongAudioStart";
                    _textEntry.DescriptionText = Core.Text["EditorEnterSongAudioStart"];
                    break;
                case 10:
                    _cursorPosition = EditorCursorPosition.MEASURE_AUDIO_START;
                    ActivateMeasureMode();
                    break;
                case 11:
                    _cursorPosition = EditorCursorPosition.SELECT_BACKGROUND_IMAGE;
                    SelectSongBackground();       
                    break;
                case 12:
                    ApplySongBackground(null);               
                    break;
                case 13:    
                    if (Core.Songs.ValidateSongFile(NewGameSong))
                    {
                        Core.Log.AddMessage(String.Format("DEBUG: Attempting to save song details to: {0}\\{1}", NewGameSong.Path, NewGameSong.DefinitionFile), LogLevel.DEBUG);
                        Core.Songs.SaveToFile(NewGameSong);
                        _cursorPosition = EditorCursorPosition.SONG_TUNING;
                        _startTime = null;
                        _songPlaying = false;
                        SetupBeatline();


                        _noteJudgementSet.Reset();
                        _numHits = 0;
              
                    }
                    break;
                case 14:
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

        private void SetupBeatline()
        {
            Core.Cookies["CurrentGameType"] = GameType.NORMAL;
            _beatlineSet.Reset();
            _beatlineSet.EndingPhrase = NewGameSong.GetEndingTimeInPhrase();
            _beatlineSet.Bpm = NewGameSong.StartBPM;
            _beatlineSet.AddTimingPointMarkers(NewGameSong);
            _beatlineSet.SetSpeeds();
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
                   if (_cursorPosition == EditorCursorPosition.DONE_DELETE )
                   {
                       _fileSelect.RefreshFolder();
                       _cursorPosition = EditorCursorPosition.SELECT_SONGFILE_DELETE;
                   }
                   else
                   {
                       Core.ScreenTransition("MainMenu");
                   }
                   
                   break;
                   
           }

        }

        #endregion

        #region Misc Helpers

        private void CreateNewBasicGameSong()
        {
            var audioFileName = Path.GetFileName(_sourceFilePath);
            
            Core.Log.AddMessage("Creating folder for new song: " + _wgibeatSongsFolder + "\\" + _destinationFolderName, LogLevel.DEBUG);
            if (!Directory.Exists(_wgibeatSongsFolder + "\\" + _destinationFolderName))
            {
                Directory.CreateDirectory(_wgibeatSongsFolder + "\\" + _destinationFolderName);
            }
            Core.Log.AddMessage("Copying selected audio file from: " + _sourceFilePath, LogLevel.DEBUG);
            Core.Log.AddMessage(" ... to: " + _wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName, LogLevel.DEBUG);

            if (!File.Exists(_wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName))
            {
                File.Copy(_sourceFilePath, _wgibeatSongsFolder + "\\" + _destinationFolderName + "\\" + audioFileName);
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

            var tags = Core.Audio.GetAudioFileMetadata(_sourceFilePath);
            var titleTag = "";
            if (tags.ContainsKey("TITLE") && !string.IsNullOrEmpty(tags["TITLE"]))
            {
                titleTag = tags["TITLE"];
            }
            if (tags.ContainsKey("TIT2") && !string.IsNullOrEmpty(tags["TIT2"]))
            {
                titleTag = tags["TIT2"];
            }
            NewGameSong.Title = titleTag.Trim();

            SongManager.SplitTitle(NewGameSong);

            if (tags.ContainsKey("ARTIST") && !string.IsNullOrEmpty(tags["ARTIST"]))
            {
                NewGameSong.Artist = tags["ARTIST"].Trim();
            }
            if (tags.ContainsKey("TPE1") && !string.IsNullOrEmpty(tags["TPE1"]))
            {
                NewGameSong.Artist = tags["TPE1"].Trim();
            }
            NewGameSong.Length = Convert.ToInt32(tags["LENGTH"]) / 1000.0;

            double testBPM;
            if (tags.ContainsKey("TBPM") && Double.TryParse(tags["TBPM"],out testBPM))
            {
                NewGameSong.StartBPM = testBPM;
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
                        (!String.IsNullOrEmpty(_sourceFilePath)) &&
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

            //Only play the last 15 seconds of the song when measuring the length of the song.
            var songLength = Core.Audio.GetChannelLength(_audioChannel);
            double desiredStartPoint = 0;

            switch (_cursorPosition)
            {
                case EditorCursorPosition.MEASURE_LENGTH:
                    desiredStartPoint = songLength - 15000;
                    break;
                case EditorCursorPosition.MEASURE_BPM:
                    desiredStartPoint = NewGameSong.Offset * 1000;
                    break;
                    case EditorCursorPosition.MEASURE_OFFSET:
                    desiredStartPoint = NewGameSong.AudioStart * 1000;
                    break;
            }
  
                if (songLength > desiredStartPoint)
                {
                    _timeElapsed += desiredStartPoint;
                    Core.Audio.SetPosition(_audioChannel, desiredStartPoint);
                }
        
        }

        private void ActivateEditMode(EditorCursorPosition position)
        {
            _fileSelect.Patterns = new[] { "*.sng","*.sm","*.dwi" };
            _fileSelect.CurrentFolder = _wgibeatSongsFolder;
            _fileSelect.ResetEvents();
            _fileSelect.FileSelected += delegate
            {
                NewGameSong = Core.Songs.LoadFromFile(_fileSelect.SelectedFile, false);
                _bpmMeter.DisplayedSong = NewGameSong;
                _songTimeLine.AudioEnd = Core.Audio.GetFileLength(NewGameSong.Path + "\\" + NewGameSong.AudioFile) / 1000.0;
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
                _songLoadDelay = 0;
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
            if (Math.Abs(delay) > 20)
            {
                _songLoadDelay += delay / 2.0;
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
            DONE_DELETE,
            MEASURE_AUDIO_START,
            SELECT_BACKGROUND_IMAGE
        }
    }
}
