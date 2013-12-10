using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoundLineCode;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
//using WGiBeat.NetSystem;
using WGiBeat.Players;
using WGiBeat.Screens;

namespace WGiBeat
{
    /// <summary>
    /// Represents the Core of the game. The main game loop happens here, and flows to the currently active
    /// GameScreen. Also contains game settings, metrics, song database and players and managers for universal access.
    /// </summary>
    public class GameCore : Game
    {
        public readonly GraphicsDeviceManager GraphicsManager;
        private SpriteBatch _spriteBatch;
        public SettingsManager Settings;
        public MetricsManager Metrics;
        public ProfileManager Profiles;
        //public NetManager Net;

        public AudioManager Audio;
        public SongManager Songs;
        public SoundEffectManager Sounds;
        public HighScoreManager HighScores;
        public CrossfaderManager Crossfader;
        public CPUManager CPUManager;
        public LogManager Log;
        public TextManager Text;
        public KeyMappings KeyMappings;
        public UpdateManager UpdateManager;

        public Player[] Players;
        public Dictionary<string, object> Cookies;

        private Dictionary<string, GameScreen> _screens;
        private GameScreen _activeScreen;
        private MenuMusicManager _menuMusicManager; 
        
        private KeyboardState _lastKeystate;
        private GamePadState[] _lastGamePadState;
        public string WgibeatRootFolder;

