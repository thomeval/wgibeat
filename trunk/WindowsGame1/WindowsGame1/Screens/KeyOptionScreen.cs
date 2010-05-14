using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Microsoft.Xna.Framework.Input;


/* Instructions
 * 
 * 1. Switch to a player by pressing any of his keys.
 * 2. Move to action you wish to change.
 * 3. Press that player's START action button.
 * 4. Press key that you wish to use to perform that action.
 * 5. Voila.
 * 
 */

namespace WGiBeat.Screens
{
    class KeyOptionScreen : GameScreen
    {
        private int _currentPlayer = 1;
        private int _selectedMenuOption = 0;
        private Boolean SelectChange = false;
        private Boolean AvoidNextAction = false;
        //private Boolean LastActionSide = false;

        private ButtonLink[] links =  {
                                         new ButtonLink(Action.P1_LEFT,     Action.P2_LEFT,     Action.P3_LEFT,     Action.P4_LEFT,     "Left"),
                                         new ButtonLink(Action.P1_RIGHT,    Action.P2_RIGHT,    Action.P3_RIGHT,    Action.P4_RIGHT,    "Right"),
                                         new ButtonLink(Action.P1_UP,       Action.P2_UP,       Action.P3_UP,       Action.P4_UP,       "Up"),
                                         new ButtonLink(Action.P1_DOWN,     Action.P2_DOWN,     Action.P3_DOWN,     Action.P4_DOWN,     "Down"),
                                         new ButtonLink(Action.P1_BEATLINE, Action.P2_BEATLINE, Action.P3_BEATLINE, Action.P4_BEATLINE, "Beatline"),
                                         new ButtonLink(Action.P1_START,    Action.P2_START,    Action.P3_START,    Action.P4_START,    "Start"),
                                         new ButtonLink(Action.P1_SELECT,   Action.P2_SELECT,   Action.P3_SELECT,   Action.P4_SELECT,   "Select"),
                                        };
                


        public KeyOptionScreen(GameCore core) : base(core)
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
            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Current player: Player" + _currentPlayer, new Vector2(50, 110), Color.Black);
            DrawMenu(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            var background = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["keyScreenBackground"],
                Width = Core.Window.ClientBounds.Width,
                X = 0,
                Y = 0
            };

            background.Draw(spriteBatch);
        }


