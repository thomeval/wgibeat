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
using WGiBeat.Screens;
using Action=WGiBeat.Managers.Action;

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

        public Player[] Players;
        public Dictionary<string, object> Cookies;

        private Dictionary<string, GameScreen> _screens;
        private GameScreen _activeScreen;
        private MenuMusicManager _menuMusicManager; 
        public readonly KeyMappings KeyMappings = new KeyMappings(); //Changed to public, for GameScreen access.

        private KeyboardState _lastKeystate;
        private GamePadState[] _lastGamePadState;
        public string WgibeatRootFolder;

        public const string VERSION_STRING = "v0.6 pre";
        public GameCore()
        {
            GraphicsManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //For some reason XNA has a bug when using FixedTimeSteps, which is enabled by default.
            //Using this causes 100% CPU usage (one core) and a frame rate drop.
            this.IsFixedTimeStep = false;
            //NOTE: Uncomment to disable vsync.
            //GraphicsManager.SynchronizeWithVerticalRetrace = false;

            WgibeatRootFolder = Path.GetDirectoryName(
Assembly.GetAssembly(typeof(GameCore)).CodeBase);
            WgibeatRootFolder = WgibeatRootFolder.Replace("file:\\", "");

            Log = new LogManager{Enabled = true, SaveLog = true};
            Log.AddMessage("INFO: Initializing Cookies...");
            Cookies = new Dictionary<string, object>();

            Metrics = new MetricsManager { Log = this.Log };
            Settings = SettingsManager.LoadFromFile("settings.txt", this.Log);
            HighScores = HighScoreManager.LoadFromFile("Scores.conf", this.Log);
            Profiles = ProfileManager.LoadFromFolder("Profiles", this.Log);

            Audio = new AudioManager(this.Log);
            Songs = new SongManager(this.Log,this.Audio,this.Settings);
            Crossfader = new CrossfaderManager(this.Log,this.Audio);

            CPUManager = new CPUManager(this.Log);
            CPUManager.LoadWeights("CPUSkill.txt");

            TextureManager.GraphicsDevice = this.GraphicsDevice;

            if (!Directory.Exists(WgibeatRootFolder + "\\" + Settings["SongFolder"]))
            {
                Directory.CreateDirectory(WgibeatRootFolder + "\\" + Settings["SongFolder"]);
            }

            LoadCurrentTheme();

            _menuMusicManager = new MenuMusicManager(this.Log)
            {
                MusicFilePath = WgibeatRootFolder + "\\MenuMusic\\",
                AudioManager = this.Audio,
                Crossfader = this.Crossfader
            };
            _menuMusicManager.LoadMusicList(_menuMusicManager.MusicFilePath + "MusicList.txt");

            Players = new Player[4];

            for (int x = 0; x < 4; x++)
            {
                Players[x] = new Player();
                Players[x].ResetStats();
            }

            Boolean passed = KeyMappings.LoadFromFile("Keys.conf");

            if (!passed)
                KeyMappings.LoadDefault();
            
            Audio.SetMasterVolume((float) Settings.Get<double>("SongVolume"));
            GraphicsManager.IsFullScreen = Settings.Get<bool>("FullScreen");

            GraphicsDevice.RenderState.ScissorTestEnable = true;
            GraphicsManager.ApplyChanges();
            _lastGamePadState = new GamePadState[4];

            base.Initialize();
        }

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

        private void DetectButtonPresses()
        {

            for (int x = 0; x < 4; x++)
            {
                GamePadState currentState = GamePad.GetState((PlayerIndex) x);
                foreach (Buttons button in GetPressedButtons(currentState))
                {

                    if (_lastGamePadState[x].IsButtonUp(button))
                    {
                        if (KeyMappings.GetAction(button,x + 1) != Action.NONE)
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
                    if (KeyMappings.GetAction(key) != Action.NONE)
                        _activeScreen.PerformAction(KeyMappings.GetAction(key));

                    _activeScreen.PerformKey(key);
                }
            }

            foreach (Keys key in _lastKeystate.GetPressedKeys())
            {
                if (currentState.IsKeyUp(key))
                {
                    _activeScreen.PerformActionReleased(KeyMappings.GetAction(key));
                }
            }
            _lastKeystate = currentState;
        }

        private void InitializeScreens()
        {
            _screens = new Dictionary<string, GameScreen>();
            _screens.Add("InitialLoad",new InitialLoadScreen(this));
            _screens.Add("MainMenu", new MainMenuScreen(this));
            _screens.Add("NewGame",new NewGameScreen(this));
            _screens.Add("MainGame",new MainGameScreen(this));
            _screens.Add("Evaluation", new EvaluationScreen(this));
            _screens.Add("SongSelect", new SongSelectScreen(this));
            _screens.Add("KeyOptions", new KeyOptionScreen(this));
            _screens.Add("Options", new OptionScreen(this));
            _screens.Add("ModeSelect", new ModeSelectScreen(this));
            _screens.Add("TeamSelect", new TeamSelectScreen(this));
            _screens.Add("Instruction", new InstructionScreen(this));
            _screens.Add("SongEdit", new SongEditorScreen(this));

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

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            _activeScreen.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);

        }

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
            Log.AddMessage("INFO: Screen transition to: " + screen);
        }
    }
}
