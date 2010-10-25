using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class InstructionScreen : GameScreen
    {
        public int PageNumber = 1;
        public const int TOTAL_PAGES = 2;
        public bool FirstScreen;
        public InstructionScreen(GameCore core) : base(core)
        {
        }

        public override void Initialize()
        {
            InitSprites();
            base.Initialize();
        }

        private void InitSprites()
        {
           
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            switch (PageNumber)
            {
                case 1:
                    DrawPage1();
                    break;
                case 2:
                    DrawPage2();
                    break;
            }
        }

        private void DrawPage1()
        {
            
        }
        private void DrawPage2()
        {
            
        }

        public override void PerformAction(Action action)
        {
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            string nextScreen = (bool) Core.Cookies.ContainsKey("FirstScreen") ? "InitialLoad" : "MainMenu";
            switch (paction)
            {
                case "START":
                    if (PageNumber < TOTAL_PAGES)
                    {
                        PageNumber++;
                    }
                    else
                    {
                        Core.Cookies["FirstScreen"] = false;
                        Core.ScreenTransition(nextScreen);
                    }
                    break;
                case "BACK":
                    Core.Cookies["FirstScreen"] = false;
                    Core.ScreenTransition(nextScreen);
                    break;
                   
            }
        }
    }
}