        private void DrawMenu(SpriteBatch spriteBatch)
        {

            DrawBackground(spriteBatch);

            Vector2 tempVector1 = new Vector2(0, 50);
            Vector2 tempVector2 = new Vector2(0, 60);

            
            for (int playerOption = 1; playerOption <= 4; playerOption++)
            {
                tempVector1.X = 100 + (170 * (playerOption - 1));
                tempVector2.X = tempVector1.X + 32;

                var menuOptionSprite = new Sprite
                {
                    Height = 50,
                    SpriteTexture = TextureManager.Textures["mainMenuOption"],
                    Width = 160
                };

                menuOptionSprite.SetPosition(tempVector1);
                menuOptionSprite.ColorShading = Color.LawnGreen;

                if (playerOption == _currentPlayer)
                {
                    menuOptionSprite.ColorShading = Color.Tomato;
                    menuOptionSprite.Y = (int) tempVector1.Y + 10;
                    //menuOptionSprite.SpriteTexture = TextureManager.Textures["mainMenuOptionSelected"];
                }

                if ((Math.Abs(playerOption - _currentPlayer) == 1))// || (Math.Abs(playerOption - _currentPlayer) == 3))
                {
                    menuOptionSprite.Y = (int)tempVector1.Y + 5;
                    menuOptionSprite.ColorShading = Color.Yellow;
                }
                
                menuOptionSprite.Draw(spriteBatch);

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Player " + playerOption , tempVector2, Color.Black);

            }

            tempVector1.X = 100;
            tempVector2.X = 120;

            for (int menuOption = 0; menuOption < links.Length; menuOption++)
            {

                tempVector1.Y = 150 + (55 * menuOption);
                tempVector2.Y = tempVector1.Y + 10;

                var menuOptionSprite = new Sprite
                {
                    Height = 50,
                    SpriteTexture = TextureManager.Textures["mainMenuOption"],
                    Width = 200
                };

               

                if (menuOption == _selectedMenuOption)
                    menuOptionSprite.SpriteTexture = TextureManager.Textures["mainMenuOptionSelected"];

                menuOptionSprite.SetPosition(tempVector1);
                menuOptionSprite.Draw(spriteBatch);

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], links[menuOption].name, tempVector2, Color.Black);

            }

            tempVector1.X = 400;
            //tempVector1.Y = 150;

            tempVector2.X = 440;
            //tempVector2.Y = 160;

            Keys[] tempKeyList = Core._keyMappings.GetKeys(links[_selectedMenuOption].getAction(_currentPlayer));

            for (int keyList = 0; keyList < tempKeyList.Length; keyList++)
            {

                /*
 
                */

                tempVector1.Y = 150 + (keyList * 55);
                tempVector2.Y = tempVector1.Y + 10;


                var menuOptionSprite = new Sprite
                {
                    Height = 50,
                    SpriteTexture = TextureManager.Textures["mainMenuOption"],
                    Width = 300
                };

                menuOptionSprite.SetPosition(tempVector1);
                menuOptionSprite.Draw(spriteBatch);
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Key = " + tempKeyList[keyList].ToString(), tempVector2, Color.Black);


                /*

                VertexPositionColor[] temp = {
                                                new VertexPositionColor(new Vector3(310, 150 + (55 * _selectedMenuOption) + 10, 0), Color.Red),
                                                new VertexPositionColor(new Vector3(400, 150 + (keyList * 55) + 10, 0), Color.Red)
                                             };

                int[] lineListIndices = { 0, 1 };


                spriteBatch.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList,
                    temp,
                    0,  // vertex buffer offset to add to each element of the index buffer
                    2,  // number of vertices in pointList
                    lineListIndices,  // the index buffer
                    0,  // first index element to read
                    1   // number of primitives to draw
                    );
                  
                */



            }

        }

        public override void PerformKey(Keys key)
        {
            if (SelectChange)
            {
                if (AvoidNextAction)
                    AvoidNextAction = false;
                else
                {
                    //Console.WriteLine("Perform " + key);
                    Core._keyMappings.SetKey(key, links[_selectedMenuOption].getAction(_currentPlayer));
                    Core._keyMappings.SaveToFile("Keys.conf");

                    SelectChange = false;
                    AvoidNextAction = true;
                }
            }
        }

        public override void PerformAction(Action action)
        {

            /*
            if (action == Action.SYSTEM_BACK)
            {
                Core.ScreenTransition("MainMenu");
                return;
            }


            switch (State.CurrentState)
            {
                case 0:
                    switch (action)
                    {
                        case Action.P1_LEFT:
                        case Action.P2_LEFT:
                        case Action.P3_LEFT:
                        case Action.P4_LEFT:

                            SelectChange = false;
                            AvoidNextAction = false;

                            _currentPlayer--;

                            if (_currentPlayer < 1)
                                _currentPlayer = 4;

                            return;

                        case Action.P1_RIGHT:
                        case Action.P2_RIGHT:
                        case Action.P3_RIGHT:
                        case Action.P4_RIGHT:

                            SelectChange = false;
                            AvoidNextAction = false;

                            _currentPlayer++;

                            if (_currentPlayer > 4)
                                _currentPlayer = 1;
                            return;

                        case Action.P1_START:
                        case Action.P2_START:
                        case Action.P3_START:
                        case Action.P4_START:
                            //Console.WriteLine("Changed to changing");
                            SelectChange = true;
                            AvoidNextAction = true;
                            break;

                    }
                    break;

                case 1:

                    State.CurrentState = 0;

                    break;

            }

            */
            if (action == Action.SYSTEM_BACK)
                Core.ScreenTransition("MainMenu");
            else
            {
                switch (action)
                {
                    case Action.P1_LEFT:
                    case Action.P2_LEFT:
                    case Action.P3_LEFT:
                    case Action.P4_LEFT:

                        SelectChange = false;
                        AvoidNextAction = false;

                        _currentPlayer--;

                        if (_currentPlayer < 1)
                            _currentPlayer = 4;

                        return;

                    case Action.P1_RIGHT:
                    case Action.P2_RIGHT:
                    case Action.P3_RIGHT:
                    case Action.P4_RIGHT:

                        SelectChange = false;
                        AvoidNextAction = false;

                        _currentPlayer++;

                        if (_currentPlayer > 4)
                            _currentPlayer = 1;
                        return;

                    case Action.P1_START:
                    case Action.P2_START:
                    case Action.P3_START:
                    case Action.P4_START:
                        //Console.WriteLine("Changed to changing");
                        SelectChange = true;
                        AvoidNextAction = true;
                        break;

                }


                if (!SelectChange)
                {
                    if (AvoidNextAction)
                        AvoidNextAction = false;
                    {
                        short outTemp;

                        if (!Int16.TryParse(action.ToString()[1] + "", out outTemp))
                            return;
                        
                        int newPlayer = outTemp;

                        if (_currentPlayer != newPlayer)
                            _currentPlayer = newPlayer;
                        else
                        {

                            switch (action)
                            {
                                case Action.P1_UP:
                                case Action.P2_UP:
                                case Action.P3_UP:
                                case Action.P4_UP:
                                    _selectedMenuOption--;

                                    if (_selectedMenuOption < 0)
                                        _selectedMenuOption = links.Length - 1;

                                    break;
                                case Action.P1_DOWN:
                                case Action.P2_DOWN:
                                case Action.P3_DOWN:
                                case Action.P4_DOWN:

                                    _selectedMenuOption++;

                                    if (_selectedMenuOption >= links.Length)
                                        _selectedMenuOption = 0;

                                    break;

                            }
                        }
                    }
                }
            }
        }

        private struct ButtonLink
        {
            private Action P1_action { get; set; }
            private Action P2_action { get; set; }
            private Action P3_action { get; set; }
            private Action P4_action { get; set; }

            public String name { get; set; }

            public ButtonLink(Action p1, Action p2, Action p3, Action p4, String _name) : this() //May be unecessary.
            {
                P1_action = p1;
                P2_action = p2;
                P3_action = p3;
                P4_action = p4;
                name = _name;

            }

            public Action getAction(int Player)
            {
                switch (Player)
                {
                    case 1:
                        return P1_action;
                    case 2:
                        return P2_action;
                    case 3:
                        return P3_action;
                    case 4:
                        return P4_action;
                }

                return Action.NONE;
            }
        }

    }
}
