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
                Size = Core.Metrics["UpdaterFrame.Size",0],
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
                                  Size = Core.Metrics["ScreenBackground.Size",0],
                                  Texture = TextureManager.Textures("MainMenuBackground"),
                              };
            _foreground = Core.Metrics.SetupFromMetrics("MainMenuForeground", 0);
            _header = new Sprite3D
                          {
                              Position = Core.Metrics["MainMenuHeader", 0],
                              Size = Core.Metrics["MainMenuHeader.Size", 0],
                              Texture = TextureManager.Textures("MainMenuHeader")
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
            DrawMenu();

            FontManager.DrawString(_errorMessage,"DefaultFont", Core.Metrics["MainMenuNoSongsError", 0], Color.Black,FontAlign.Left);
            DrawUpdater();

        }

        private void DrawUpdater()
        {
            _updaterFrame.Visible = Core.Settings.Get<bool>("CheckForUpdates");
            _updaterFrame.Draw();
        }

        private void DrawBackground(GameTime gameTime)
        {
            _background.Draw();
            _field.Draw(gameTime);                                
            _foreground.Draw();
            _header.Draw();
        }

        private void DrawMenu()
        {

            for (int menuOption = 0; menuOption < (int)MainMenuOption.Count; menuOption++)
            {
                var size = menuOption == 0
                  ? Core.Metrics["MainMenuOptions.Size", 1]
                  : Core.Metrics["MainMenuOptions.Size", 0];
                var idx = (menuOption == (int) _selectedMenuOption) ? 1 : 0;
                
                VertexPositionColorTexture[] result = _menuOptionSprite.GetVertices(idx, Core.Metrics["MainMenuOptions", menuOption],size);
                result.CopyTo(_vertices,0,menuOption*6);
               

            }

            _menuOptionSprite.DrawVertices(_vertices);
            for (int menuOption = 0; menuOption < (int) MainMenuOption.Count; menuOption++)
            {
                var textPosition = Core.Metrics["MainMenuOptions", menuOption].Clone();
                var size = menuOption == 0
                       ? Core.Metrics["MainMenuOptions.Size", 1]
                       : Core.Metrics["MainMenuOptions.Size", 0];
                textPosition.X += size.X / 2;
                textPosition.Y += size.Y / 2 - 25;
                FontManager.DrawString(_menuText[menuOption], "TwoTech36", textPosition, Color.Black, FontAlign.Center);
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
            newOptionValue %= (int) MainMenuOption.Count;
            if (newOptionValue < 0)
            {
                newOptionValue += (int) MainMenuOption.Count;
            }
            _selectedMenuOption = (MainMenuOption) newOptionValue;
        
        }

        private void MenuOptionSelected(int player)
        {
            RaiseSoundTriggered(SoundEvent.MAIN_MENU_DECIDE);
            switch (_selectedMenuOption)
            {
                case MainMenuOption.StartGame:
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
                case MainMenuOption.Stats:
                    Core.ScreenTransition("Stats");
                    break;
                case MainMenuOption.HowToPlay:
                    Core.ScreenTransition("Instruction");
                    break;
                case MainMenuOption.Keys:
                    Core.ScreenTransition("KeyOptions");
                    break;
                case MainMenuOption.Options:
                    Core.ScreenTransition("Options");
                    break;
                    case MainMenuOption.SongEdit:
                    Core.ScreenTransition("SongEdit");
                    break;
                    case MainMenuOption.Credits:
                    Core.ScreenTransition("Credits");
                    break;
                case MainMenuOption.Website:
                    var thread = new Thread(LaunchBrowser);
                    thread.Start();
          
                    
                    break;

                case MainMenuOption.Exit:
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
                Core.Log.AddMessage(ex.Message, LogLevel.WARN);
                Core.Log.AddException(ex);
            }
        }
    }

    public enum MainMenuOption
    {
        StartGame = 0,
        Stats = 1,
        HowToPlay = 2,
        Keys = 3,
        Options = 4,
        SongEdit = 5,
        Website = 6,
        Credits = 7,
        Exit = 8,
        Netplay = 9,
        Count = 9,
    }

}
