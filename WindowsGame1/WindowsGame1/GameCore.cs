using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        public GraphicsDeviceManager GraphicsManager;
        private SpriteBatch _spriteBatch;
        public SettingsManager Settings;
        public MetricsManager Metrics;
        public SongManager Songs;
        public HighScoreManager HighScores;
        public Player[] Players;
        public Dictionary<string, object> Cookies;

        private Dictionary<string, GameScreen> _screens;
        private GameScreen _activeScreen;

        public readonly KeyMappings KeyMappings = new KeyMappings(); //Changed to public, for GameScreen access.

        private KeyboardState _lastKeystate;
        private GamePadState[] _lastGamePadState;

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
            Cookies = new Dictionary<string, object>();
            Metrics = MetricsManager.Load("metrics.txt");
            Settings = SettingsManager.LoadFromFile("settings.txt");
            HighScores = HighScoreManager.LoadFromFile("Scores.conf");
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\" + Settings["SongFolder"]))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + Settings["SongFolder"]);
            }
            Songs = SongManager.LoadFromFolder(Directory.GetCurrentDirectory() +"\\" + Settings["SongFolder"]);


            Players = new Player[4];

            for (int x = 0; x < 4; x++)
            {
                Players[x] = new Player
                                 {
                                     Hits = 0,
                                     Momentum = 0,
                                     Life = 50,
                                     Score = 0,
                                     PlayDifficulty = Difficulty.BEGINNER,
                                     Streak = -1,
                                     BeatlineSpeed = 1.0
                                 };
            }

            Boolean passed = KeyMappings.LoadFromFile("Keys.conf");

            if (!passed)
                KeyMappings.LoadDefault();
            
            if (Settings.Exists("SongVolume"))
            {
                Songs.SetMasterVolume((float) Settings.Get<double>("SongVolume"));
            }

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

            var files = Directory.GetFiles(Content.RootDirectory + "/Textures", "*.xnb");
            foreach (string file in files)
            {
                TextureManager.AddTexture(Path.GetFileNameWithoutExtension(file), Content.Load<Texture2D>("Textures/" + Path.GetFileNameWithoutExtension(file)));
            }

            files = Directory.GetFiles(Content.RootDirectory + "/Fonts", "*.xnb");
            foreach (string file in files)
            {
                TextureManager.AddFont(Path.GetFileNameWithoutExtension(file), Content.Load<SpriteFont>("Fonts/" + Path.GetFileNameWithoutExtension(file)));
            }

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

                        _activeScreen.PerformButton(button,x + 1);
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
            _lastKeystate = currentState;
        }

        private void InitializeScreens()
        {
            _screens = new Dictionary<string, GameScreen>();
            _screens.Add("MainMenu", new MainMenuScreen(this));
            _screens.Add("NewGame",new NewGameScreen(this));
            _screens.Add("MainGame",new MainGameScreen(this));
            _screens.Add("Evaluation", new EvaluationScreen(this));
            _screens.Add("SongSelect", new SongSelectScreen(this));
            _screens.Add("KeyOptions", new KeyOptionScreen(this));
            _screens.Add("Options", new OptionScreen(this));
            _screens.Add("ModeSelect", new ModeSelectScreen(this));
            _activeScreen = _screens["MainMenu"];
            _activeScreen.Initialize();
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
        }
    }
}