        public const string VERSION_STRING = "v2.0 a3";
        private GameCore()
        {
            GraphicsManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        //TODO: Refactor to use this with Sets.
        private static GameCore _instance;
      
        public static GameCore Instance
        {
            get { return _instance ?? (_instance = new GameCore()); }
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

            WgibeatRootFolder = "" + Path.GetDirectoryName(
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
            Log = new LogManager
                      {Enabled = true, SaveLog = true, RootFolder = WgibeatRootFolder, LogLevel = LogLevel.INFO};
            Log.AddMessage("Initializing Cookies...", LogLevel.INFO);
            Cookies = new Dictionary<string, object>();

            TextureManager.Log = Log;
            TextureManager.GraphicsDevice = this.GraphicsDevice;

            Metrics = new MetricsManager {Log = this.Log};
            Settings = SettingsManager.LoadFromFile(WgibeatRootFolder + "\\settings.txt", this.Log);
            Log.LogLevel = (LogLevel) Settings.Get<int>("LogLevel");
            HighScores = HighScoreManager.LoadFromFile(WgibeatRootFolder + "\\Scores.conf", this.Log);
            Profiles = ProfileManager.LoadFromFolder(WgibeatRootFolder + "\\Profiles", this.Log);
            //TODO: Refactor
            Text = TextManager.LoadFromFile(WgibeatRootFolder + "\\Content\\Text\\OptionText.txt", this.Log);
            Text.AddResource(WgibeatRootFolder + "\\Content\\Text\\EditorText.txt");
            Text.AddResource(WgibeatRootFolder + "\\Content\\Text\\ModeText.txt");

            Audio = new AudioManager(this.Log)
                        {
                            FallbackSound = (WgibeatRootFolder + "\\Content\\SoundEffects\\Fallback.ogg")
                        };
            Audio.SetMasterVolume((float) Settings.Get<double>("SongVolume"));

            Songs = new SongManager(this.Log, this.Audio, this.Settings);
            Sounds = new SoundEffectManager(this.Log, this.Audio, this.Settings);

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
    //        _menuMusicManager.ChangeMusic("InitialLoad");

            KeyMappings = new KeyMappings(this.Log);
            if (!KeyMappings.LoadFromFile("Keys.conf"))
            {
                KeyMappings.LoadDefault();
            }            

            UpdateManager = new UpdateManager {Log = this.Log};
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
            _screens.Add("Credits", new CreditsScreen(this));
           // _screens.Add("Net",new NetLobbyScreen(this));

            foreach (GameScreen screen in _screens.Values)
            {
                screen.SoundTriggered += (s,e) => Sounds.PlaySoundEffect((SoundEvent) (e.Object));
            }
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

        public const int INTERNAL_WIDTH = 1280;
        public const int INTERNAL_HEIGHT = 720;
        public void SetGraphicsSettings()
        {
            
 
            GraphicsManager.IsFullScreen = Settings.Get<bool>("FullScreen");
            GraphicsManager.SynchronizeWithVerticalRetrace = Settings.Get<bool>("VSync");
            string[] resolution = Settings.Get<string>("ScreenResolution").Split('x');
            GraphicsManager.PreferredBackBufferWidth = Convert.ToInt32(resolution[0]);
            GraphicsManager.PreferredBackBufferHeight = Convert.ToInt32(resolution[1]);

            Sprite3D.Device = this.GraphicsDevice;
            SpriteMap3D.Device = this.GraphicsDevice;
            Sprite3D.EffectInit = false;
            RoundLineManager.Instance.Init(this.GraphicsDevice,this.Content, Sprite3D.GetViewProjMatrix(INTERNAL_WIDTH, INTERNAL_HEIGHT));
            RoundLineManager.Instance.BlurThreshold = 
                RoundLineManager.Instance.ComputeBlurThreshold(1.0f,Sprite3D.GetViewProjMatrix(INTERNAL_WIDTH,INTERNAL_HEIGHT),INTERNAL_WIDTH);
            GraphicsManager.ApplyChanges();
            GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColorTexture.VertexElements);
            FontManager.FontMatrix = Matrix.CreateScale(1.0f * GraphicsManager.PreferredBackBufferWidth/INTERNAL_WIDTH,
                                                           1.0f * GraphicsManager.PreferredBackBufferHeight/INTERNAL_HEIGHT, 1);

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
            FontManager.SpriteBatch = _spriteBatch;
  
            LoadCurrentTheme();

            var files = Directory.GetFiles(Content.RootDirectory + "/Fonts", "*.xnb");
            foreach (string file in files)
            {
                FontManager.AddFont(Path.GetFileNameWithoutExtension(file), Content.Load<SpriteFont>("Fonts/" + Path.GetFileNameWithoutExtension(file)));
            }
           
        }

        public void LoadCurrentTheme()
        {
            var themeFolder = WgibeatRootFolder + "\\Content\\Textures\\";
            var currentTheme = Settings.Get<string>("Theme");
            if (currentTheme != "Default")
            {
                TextureManager.LoadTheme(themeFolder + "Default");
                Metrics.LoadFromFile(themeFolder + "Default\\metrics.txt");
                Sounds.LoadFromFolder(WgibeatRootFolder + "\\Content\\SoundEffects\\Default");
            }
            TextureManager.LoadTheme(themeFolder + currentTheme);
            Metrics.LoadFromFile(themeFolder + currentTheme + "\\metrics.txt");
            Sounds.LoadFromFolder(WgibeatRootFolder + "\\Content\\SoundEffects\\" + currentTheme);
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
            //Net.ListenForMessages();
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
            TextureManager.LastGameTime = gameTime;
            GraphicsDevice.Clear(Color.Black);     
            _activeScreen.Draw(gameTime, _spriteBatch);
      
            base.Draw(gameTime);

        }

        #endregion

        #region Input Handling
        private void DetectButtonPresses()
        {

            for (int x = 0; x < 4; x++)
            {
                var currentState = GamePad.GetState((PlayerIndex) x);
                foreach (var button in GetPressedButtons(currentState))
                {

                    if (_lastGamePadState[x].IsButtonUp(button))
                    {
                        if (KeyMappings.GetAction(button,x + 1) != null)
                            _activeScreen.PerformAction(KeyMappings.GetAction(button,x + 1));

                        _activeScreen.PerformButton(button, x + 1);
                    }
                }

                foreach (var button in GetPressedButtons(_lastGamePadState[x]))
                {
                    if (currentState.IsButtonUp(button))
                    {
                        _activeScreen.PerformActionReleased(KeyMappings.GetAction(button,x + 1));
                    }
                }

                _lastGamePadState[x] = currentState;
            }

        }

        private IEnumerable<Buttons> GetPressedButtons(GamePadState state)
        {
            Buttons[] options = {
                                    Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LeftShoulder, Buttons.RightShoulder
                                    , Buttons.Start, Buttons.Back, Buttons.LeftTrigger, Buttons.RightTrigger, Buttons.DPadDown,
                                    Buttons.DPadLeft, Buttons.DPadUp, Buttons.DPadRight
                                };
            return options.Where(state.IsButtonDown).ToList();
        }

        private void DetectKeyPresses()
        {
            var currentState = Keyboard.GetState();
            
            foreach (var key in currentState.GetPressedKeys())
            {
                if (_lastKeystate.IsKeyUp(key))
                {
                    if (KeyMappings.GetAction(key) != null)
                        _activeScreen.PerformAction(KeyMappings.GetAction(key));

                    _activeScreen.PerformKey(key);

                    if (key == Keys.Enter && (currentState.IsKeyDown(Keys.LeftAlt) || currentState.IsKeyDown(Keys.RightAlt)))
                    {
                        ToggleFullScreen();
                    }
                }
                
            }

            foreach (var key in _lastKeystate.GetPressedKeys())
            {
                if ((currentState.IsKeyUp(key)) && (KeyMappings.GetAction(key) != null))
                {
                    _activeScreen.PerformActionReleased(KeyMappings.GetAction(key));
                }
            }
            _lastKeystate = currentState;
        }

        private void ToggleFullScreen()
        {
            GraphicsManager.IsFullScreen = ! GraphicsManager.IsFullScreen;
            GraphicsManager.ApplyChanges();
            GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColorTexture.VertexElements);
            Settings["FullScreen"] = GraphicsManager.IsFullScreen;
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
