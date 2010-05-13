using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGame1.AudioSystem;
using WindowsGame1.Drawing;
using WindowsGame1.Screens;

namespace WindowsGame1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameCore : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager GraphicsManager;
        private SpriteBatch _spriteBatch;
        public SettingsManager Settings;
        public MetricsManager Metrics;
        public SongManager Songs;
        public Player[] Players;

        private Dictionary<string, GameScreen> _screens;
        private GameScreen _activeScreen;

        public KeyMappings _keyMappings = new KeyMappings(); //Changed to public, for GameScreen access.

        private KeyboardState lastKeystate;

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
            // TODO: Add your initialization logic here


            Metrics = MetricsManager.Load("metrics.txt");
            Settings = SettingsManager.LoadFromFile("settings.txt");
            Songs = SongManager.LoadFromFolder(Directory.GetCurrentDirectory() +"\\" + Settings["SongFolder"]);
            Songs.LoadHighScores("Scores.conf");
            //this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 15);
            Players = new Player[4];
           // this.IsFixedTimeStep = false;

            for (int x = 0; x < 4; x++)
            {

                Players[x] = new Player {Hits = 0, Momentum = 0, Life = 50, Score = 0, PlayDifficulty = Difficulty.BEGINNER, Streak = -1};

            }


            Boolean passed = _keyMappings.LoadFromFile("Keys.conf");

            if (!passed)
                _keyMappings.LoadDefault();
            
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

            // TODO: use this.Content to load your game content here


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
            // TODO: Unload any non ContentManager content here
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
            KeyboardState currentState = Keyboard.GetState();

            foreach (Keys key in currentState.GetPressedKeys())
            {

                if (lastKeystate.IsKeyUp(key))
                {
                    if (_keyMappings.GetAction(key) != Action.NONE)
                        _activeScreen.PerformAction(_keyMappings.GetAction(key));

                    _activeScreen.PerformKey(key);
                }



                
            }

            lastKeystate = currentState;
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            _activeScreen.Update(gameTime);
            base.Update(gameTime);
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
