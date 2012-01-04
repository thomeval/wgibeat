using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;

namespace WGiBeat.Screens
{
    public class MainMenuScreen : GameScreen
    {
        private MainMenuOption _selectedMenuOption;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();

        private bool _displayNoSongsError;
        private readonly string[] _menuText = { "Start Game", "Stats","How to play", "Keys", "Options", "Song Editor", "Website", "Exit"};
        private Sprite _background;
        private Sprite _header;
        private SpriteMap _menuOptionSprite;
        private Sprite _foreground;
        private UpdaterFrame _updaterFrame;
        private string _website = "http://wgibeat.googlecode.com/";
        private Thread _updateThread;


        public MainMenuScreen(GameCore core)
            : base(core)
        {
        }

        public override void Initialize()
        {
            _updaterFrame = new UpdaterFrame
            {
                Position = Core.Metrics["UpdaterFrame", 0],
                Status = UpdaterStatus.DISABLED
            };

            InitSprites();
            InitUpdater();

            base.Initialize();
        }

        private void InitUpdater()
        {
            if (!Core.Settings.Get<bool>("CheckForUpdates"))
            {
                return;
            }

            _updateThread = new Thread(RunUpdater) {Name = "Updater"};
            _updateThread.Start();
            
        }

        private void RunUpdater()
        {
            _updaterFrame.Status = UpdaterStatus.CHECKING;
            Core.UpdateManager.UpdateInfoAvailable += UpdateInfoAvailable;
            Core.UpdateManager.UpdateInfoFailed += UpdateInfoFailed;
            Core.UpdateManager.GetLatestVersion();
        }

        private void UpdateInfoFailed(object sender, EventArgs e)
        {
            _updaterFrame.Status = UpdaterStatus.FAILED;
            _updaterFrame.NewsMessage = Core.UpdateManager.ErrorMessage;
        }

        private void UpdateInfoAvailable(object sender, EventArgs e)
        {
            _updaterFrame.NewsMessage = Core.UpdateManager.NewsFeed;
            _updaterFrame.AvailableVersion = Core.UpdateManager.LatestVersion;
            _updaterFrame.CurrentVersion = GameCore.VERSION_STRING.Substring(1);
            _updaterFrame.Status = UpdaterStatus.SUCCESSFUL;
        }

        private void InitSprites()
        {
            _background = new Sprite
                              {
                                  Height = 600,
                                  Width = 800,
                                  SpriteTexture = TextureManager.Textures("MainMenuBackground"),
                              };
            _foreground = new Sprite
                              {
                                  SpriteTexture = TextureManager.Textures("MainMenuForeground"),
                                  Height = 600,
                                  Width = 800,
                              };
            _header = new Sprite
                          {
                              SpriteTexture = TextureManager.Textures("MainMenuHeader")
                          };
            _menuOptionSprite = new SpriteMap
                                    {
                                        SpriteTexture = TextureManager.Textures("MainMenuOption"),
                                        Columns = 1,
                                        Rows = 2
                                    };

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch,gameTime);
            DrawMenu(spriteBatch);

            if (_displayNoSongsError)
            {
                TextureManager.DrawString(spriteBatch,"Error: No songs loaded","DefaultFont", Core.Metrics["MainMenuNoSongsError", 0], Color.Black,FontAlign.LEFT);
            }
            DrawUpdater(spriteBatch);

        }

        private void DrawUpdater(SpriteBatch spriteBatch)
        {
            _updaterFrame.Visible = Core.Settings.Get<bool>("CheckForUpdates");
            _updaterFrame.Draw(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch,GameTime gameTime)
        {
            _background.Draw(spriteBatch);
            _field.Draw(spriteBatch, gameTime);                                
            _foreground.Draw(spriteBatch);        
        }

        private void DrawMenu(SpriteBatch spriteBatch)
        {
      

            _header.Draw(spriteBatch);
            
            for (int menuOption = 0; menuOption < (int)MainMenuOption.COUNT; menuOption++)
            {

                var idx = (menuOption == (int) _selectedMenuOption) ? 1 : 0;
                _menuOptionSprite.Draw(spriteBatch,idx,Core.Metrics["MainMenuOptions",menuOption]);
                var textPosition = Core.Metrics["MainMenuOptions", menuOption].Clone();
                textPosition.X +=  _menuOptionSprite.SpriteTexture.Width / 2 - 0 ;
                textPosition.Y += _menuOptionSprite.SpriteTexture.Height / 4 - 25;
                TextureManager.DrawString(spriteBatch,_menuText[menuOption],"TwoTech36",textPosition,Color.Black, FontAlign.CENTER);
            }
        }

        public override void PerformAction(InputAction inputAction)
        {
            int newOptionValue;

            switch (inputAction.Action)
            {
                case "UP":
                    newOptionValue = (int)_selectedMenuOption - 1;
                    if (newOptionValue < 0)
                    {
                        newOptionValue += (int)MainMenuOption.COUNT;
                    }
                    _selectedMenuOption = (MainMenuOption)newOptionValue;
                    RaiseSoundTriggered(SoundEvent.MAIN_MENU_SELECT_UP);
                    break;
                case "DOWN":
                    newOptionValue = (int)_selectedMenuOption + 1;
                    newOptionValue %= (int)MainMenuOption.COUNT;
                    _selectedMenuOption = (MainMenuOption)newOptionValue;
                    RaiseSoundTriggered(SoundEvent.MAIN_MENU_SELECT_DOWN);
                    break;
                case "START":
                     MenuOptionSelected(inputAction.Player - 1);
                    break;
                case "BEATLINE":
                    MenuOptionSelected(inputAction.Player - 1);
                    break;
                case "BACK":
                    Core.Exit();
                    break;
            }
        }

        private void MenuOptionSelected(int player)
        {
            RaiseSoundTriggered(SoundEvent.MAIN_MENU_DECIDE);
            switch (_selectedMenuOption)
            {
                case MainMenuOption.START_GAME:
                    if (Core.Songs.Songs.Count > 0)
                    {
                        Core.Cookies["JoiningPlayer"] =  player;
                        Core.ScreenTransition("NewGame");
                    }
                    else
                    {
                        _displayNoSongsError = true;
                    }
                    break;
                    /*
                case MainMenuOption.NETPLAY:
                    if (Core.Settings.Get<bool>("AllowPDA"))
                    {
                        Core.ScreenTransition("Net"); 
                    }
                    break;
                     */
                case MainMenuOption.STATS:
                    Core.ScreenTransition("Stats");
                    break;
                case MainMenuOption.HOW_TO_PLAY:
                    Core.ScreenTransition("Instruction");
                    break;
                case MainMenuOption.KEYS:
                    Core.ScreenTransition("KeyOptions");
                    break;
                case MainMenuOption.OPTIONS:
                    Core.ScreenTransition("Options");
                    break;
                    case MainMenuOption.SONG_EDIT:
                    Core.ScreenTransition("SongEdit");
                    break;
                case MainMenuOption.WEBSITE:
                    System.Diagnostics.Process.Start(_website);
                    break;

                case MainMenuOption.EXIT:
                    Core.Exit();
                    break;
            }
        }
    }

    public enum MainMenuOption
    {
        START_GAME = 0,
        STATS = 1,
        HOW_TO_PLAY = 2,
        KEYS = 3,
        OPTIONS = 4,
        SONG_EDIT = 5,
        WEBSITE = 6,
        EXIT = 7,
        NETPLAY = 8,
        COUNT = 8,
    }

}
