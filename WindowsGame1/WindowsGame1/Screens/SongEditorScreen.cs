using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private FileSelectDialog _fileSelect = new FileSelectDialog();
        private bool _keyInput = false;
        private EditorCursorPosition _cursorPosition;

        private string _activeMenu;

        public GameSong GameSong;
        public SongEditorScreen(GameCore core) : base(core)
        {
            CreateMenus();
        }

        private void CreateMenus()
        {
            var mainMenu = new Menu{Width = 800};
            mainMenu.AddItem(new MenuItem {ItemText = "Create New Song", ItemValue = 0});
            mainMenu.AddItem(new MenuItem { ItemText = "Edit Existing Song", ItemValue = 1 });
            mainMenu.AddItem(new MenuItem { ItemText = "Exit", ItemValue = 2 });
            _menus.Add("Main",mainMenu);
            _fileSelect.Width = 800;
            _fileSelect.CurrentFolder = Directory.GetCurrentDirectory();
        }

        public override void Initialize()
        {
            _cursorPosition = EditorCursorPosition.MAIN_MENU;
            base.Initialize();
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            switch (_cursorPosition)
            {
                case EditorCursorPosition.SELECT_AUDIO:
                    _fileSelect.Draw(spriteBatch);
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

        private void DoMenuActionMain(Menu menu)
        {
            switch (menu.SelectedItem().ItemValue.ToString())
            {
                case "0":
                    _cursorPosition = EditorCursorPosition.SELECT_AUDIO;
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
