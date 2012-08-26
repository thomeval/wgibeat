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


        private readonly string[] _menuText = { "Start Game", "Stats","How to play", "Keys", "Options", "Song Editor", "Website", "Credits", "Exit"};
        private Sprite3D _background;
        private Sprite3D _header;
        private SpriteMap3D _menuOptionSprite;
        private Sprite3D _foreground;
        private UpdaterFrame _updaterFrame;
        private const string WEBSITE = "http://code.google.com/p/wgibeat/?lol=orz";
        private string _errorMessage = "";
        private Thread _updateThread;

        private VertexPositionColorTexture[] _vertices;
        public MainMenuScreen(GameCore core)
            : base(core)
        {
        }

        public override void Initialize()
        {
            _vertices = new VertexPositionColorTexture[_menuText.Length*6];
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
            _updaterFrame.UpdateDetails = Core.UpdateManager.ErrorMessage;
        }

        private void UpdateInfoAvailable(object sender, EventArgs e)
        {
            _updaterFrame.UpdateDetails = Core.UpdateManager.UpdateDetails;
            _updaterFrame.NewsMessage = Core.UpdateManager.NewsFeed;  
            _updaterFrame.AvailableVersion = Core.UpdateManager.LatestVersion;
            _updaterFrame.CurrentVersion = GameCore.VERSION_STRING.Substring(1);
            _updaterFrame.Status = UpdaterStatus.SUCCESSFUL;
        }

        private void InitSprites()
        {
            _background = new Sprite3D
                              {
                                  Height = 600,
                                  Width = 800,
                                  Texture = TextureManager.Textures("MainMenuBackground"),
                              };
            _foreground = new Sprite3D
                              {
                                  Texture = TextureManager.Textures("MainMenuForeground"),
                                  Height = 600,
                                  Width = 800,
                              };
            _header = new Sprite3D
                          {
                              Texture = TextureManager.Textures("MainMenuHeader"),
                              Position = Core.Metrics["ScreenHeader",0],
                              Size = Core.Metrics["ScreenHeader.Size",0]
                          };
            _menuOptionSprite = new SpriteMap3D
                                    {
                                        Texture = TextureManager.Textures("MainMenuOption"),
                                        Columns = 1,
                                        Rows = 2
                                    };
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
          
            DrawBackground(gameTime);
            DrawMenu(spriteBatch);

            TextureManager.DrawString(spriteBatch,_errorMessage,"DefaultFont", Core.Metrics["MainMenuNoSongsError", 0], Color.Black,FontAlign.LEFT);
            DrawUpdater(spriteBatch);

        }

        private void DrawUpdater(SpriteBatch spriteBatch)
        {
            _updaterFrame.Visible = Core.Settings.Get<bool>("CheckForUpdates");
            _updaterFrame.Draw(spriteBatch);
        }

        private void DrawBackground(GameTime gameTime)
        {
            _background.Draw();
            _field.Draw(gameTime);                                
            _foreground.Draw();
            _header.Draw();
        }

        private void DrawMenu(SpriteBatch spriteBatch)
        {

            for (int menuOption = 0; menuOption < (int)MainMenuOption.COUNT; menuOption++)
            {
                var size = menuOption == 0
                  ? Core.Metrics["MainMenuOptions.Size", 1]
                  : Core.Metrics["MainMenuOptions.Size", 0];
                var idx = (menuOption == (int) _selectedMenuOption) ? 1 : 0;
                
                VertexPositionColorTexture[] result = _menuOptionSprite.GetVertices(idx, Core.Metrics["MainMenuOptions", menuOption],size);
                result.CopyTo(_vertices,0,menuOption*6);
               

            }

            _menuOptionSprite.DrawVertices(_vertices);
            for (int menuOption = 0; menuOption < (int) MainMenuOption.COUNT; menuOption++)
            {
                var textPosition = Core.Metrics["MainMenuOptions", menuOption].Clone();
                var size = menuOption == 0
                       ? Core.Metrics["MainMenuOptions.Size", 1]
                       : Core.Metrics["MainMenuOptions.Size", 0];
                textPosition.X += size.X / 2;
                textPosition.Y += size.Y / 2 - 25;
                TextureManager.DrawString(spriteBatch, _menuText[menuOption], "TwoTech36", textPosition, Color.Black, FontAlign.CENTER);
            }

        }

        public override void PerformAction(InputAction inputAction)
        {
  

            switch (inputAction.Action)
            {
                case "UP":
                    AdjustMenuOption(-1);
                    
                    RaiseSoundTriggered(SoundEvent.MAIN_MENU_SELECT_UP);
                    break;
                case "DOWN":
                    AdjustMenuOption(1);
                    RaiseSoundTriggered(SoundEvent.MAIN_MENU_SELECT_DOWN);
                    break;
                case "LEFT":
                    if (_selectedMenuOption != 0)
                    {
                        AdjustMenuOption(-4);
                    }
                    RaiseSoundTriggered(SoundEvent.MAIN_MENU_SELECT_UP);
                    break;
                case "RIGHT":
                    if (_selectedMenuOption != 0)
                    {
                        AdjustMenuOption(4);
                    }
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

        private void AdjustMenuOption(int value)
        {
            
            var newOptionValue = (int) _selectedMenuOption + value;
            newOptionValue %= (int) MainMenuOption.COUNT;
            if (newOptionValue < 0)
            {
                newOptionValue += (int) MainMenuOption.COUNT;
            }
            _selectedMenuOption = (MainMenuOption) newOptionValue;
        
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
                        _errorMessage = "";
                    }
                    else
                    {
                        _errorMessage = "Error: No songs loaded.";
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
                    case MainMenuOption.CREDITS:
                    Core.ScreenTransition("Credits");
                    break;
                case MainMenuOption.WEBSITE:
                    var thread = new Thread(LaunchBrowser);
                    thread.Start();
          
                    
                    break;

                case MainMenuOption.EXIT:
                    Core.Exit();
                    break;
            }
        }

        private void LaunchBrowser()
        {
            try
            {
                System.Diagnostics.Process.Start(WEBSITE);
            }
            catch (Exception ex)
            {
                _errorMessage = "Error: Failed to launch browser.";
                Core.Log.AddMessage(ex.Message, LogLevel.ERROR);
                Core.Log.AddException(ex);
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
        CREDITS = 7,
        EXIT = 8,
        NETPLAY = 9,
        COUNT = 9,
    }

}
