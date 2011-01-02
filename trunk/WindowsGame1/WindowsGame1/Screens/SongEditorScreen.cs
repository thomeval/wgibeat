using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
        private bool _keyInput = false;
        private EditorCursorPosition _cursorPosition;

        private string _audioFilePath = "";
        private string _destinationFileName = "";
        private string _destinationFolderName = "";

        private Sprite _backgroundSprite;

        private string _activeMenu;

        public GameSong GameSong;
        private string _textEntryDestination;


        public SongEditorScreen(GameCore core) : base(core)
        {
            CreateMenus();
            InitSprites();
            
        }

        public void InitSprites()
        {
            _backgroundSprite = new Sprite() {SpriteTexture = TextureManager.Textures["allBackground"]};
        }
        
        private void CreateMenus()
        {
            var mainMenu = new Menu{Width = 800};
            mainMenu.AddItem(new MenuItem {ItemText = "Create New Song", ItemValue = 0});
            mainMenu.AddItem(new MenuItem { ItemText = "Edit Existing Song", ItemValue = 1 });
            mainMenu.AddItem(new MenuItem { ItemText = "Exit", ItemValue = 2 });
            _menus.Add("Main",mainMenu);

            var basicsMenu = new Menu {Width = 800};
            basicsMenu.AddItem(new MenuItem {ItemText = "Select Audio File", ItemValue = 0});
            basicsMenu.AddItem(new MenuItem { ItemText = "Enter Folder Name", ItemValue = 1 });
            basicsMenu.AddItem(new MenuItem {ItemText = "Enter Destination File Name", ItemValue = 2});
            basicsMenu.AddItem(new MenuItem { ItemText = "Next Step", ItemValue = 3 });
            basicsMenu.AddItem(new MenuItem {ItemText = "Back", ItemValue = 4});

            _menus.Add("Basics", basicsMenu);
            _fileSelect.Width = 800;
            
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
            }
            _keyInput = false;
        }

        private void TextEntryEntryComplete(object sender, EventArgs e)
        {
            switch (_textEntryDestination)
            {
                case "SongFile":
                    _destinationFolderName = _textEntry.EnteredText;
                    _cursorPosition = EditorCursorPosition.SONG_BASICS;
                    break;
                case "SongFolder":
                    _destinationFolderName = _textEntry.EnteredText;
                    _cursorPosition = EditorCursorPosition.SONG_BASICS;
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
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SELECT_AUDIO:
                    _fileSelect.Draw(spriteBatch);
                    break;
                    case EditorCursorPosition.KEY_ENTRY:
                    _textEntry.Draw(spriteBatch);
                    break;
            }

            SetActiveMenu();
            if (_activeMenu != null)
            {
                _menus[_activeMenu].Draw(spriteBatch);
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
                case "SelectAudio":
                    break;
                case "SongInfo":
                    break;
                case "SongArea":
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
                    _keyInput = true;
                    _textEntry.Clear();
                    _cursorPosition = EditorCursorPosition.KEY_ENTRY;
                    _textEntry.DescriptionText =
                        "Enter the name of the folder where this song will be stored. It will be created if it doesn't exist.";
                    _textEntryDestination = "SongFolder";
                    break;
                case "2":
                    _keyInput = true;
                    _textEntry.Clear();
                    _cursorPosition = EditorCursorPosition.KEY_ENTRY;
                    _textEntry.DescriptionText =
                        "Enter the name of the song file (.sng) that will be created. The name has no effect on gameplay.";
                    _textEntryDestination = "SongFile";
                    break;
                case "3":

                    break;
                case "4":
                    _cursorPosition = EditorCursorPosition.MAIN_MENU;
                    break;
            }
        }

        private void DoMenuActionMain(Menu menu)
        {
            switch (menu.SelectedItem().ItemValue.ToString())
            {
                case "0":
                    GameSong = new GameSong();
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

        public override void PerformKey(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (!_keyInput)
            {
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
            SONG_INFO,
            SONG_TWEAKING
        }
    }
}
