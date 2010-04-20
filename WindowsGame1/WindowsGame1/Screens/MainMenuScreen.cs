using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame1.Drawing;

namespace WindowsGame1.Screens
{
    public class MainMenuScreen : GameScreen
    {
        public MainMenuOption SelectedMenuOption;

        private string[] _menuText = {"Start Game", "Options", "Exit"};
        public MainMenuScreen(GameCore core) : base(core)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Initialize()
        {
            base.Initialize();
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawMenu(spriteBatch);
        }

        private void DrawMenu(SpriteBatch spriteBatch)
        {
           for (int menuOption = 0; menuOption < (int) MainMenuOption.COUNT; menuOption++)
           {
               var menuOptionSprite = new Sprite
                                          {
                                              Height = 50,
                                              SpriteTexture = TextureManager.Textures["mainMenuOption"],
                                              Width = 300
                                          };
               if (menuOption == (int) SelectedMenuOption)
               {
                   menuOptionSprite.SpriteTexture = TextureManager.Textures["mainMenuOptionSelected"];
               }
               menuOptionSprite.SetPosition(Core.Metrics["MainMenuOptions", menuOption]);
               menuOptionSprite.Draw(spriteBatch);

               spriteBatch.DrawString(TextureManager.Fonts["LargeFont"],_menuText[menuOption], Core.Metrics["MainMenuOptionText",menuOption],Color.Black);
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
                    newOptionValue = (int) SelectedMenuOption - 1;
                    if (newOptionValue < 0)
                    {
                        newOptionValue += (int) MainMenuOption.COUNT;
                    }
                    SelectedMenuOption = (MainMenuOption) newOptionValue;
                    break;
                case Action.P1_DOWN:
                case Action.P2_DOWN:
                case Action.P3_DOWN:
                case Action.P4_DOWN:
                    newOptionValue = (int)SelectedMenuOption + 1;
                    newOptionValue %= (int) MainMenuOption.COUNT;
                    SelectedMenuOption = (MainMenuOption) newOptionValue;
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
            }
        }

        private void MenuOptionSelected()
        {
            switch (SelectedMenuOption)
            {
                case MainMenuOption.START_GAME:
                    Core.ScreenTransition("NewGame");
                    break;
                case MainMenuOption.OPTIONS:
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
        OPTIONS = 1,
        EXIT = 2,
        COUNT = 3
    }
}
