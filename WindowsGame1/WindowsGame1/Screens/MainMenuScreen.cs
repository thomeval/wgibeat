using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class MainMenuScreen : GameScreen
    {
        private MainMenuOption _selectedMenuOption;

        private SineSwayParticleField _field = new SineSwayParticleField();

        private bool _displayNoSongsError;
        private readonly string[] _menuText = { "Start Game", "How to play", "Keys", "Options", "Song Editor", "Exit" };
        private Sprite _background;
        private Sprite _header;
        private Sprite _menuOptionSprite;
        private Sprite _foreground;

        public MainMenuScreen(GameCore core)
            : base(core)
        {
        }

        public override void Update(GameTime gameTime)        
        {            
            base.Update(gameTime);
        }

        public override void Initialize()
        {
            InitSprites();
            base.Initialize();
        }
        private void InitSprites()
        {
            _background = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["MainMenuBackground"],
                Width = Core.Window.ClientBounds.Width
            };
            _foreground = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["MainMenuForeground"],
                Width = Core.Window.ClientBounds.Width
            };
            _header = new Sprite
            {
                SpriteTexture = TextureManager.Textures["mainMenuHeader"]
            };
            _menuOptionSprite = new Sprite
            {
                Height = 50,
                SpriteTexture = TextureManager.Textures["mainMenuOption"],
                Width = 300
            };
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawMenu(spriteBatch);

            if (_displayNoSongsError)
            {
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"],"Error: No songs loaded", Core.Metrics["MainMenuNoSongsError",0],Color.Black);
            }
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            _background.Draw(spriteBatch);
            _field.Draw(spriteBatch);                                
            _foreground.Draw(spriteBatch);        
        }

        private void DrawMenu(SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);

            _header.Draw(spriteBatch);
            
            for (int menuOption = 0; menuOption < (int)MainMenuOption.COUNT; menuOption++)
            {

                _menuOptionSprite.SpriteTexture = menuOption == (int)_selectedMenuOption ? TextureManager.Textures["mainMenuOptionSelected"] : TextureManager.Textures["mainMenuOption"];
                _menuOptionSprite.Position = (Core.Metrics["MainMenuOptions", menuOption]);
                _menuOptionSprite.Draw(spriteBatch);
                var textPosition = Core.Metrics["MainMenuOptions", menuOption].Clone();
                textPosition.X += 150;
                textPosition.Y -= 0;
                TextureManager.DrawString(spriteBatch,_menuText[menuOption],"TwoTech36",textPosition,Color.Black, FontAlign.CENTER);
            }
        }

        public override void PerformAction(Action action)
        {
            int newOptionValue;
            int player = -1;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            switch (paction)
            {
                case "UP":
                    newOptionValue = (int)_selectedMenuOption - 1;
                    if (newOptionValue < 0)
                    {
                        newOptionValue += (int)MainMenuOption.COUNT;
                    }
                    _selectedMenuOption = (MainMenuOption)newOptionValue;
                    break;
                case "DOWN":
                    newOptionValue = (int)_selectedMenuOption + 1;
                    newOptionValue %= (int)MainMenuOption.COUNT;
                    _selectedMenuOption = (MainMenuOption)newOptionValue;
                    break;
                case "START":
                     MenuOptionSelected(player);
                    break;
                case "BEATLINE":
                    MenuOptionSelected(player);
                    break;
                case "BACK":
                    Core.Exit();
                    break;
            }
        }

        private void MenuOptionSelected(int player)
        {
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
                case MainMenuOption.EXIT:
                    Core.Exit();
                    break;
            }
        }
    }

    public enum MainMenuOption
    {
        START_GAME = 0,
        HOW_TO_PLAY = 1,
        KEYS = 2,
        OPTIONS = 3,
        SONG_EDIT = 4,
        EXIT = 5,
        COUNT = 6
    }

}
