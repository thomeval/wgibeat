using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;

namespace WGiBeat.Screens
{
    public class MainMenuScreen : GameScreen
    {
        private MainMenuOption _selectedMenuOption;

        private int _tick = 0;
        private int _maxTick = 0;

        private bool _displayNoSongsError = false;
        private readonly string[] _menuText = { "Start Game", "Keys", "Options", "Exit" };
        public MainMenuScreen(GameCore core)
            : base(core)
        {
        }


        public override void Initialize()
        {
            base.Initialize();

            try
            {
                if (Core.Songs.CurrentSong() == null || Core.Songs.CurrentSong().SongFile != @"MenuMusic.mp3")
                {
                    GameSong song = new GameSong()
                                        {
                                            Path = @"Content\Audio",
                                            SongFile = @"MenuMusic.mp3"
                                        };
                    Core.Songs.LoadSong(song);
                    Core.Songs.PlaySong(Core.Settings.Get<double>("SongVolume"));
                }
            }

            catch (Exception ex)
            {
                if (ex is System.IO.FileNotFoundException)
                {
                    Console.WriteLine(ex.Message);
                }
                else
                {
                    throw;                
                }

            }
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
            var background = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["menuBackground"],
                Width = Core.Window.ClientBounds.Width,
                X = 0,
                Y = 0
            };

            background.Draw(spriteBatch);
        }

        private void DrawMenu(SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            
            for (int menuOption = 0; menuOption < (int)MainMenuOption.COUNT; menuOption++)
            {
                var menuOptionSprite = new Sprite
                                           {
                                               Height = 50,
                                               SpriteTexture = TextureManager.Textures["mainMenuOption"],
                                               Width = 300
                                           };
                if (menuOption == (int)_selectedMenuOption)
                {
                    menuOptionSprite.SpriteTexture = TextureManager.Textures["mainMenuOptionSelected"];
                }
                menuOptionSprite.SetPosition(Core.Metrics["MainMenuOptions", menuOption]);
                menuOptionSprite.Draw(spriteBatch);

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], _menuText[menuOption], Core.Metrics["MainMenuOptionText", menuOption], Color.Black);
            }
        }

        public override void PerformAction(Action action)
        {
            int newOptionValue;
            switch (action)
            {
                case Action.P1_UP:
                case Action.P2_UP:
                case Action.P3_UP:
                case Action.P4_UP:
                    newOptionValue = (int)_selectedMenuOption - 1;
                    if (newOptionValue < 0)
                    {
                        newOptionValue += (int)MainMenuOption.COUNT;
                    }
                    _selectedMenuOption = (MainMenuOption)newOptionValue;
                    break;
                case Action.P1_DOWN:
                case Action.P2_DOWN:
                case Action.P3_DOWN:
                case Action.P4_DOWN:
                    newOptionValue = (int)_selectedMenuOption + 1;
                    newOptionValue %= (int)MainMenuOption.COUNT;
                    _selectedMenuOption = (MainMenuOption)newOptionValue;
                    break;
                case Action.P1_START:
                case Action.P2_START:
                case Action.P3_START:
                case Action.P4_START:
                     MenuOptionSelected();
                    break;
                case Action.P1_BEATLINE:
                case Action.P2_BEATLINE:
                case Action.P3_BEATLINE:
                case Action.P4_BEATLINE:
                    MenuOptionSelected();
                    break;
                case Action.SYSTEM_BACK:
                    Core.Exit();
                    break;
            }
        }

        private void MenuOptionSelected()
        {
            switch (_selectedMenuOption)
            {
                case MainMenuOption.START_GAME:
                    //Core.Songs.StopSong();
                    if (Core.Songs.AllSongs().Count > 0)
                    {
                        Core.ScreenTransition("NewGame");
                    }
                    else
                    {
                        _displayNoSongsError = true;
                    }
                    break;
                case MainMenuOption.KEYS:
                    //Core.Songs.StopSong();
                    Core.ScreenTransition("KeyOptions");
                    break;
                case MainMenuOption.OPTIONS:
                    Core.ScreenTransition("Options");
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
        KEYS = 1,
        OPTIONS = 2,
        EXIT = 3,
        COUNT = 4
    }
}
