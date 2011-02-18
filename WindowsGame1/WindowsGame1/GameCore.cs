using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.Players;
using WGiBeat.Screens;

namespace WGiBeat
{
    /// <summary>
    /// Represents the Core of the game. The main game loop happens here, and flows to the currently active
    /// GameScreen. Also contains game settings, metrics, song database and players for universal access.
    /// </summary>
    public class GameCore : Game
    {
        public readonly GraphicsDeviceManager GraphicsManager;
        private SpriteBatch _spriteBatch;
        public SettingsManager Settings;
        public MetricsManager Metrics;
        public ProfileManager Profiles;

        public AudioManager Audio;
        public SongManager Songs;
        public HighScoreManager HighScores;
        public CrossfaderManager Crossfader;
        public CPUManager CPUManager;
        public LogManager Log;
        public TextManager Text;
        public KeyMappings KeyMappings; //Changed to public, for GameScreen access.

        public Player[] Players;
        public Dictionary<string, object> Cookies;

        private Dictionary<string, GameScreen> _screens;
        private GameScreen _activeScreen;
        private MenuMusicManager _menuMusicManager; 
        

        private KeyboardState _lastKeystate;
        private GamePadState[] _lastGamePadState;
        public string WgibeatRootFolder;
        private bool _drawInProgress;

        public const string VERSION_STRING = "v0.65";
        public GameCore()
        {
            GraphicsManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        #region Initialization

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _lastGamePadState = new GamePadState[4];

            //For some reason XNA has a bug when using FixedTimeSteps, which is enabled by default.
            //Using this causes 100% CPU usage (one core) and a frame rate drop.
            this.IsFixedTimeStep = false;
            //NOTE: Uncomment to disable vsync.
            //GraphicsManager.SynchronizeWithVerticalRetrace = false;

            WgibeatRootFolder = Path.GetDirectoryName(
Assembly.GetAssembly(typeof(GameCore)).CodeBase);
            WgibeatRootFolder = WgibeatRootFolder.Replace("file:\\", "");

            InitManagers();
            LoadCurrentTheme();
        
            SetGraphicsSettings();
            InitPlayers();
            
            base.Initialize();
        }

        private void InitManagers()
        {
            Log = new LogManager { Enabled = true, SaveLog = true, RootFolder = WgibeatRootFolder, LogLevel = LogLevel.INFO };
            Log.AddMessage("Initializing Cookies...", LogLevel.INFO);
            Cookies = new Dictionary<string, object>();

            TextureManager.Log = Log;
            TextureManager.GraphicsDevice = this.GraphicsDevice;

            Metrics = new MetricsManager { Log = this.Log };
            Settings = SettingsManager.LoadFromFile(WgibeatRootFolder + "\\settings.txt", this.Log);
            Log.LogLevel = (LogLevel)Settings.Get<int>("LogLevel");
            HighScores = HighScoreManager.LoadFromFile(WgibeatRootFolder + "\\Scores.conf", this.Log);
            Profiles = ProfileManager.LoadFromFolder(WgibeatRootFolder + "\\Profiles", this.Log);
            Text = TextManager.LoadFromFile(WgibeatRootFolder + "\\Content\\Text\\OptionText.txt", this.Log);
            Text.AddResource(WgibeatRootFolder + "\\Content\\Text\\EditorText.txt");
            Text.AddResource(WgibeatRootFolder + "\\Content\\Text\\ModeText.txt");

            Audio = new AudioManager(this.Log)
            {
                FallbackSound = (WgibeatRootFolder + "\\Content\\Audio\\Fallback.ogg")
            };
            Audio.SetMasterVolume((float)Settings.Get<double>("SongVolume"));

            Songs = new SongManager(this.Log, this.Audio, this.Settings);
            Crossfader = new CrossfaderManager(this.Log, this.Audio);

            CPUManager = new CPUManager(this.Log);
            CPUManager.LoadWeights("CPUSkill.txt");

            _menuMusicManager = new MenuMusicManager(this.Log)
            {
                MusicFilePath = WgibeatRootFolder + "\\MenuMusic\\",
                AudioManager = this.Audio,
                Crossfader = this.Crossfader
            };
            _menuMusicManager.LoadMusicList(_menuMusicManager.MusicFilePath + "MusicList.txt");

            KeyMappings = new KeyMappings(this.Log);
            bool passed = KeyMappings.LoadFromFile("Keys.conf");

            if (!passed)
                KeyMappings.LoadDefault();
        }

        private void InitPlayers()
        {
            Players = new Player[4];
            for (int x = 0; x < 4; x++)
            {
                Players[x] = new Player();
                Players[x].ResetStats();
            }
        }

        private void InitializeScreens()
        {
            _screens = new Dictionary<string, GameScreen>();
            _screens.Add("InitialLoad", new InitialLoadScreen(this));
            _screens.Add("MainMenu", new MainMenuScreen(this));
            _screens.Add("NewGame", new NewGameScreen(this));
            _screens.Add("MainGame", new MainGameScreen(this));
            _screens.Add("Evaluation", new EvaluationScreen(this));
            _screens.Add("SongSelect", new SongSelectScreen(this));
            _screens.Add("KeyOptions", new KeyOptionScreen(this));
            _screens.Add("Options", new OptionScreen(this));
            _screens.Add("ModeSelect", new ModeSelectScreen(this));
            _screens.Add("TeamSelect", new TeamSelectScreen(this));
            _screens.Add("Instruction", new InstructionScreen(this));
            _screens.Add("SongEdit", new SongEditorScreen(this));
            _screens.Add("Stats", new StatsScreen(this));

            if (!Settings.Get<bool>("RunOnce"))
            {
                ScreenTransition("Instruction");
                Cookies["FirstScreen"] = true;
                Settings["RunOnce"] = true;
                Settings.SaveToFile("settings.txt");
            }
            else
            {
                ScreenTransition("InitialLoad");
            }

        }

