using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class SongEditorScreen : GameScreen
    {

        private readonly Dictionary<string, Menu> _menus = new Dictionary<string, Menu>();
        private readonly FileSelectDialog _fileSelect = new FileSelectDialog();
        private readonly TextEntry _textEntry = new TextEntry {Width = 800, Height = 600};
        private bool _keyInput;
        private bool _ignoreNextKey;
        private EditorCursorPosition _cursorPosition;

        private string _audioFilePath = "";
        private string _destinationFileName = "";
        private string _destinationFolderName = "";

        private Sprite _backgroundSprite;

        private string _activeMenu;
        private int _editProgress;
        public GameSong NewGameSong;
        private string _textEntryDestination;
        private Sprite _editProgressBaseSprite;
        private SpriteMap _editProgressSpriteMap;
        private BpmMeter _bpmMeter;


        public SongEditorScreen(GameCore core) : base(core)
        {
            CreateMenus();
            InitSprites();
            InitObjects();
            
        }

        private void InitObjects()
        {
            _bpmMeter = new BpmMeter();
            _bpmMeter.Position = (Core.Metrics["EditorBPMMeter", 0]);
        }

        public void InitSprites()
        {
            _backgroundSprite = new Sprite() {SpriteTexture = TextureManager.Textures["allBackground"]};
            _editProgressBaseSprite = new Sprite() {SpriteTexture = TextureManager.Textures["SongEditProgressBase"]};
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
            mainMenu.AddItem(new MenuItem {ItemText = "Create New Song", ItemValue = 0});
            mainMenu.AddItem(new MenuItem { ItemText = "Edit Existing Song", ItemValue = 1 });
            mainMenu.AddItem(new MenuItem { ItemText = "Exit", ItemValue = 2 });
            _menus.Add("Main",mainMenu);

            var basicsMenu = new Menu {Width = 800, Position = Core.Metrics["EditorMenuStart",0]};
            basicsMenu.AddItem(new MenuItem {ItemText = "Select Audio File", ItemValue = 0});
            basicsMenu.AddItem(new MenuItem { ItemText = "Enter Folder Name", ItemValue = 1 });
            basicsMenu.AddItem(new MenuItem {ItemText = "Enter Destination File Name", ItemValue = 2});
            basicsMenu.AddItem(new MenuItem { ItemText = "Next Step", ItemValue = 3, Enabled = false });
            basicsMenu.AddItem(new MenuItem {ItemText = "Back", ItemValue = 4});
            _menus.Add("Basics", basicsMenu);

            var detailsMenu = new Menu() {Width = 400, Position = Core.Metrics["EditorMenuStart", 0]};
            detailsMenu.AddItem(new MenuItem{ItemText = "Enter Title",ItemValue = 0});
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Subtitle", ItemValue = 1 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Artist", ItemValue = 2 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter BPM Manually", ItemValue = 3 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure BPM", ItemValue = 4 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Offset Manually", ItemValue = 5 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure Offset", ItemValue = 6 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Enter Length Manually", ItemValue = 7 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Measure Length", ItemValue = 8 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Next Step", ItemValue = 9 });
            detailsMenu.AddItem(new MenuItem { ItemText = "Back", ItemValue = 10 });
            _menus.Add("Details",detailsMenu);

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
                    if (Double.TryParse(_textEntry.EnteredText,out temp))
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
                    break;
                case EditorCursorPosition.SONG_TWEAKING:
                    _editProgress = 3;
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
                    currentMenu.GetByItemText("Enter Folder Name").AddOption(_destinationFolderName.Substring(_destinationFolderName.LastIndexOf("\\") + 1),null);           
              
                    if (!String.IsNullOrEmpty(_destinationFileName))
                    {
                        currentMenu.GetByItemText("Enter Destination File Name").AddOption(_destinationFileName, null);  
                    }
                    
                    break;
                    case EditorCursorPosition.SONG_DETAILS:
            //TODO: Complete
                    break;
            }
        }

        private void ValidateInputs()
        {
            _menus["Basics"].GetByItemText("Next Step").Enabled = (!String.IsNullOrEmpty(_destinationFileName)) &&
                                                                  (!String.IsNullOrEmpty(_destinationFolderName)) &&
                                                                  (!String.IsNullOrEmpty(_audioFilePath));
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
            TextureManager.DrawString(spriteBatch,headingText,"TwoTechLarge",Core.Metrics["EditorHeading",0],Color.Black, FontAlign.LEFT );
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
                    _editProgressSpriteMap.Draw(spriteBatch,x+3,196,55,position);
                }
                else
                {
                    _editProgressSpriteMap.Draw(spriteBatch,x-1,196,55,position);
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
            }
        }

        public override void PerformAction(Action action)
        {
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SELECT_AUDIO:
                    _fileSelect.PerformAction(action);
                    return;
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
            }
        }

        private void DoMenuActionBasics(Menu menu)
        {
            switch (menu.SelectedItem().ItemValue.ToString())
            {
                case "0":
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
            switch ((int) menu.SelectedItem().ItemValue)
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
                    //_activeMenu = "SelectSongFile";
                    break;
                case "2":
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

        private enum EditorCursorPosition
        {
            MAIN_MENU,
            KEY_ENTRY,
            SELECT_AUDIO,
            SONG_BASICS,
            SONG_DETAILS,
            SONG_TWEAKING,
            DONE
        }
    }
}
