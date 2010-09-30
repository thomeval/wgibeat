using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Microsoft.Xna.Framework.Input;
using Action = WGiBeat.Managers.Action;


/* Instructions
 * 
 * 1. Switch to a player by pressing any of his keys.
 * 2. Move to action you wish to change.
 * 3. Press any player's START action button.
 * 4. Press key that you wish to use to perform that action.
 * 5. Voila.
 * 
 */

namespace WGiBeat.Screens
{
    public class KeyOptionScreen : GameScreen
    {
        private int _currentPlayer = 1;
        private int _selectedMenuOption = 0;
        //private Boolean LastActionSide = false;

        private static int _rowOne = 95;
        private static int _rowTwo = 160;
        private static int _columnOne = 100;
        private static int _columnTwo = 400;


        private readonly ButtonLink[] _links =  {
                                         new ButtonLink(Action.P1_LEFT,     Action.P2_LEFT,     Action.P3_LEFT,     Action.P4_LEFT,     "Left"),
                                         new ButtonLink(Action.P1_RIGHT,    Action.P2_RIGHT,    Action.P3_RIGHT,    Action.P4_RIGHT,    "Right"),
                                         new ButtonLink(Action.P1_UP,       Action.P2_UP,       Action.P3_UP,       Action.P4_UP,       "Up"),
                                         new ButtonLink(Action.P1_DOWN,     Action.P2_DOWN,     Action.P3_DOWN,     Action.P4_DOWN,     "Down"),
                                         new ButtonLink(Action.P1_BEATLINE, Action.P2_BEATLINE, Action.P3_BEATLINE, Action.P4_BEATLINE, "Beatline"),
                                         new ButtonLink(Action.P1_START,    Action.P2_START,    Action.P3_START,    Action.P4_START,    "Start"),
                                         new ButtonLink(Action.P1_SELECT,   Action.P2_SELECT,   Action.P3_SELECT,   Action.P4_SELECT,   "Select"),
                                         new ButtonLink(Action.P1_SELECT,   Action.P2_SELECT,   Action.P3_SELECT,   Action.P4_SELECT,   "Set Defaults"),
                                        };

        private SineSwayParticleField _field = new SineSwayParticleField();

        public KeyOptionScreen(GameCore core)
            : base(core)
        {
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Current player: Player" + _currentPlayer, new Vector2(50, 110), Color.Black);
            DrawBackground(spriteBatch);
            DrawOverlay(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            var background = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["allBackground"],
                Width = Core.Window.ClientBounds.Width,
                X = 0,
                Y = 0
            };

            background.Draw(spriteBatch);
        }

        private void DrawOverlay(SpriteBatch spriteBatch)
        {

            DrawBackground(spriteBatch);

            _field.Draw(spriteBatch);

            var header = new Sprite
                             {
                                 SpriteTexture = TextureManager.Textures["keyOptionHeader"],
                                 X = 0,
                                 Y = 0
                             };

            header.Draw(spriteBatch);

            String instructionText = "";

            if (State.CurrentState == 3)
                instructionText = "Press key to add to Player " + _currentPlayer+ "'s" + " action '" + _links[_selectedMenuOption].Name +
                                  "'";
            else
            {
                instructionText = "Press your start button to add a key to the selected action.";
            }

            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], instructionText, new Vector2(60, 530), Color.Red);




            var panelPosition = new Vector2(0, _rowOne);
            var textPosition = new Vector2(0, 0);

            var menuOptionSprite = new Sprite
                                       {
                                           Height = 50,
                                           SpriteTexture = TextureManager.Textures["mainMenuOption"],
                                           Width = 160
                                       };



            //Draw player list.