        public void SetGraphicsSettings()
        {
            GraphicsManager.IsFullScreen = Settings.Get<bool>("FullScreen");
            string[] resolution = Settings.Get<string>("ScreenResolution").Split('x');
            GraphicsManager.PreferredBackBufferWidth = Convert.ToInt32(resolution[0]);
            GraphicsManager.PreferredBackBufferHeight = Convert.ToInt32(resolution[1]);
            GraphicsDevice.RenderState.ScissorTestEnable = true;
            Sprite.SetMultiplier(Convert.ToInt32(resolution[0]), Convert.ToInt32(resolution[1]));
            Sprite.Core = this;
            PrimitiveLine.Multiplier = Sprite.Multiplier;
            GraphicsManager.ApplyChanges();
        }

        #endregion

        #region Graphics Loading
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {         
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadCurrentTheme();

            var files = Directory.GetFiles(Content.RootDirectory + "/Fonts", "*.xnb");
            foreach (string file in files)
            {
                TextureManager.AddFont(Path.GetFileNameWithoutExtension(file), Content.Load<SpriteFont>("Fonts/" + Path.GetFileNameWithoutExtension(file)));
            }
        }

        public void LoadCurrentTheme()
        {
            var themeFolder = WgibeatRootFolder + "\\Content\\Textures\\";
            if (!(Settings.Get<string>("Theme") == "Default"))
            {
                TextureManager.LoadTheme(themeFolder + "Default");
                Metrics.LoadFromFile(themeFolder + "Default\\metrics.txt");
            }
            TextureManager.LoadTheme(themeFolder + Settings.Get<string>("Theme"));
            Metrics.LoadFromFile(themeFolder + Settings.Get<string>("Theme") + "\\metrics.txt");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }
        #endregion

        #region Overrides

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (_screens == null)
            {
                InitializeScreens();
            }

            DetectKeyPresses();
            DetectButtonPresses();

            _activeScreen.Update(gameTime);
            Crossfader.Update(gameTime.TotalRealTime.TotalSeconds);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            ShiftSpriteBatch(false);

            _activeScreen.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();
            _drawInProgress = false;
            base.Draw(gameTime);
        }


        public void ShiftSpriteBatch(bool enableWrap)
        {
            if (_drawInProgress)
            {
                _drawInProgress = false;
                _spriteBatch.End();
            }

            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend,SpriteSortMode.Immediate,SaveStateMode.None);
            if (enableWrap)
            {
                GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
                GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            }
            else
            {
                GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
                GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
            }
            _drawInProgress = true;

        }
        #endregion

        #region Input Handling
        private void DetectButtonPresses()
        {

            for (int x = 0; x < 4; x++)
            {
                GamePadState currentState = GamePad.GetState((PlayerIndex) x);
                foreach (Buttons button in GetPressedButtons(currentState))
                {

                    if (_lastGamePadState[x].IsButtonUp(button))
                    {
                        if (KeyMappings.GetAction(button,x + 1) != null)
                            _activeScreen.PerformAction(KeyMappings.GetAction(button,x + 1));

                        _activeScreen.PerformButton(button, x + 1);
                    }
                }

                foreach (Buttons button in GetPressedButtons(_lastGamePadState[x]))
                {
                    if (currentState.IsButtonUp(button))
                    {
                        _activeScreen.PerformActionReleased(KeyMappings.GetAction(button,x + 1));
                    }
                }

                _lastGamePadState[x] = currentState;
            }

        }

        private List<Buttons> GetPressedButtons(GamePadState state)
        {
            var result = new List<Buttons>();

            Buttons[] options = {
                                    Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LeftShoulder, Buttons.RightShoulder
                                    , Buttons.Start, Buttons.Back, Buttons.LeftTrigger, Buttons.RightTrigger, Buttons.DPadDown,
                                    Buttons.DPadLeft, Buttons.DPadUp, Buttons.DPadRight
                                };
            foreach (Buttons option in options)
            {
                if (state.IsButtonDown(option))
                {
                    result.Add(option);
                }
            }
            return result;
        }

        private void DetectKeyPresses()
        {
            KeyboardState currentState = Keyboard.GetState();
            
            foreach (Keys key in currentState.GetPressedKeys())
            {
                if (_lastKeystate.IsKeyUp(key))
                {
                    if (KeyMappings.GetAction(key) != null)
                        _activeScreen.PerformAction(KeyMappings.GetAction(key));

                    _activeScreen.PerformKey(key);
                }
            }

            foreach (Keys key in _lastKeystate.GetPressedKeys())
            {
                if ((currentState.IsKeyUp(key)) && (KeyMappings.GetAction(key) != null))
                {
                    _activeScreen.PerformActionReleased(KeyMappings.GetAction(key));
                }
            }
            _lastKeystate = currentState;
        }
        #endregion


        /// <summary>
        /// Changes the currently displayed GameScreen to the one with the provided name.
        /// This GameScreen must already be present on the GameScreen Dictionary.
        /// </summary>
        /// <param name="screen">The name of the GameScreen to change to.</param>
        public void ScreenTransition(string screen)
        {
            if (!_screens.ContainsKey(screen))
            {
                throw new InvalidOperationException("GameCore does not contain a screen called: " + screen);
            }
            _screens[screen].Initialize();
            _activeScreen = _screens[screen];
            _menuMusicManager.ChangeMusic(screen);
            Log.AddMessage("Screen transition to: " + screen,LogLevel.INFO);
        }
    }
}
