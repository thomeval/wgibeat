using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame1.Drawing;
using Microsoft.Xna.Framework.Input;


/* Instructions
 * 
 * 1. Switch to a player by pressing any of his keys.
 * 2. Move to action you wish to change.
 * 3. Press that player's START action button.
 * 4. Press key that you wish to use to perform that action.
 * 5. Voila.
 * 
 * Note. Saving to file is not yet working. Some sort of serializing error. Will look into it next."
 */

namespace WindowsGame1.Screens
{
    class KeyOptionScreen : GameScreen
    {
        private int CurrentPlayer = 1;
        private int SelectedMenuOption = 0;
        private Boolean SelectChange = false;
        private Boolean AvoidNextAction = false;

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
            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Current player: Player" + CurrentPlayer , new Vector2(50, 100), Color.White);
            DrawMenu(spriteBatch);
        }


        private void DrawMenu(SpriteBatch spriteBatch)
        {
            Vector2 tempVector1 = new Vector2(200, 0);
            Vector2 tempVector2 = new Vector2(250, 0);

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

               

                if (menuOption == (int)SelectedMenuOption)
                    menuOptionSprite.SpriteTexture = TextureManager.Textures["mainMenuOptionSelected"];

                menuOptionSprite.SetPosition(tempVector1);
                menuOptionSprite.Draw(spriteBatch);

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], links[menuOption].name, tempVector2, Color.Black);

                

                
                    
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
                    Console.WriteLine("Perform " + key.ToString());
                    Core._keyMappings.SetKey(key, links[SelectedMenuOption].getAction(CurrentPlayer));
                    Core._keyMappings.SaveToFile("Keys.conf");

                    SelectChange = false;
                    AvoidNextAction = true;
                }
            }
        }

        public override void PerformAction(Action action)
        {
            if (action == Action.P1_ESCAPE)
                Core.Exit();
            else
            {
                if (!SelectChange)
                {
                    if (AvoidNextAction)
                        AvoidNextAction = false;
                    {
                        int newPlayer = 1;

                        if (action.ToString().Contains("P1"))
                            newPlayer = 1;
                        else if (action.ToString().Contains("P2"))
                            newPlayer = 2;
                        else if (action.ToString().Contains("P3"))
                            newPlayer = 3;
                        else if (action.ToString().Contains("P4"))
                            newPlayer = 4;

                        if (CurrentPlayer != newPlayer)
                            CurrentPlayer = newPlayer;
                        else
                        {

                            switch (action)
                            {
                                case Action.P1_UP:
                                case Action.P2_UP:
                                case Action.P3_UP:
                                case Action.P4_UP:
                                    SelectedMenuOption--;

                                    if (SelectedMenuOption < 0)
                                        SelectedMenuOption = links.Length - 1;

                                    break;
                                case Action.P1_DOWN:
                                case Action.P2_DOWN:
                                case Action.P3_DOWN:
                                case Action.P4_DOWN:

                                    SelectedMenuOption++;

                                    if (SelectedMenuOption >= links.Length)
                                        SelectedMenuOption = 0;

                                    break;
                                case Action.P1_START:
                                case Action.P2_START:
                                case Action.P3_START:
                                case Action.P4_START:
                                    Console.WriteLine("Changed to changing");
                                    SelectChange = true;
                                    AvoidNextAction = true;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private struct ButtonLink
        {
            public Action P1_action { get; set; }
            public Action P2_action { get; set; }
            public Action P3_action { get; set; }
            public Action P4_action { get; set; }

            public String name { get; set; }

            public ButtonLink(Action p1, Action p2, Action p3, Action p4, String _name) : this() //May be unecessary.
            {
                this.P1_action = p1;
                this.P2_action = p2;
                this.P3_action = p3;
                this.P4_action = p4;
                this.name = _name;

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