            for (int playerOption = 1; playerOption <= 4; playerOption++)
            {
                panelPosition.X = 60 + (170 * (playerOption - 1));

                menuOptionSprite.SetPosition(panelPosition);

                if (playerOption == _currentPlayer)
                {
                    menuOptionSprite.SpriteTexture = TextureManager.Textures["mainMenuOptionSelected"];
                    menuOptionSprite.Height = menuOptionSprite.Height;
                }
                else
                {
                    menuOptionSprite.SpriteTexture = TextureManager.Textures["mainMenuOption"];
                }


                menuOptionSprite.Draw(spriteBatch);
                textPosition.X = panelPosition.X + 32;
                textPosition.Y = panelPosition.Y + 10;
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Player " + playerOption, textPosition,
                                       Color.Black);

            }


            //Draw options.

            panelPosition.X = _columnOne;
            textPosition.X = _columnOne + 20;

            menuOptionSprite.Width = 200;
            menuOptionSprite.Height = 40;

            for (int menuOption = 0; menuOption < _links.Length; menuOption++)
            {

                panelPosition.Y = _rowTwo + (40 * menuOption);
                textPosition.Y = panelPosition.Y + 5;

                if (menuOption == _selectedMenuOption)
                    menuOptionSprite.SpriteTexture = TextureManager.Textures["Button_Active"];
                else
                    menuOptionSprite.SpriteTexture = TextureManager.Textures["Button_Idle"];

                menuOptionSprite.SetPosition(panelPosition);
                menuOptionSprite.Draw(spriteBatch);

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], _links[menuOption].Name, textPosition,
                                       Color.Black);

            }



            //Draw listed keys.

            panelPosition.X = _columnTwo;
            panelPosition.Y = _rowTwo;

            textPosition.X = _columnTwo + 20;
            textPosition.Y = _rowTwo + 5;

            menuOptionSprite.Width = 300;

            if (_selectedMenuOption != _links.Length - 1)
            {
                Keys[] tempKeyList = Core.KeyMappings.GetKeys(_links[_selectedMenuOption].GetAction(_currentPlayer));

                foreach (Keys key in tempKeyList)
                {
                    menuOptionSprite.SetPosition(panelPosition);
                    menuOptionSprite.Draw(spriteBatch);
                    spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Key = " + key, textPosition, Color.Black);
                    panelPosition.Y += 40;
                    textPosition.Y = panelPosition.Y + 5;
                }


                for (int index = 1; index < 4; index++)
                {
                    var buttonList = Core.KeyMappings.GetButtons(_links[_selectedMenuOption].GetAction(_currentPlayer),
                                                                 index);
                    foreach (Buttons button in buttonList)
                    {

                        menuOptionSprite.SetPosition(panelPosition);
                        menuOptionSprite.Draw(spriteBatch);
                        spriteBatch.DrawString(TextureManager.Fonts["LargeFont"],
                                               "Pad " + index + " = " + button, textPosition,
                                               Color.Black);
                        panelPosition.Y += 40;
                        textPosition.Y = panelPosition.Y + 5;
                    }
                }
            }
        }

        public override void PerformKey(Keys key)
        {
            switch (State.CurrentState)
            {
                case 2:
                    State.CurrentState = 3;
                    break;
                case 3:

                    Core.KeyMappings.SetKey(key, _links[_selectedMenuOption].GetAction(_currentPlayer));
                    Core.KeyMappings.SaveToFile("Keys.conf");

                    //_selectChange = false;
                    //_avoidNextAction = true;
                    State.CurrentState = 1;
                    break;
            }
        }

        public override void PerformButton(Buttons buttons, int playerIndex)
        {
            switch (State.CurrentState)
            {
                case 2:
                    State.CurrentState = 3;
                    break;
                case 3:

                    Core.KeyMappings.SetButton(buttons, playerIndex, _links[_selectedMenuOption].GetAction(_currentPlayer));
                    Core.KeyMappings.SaveToFile("Keys.conf");

                    //_selectChange = false;
                    //_avoidNextAction = true;
                    State.CurrentState = 1;
                    break;
            }
        }

        public override void PerformAction(Action action)
        {


            if (action == Action.SYSTEM_BACK)
                Core.ScreenTransition("MainMenu");
            else
            {
                if (State.CurrentState == 1)
                {

                    switch (action)
                    {
                        case Action.P1_UP:
                        case Action.P2_UP:
                        case Action.P3_UP:
                        case Action.P4_UP:
                            _selectedMenuOption--;

                            if (_selectedMenuOption < 0)
                                _selectedMenuOption = _links.Length - 1;

                            break;

                        case Action.P1_DOWN:
                        case Action.P2_DOWN:
                        case Action.P3_DOWN:
                        case Action.P4_DOWN:
                            _selectedMenuOption++;

                            if (_selectedMenuOption >= _links.Length)
                                _selectedMenuOption = 0;

                            break;

                        case Action.P1_LEFT:
                        case Action.P2_LEFT:
                        case Action.P3_LEFT:
                        case Action.P4_LEFT:
                            //_selectChange = false;
                            //_avoidNextAction = false;

                            _currentPlayer--;

                            if (_currentPlayer < 1)
                                _currentPlayer = 4;

                            return;

                        case Action.P1_RIGHT:
                        case Action.P2_RIGHT:
                        case Action.P3_RIGHT:
                        case Action.P4_RIGHT:
                            //_selectChange = false;
                            //_avoidNextAction = false;

                            _currentPlayer++;

                            if (_currentPlayer > 4)
                                _currentPlayer = 1;
                            return;

                        case Action.P1_START:
                        case Action.P2_START:
                        case Action.P3_START:
                        case Action.P4_START:

                            if (_selectedMenuOption == _links.Length - 1)
                            {
                                Core.KeyMappings.LoadDefault();
                                Core.KeyMappings.SaveToFile("Keys.conf");
                            }
                            else
                            {
                                //_selectChange = true;
                                //_avoidNextAction = true;
                                State.CurrentState = 2;
                            }
                            break;
                    }
                }
            }
        }

        private struct ButtonLink
        {
            private Action P1Action { get; set; }
            private Action P2Action { get; set; }
            private Action P3Action { get; set; }
            private Action P4Action { get; set; }

            public String Name { get; set; }

            public ButtonLink(Action p1, Action p2, Action p3, Action p4, String name)
                : this() //May be unecessary.
            {
                P1Action = p1;
                P2Action = p2;
                P3Action = p3;
                P4Action = p4;
                Name = name;

            }

            public Action GetAction(int player)
            {
                switch (player)
                {
                    case 1:
                        return P1Action;
                    case 2:
                        return P2Action;
                    case 3:
                        return P3Action;
                    case 4:
                        return P4Action;
                }

                return Action.NONE;
            }
        }

    }
}
