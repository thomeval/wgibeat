using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class InstructionScreen : GameScreen
    {
        public int PageNumber = 1;
        public const int TOTAL_PAGES = 2;
        public bool FirstScreen;
        private Sprite _baseSprite;
        private Sprite[] _instructionPages;

        public InstructionScreen(GameCore core) : base(core)
        {
        }

        public override void Initialize()
        {
            
            InitSprites();
            PageNumber = 1;
            base.Initialize();
        }

        private void InitSprites()
        {
            _instructionPages = new Sprite[TOTAL_PAGES];
            for (int x = 0; x < TOTAL_PAGES; x++)
            {
                _instructionPages[x] = new Sprite {SpriteTexture = TextureManager.Textures["InstructionPage" + (x + 1)]};
            }
            _baseSprite = new Sprite { SpriteTexture = TextureManager.Textures["LoadingMessageBase"] };
            _baseSprite.SetPosition(Core.Metrics["LoadMessageBase", 0]);
           
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
          
            _baseSprite.Draw(spriteBatch);
            _instructionPages[PageNumber-1].Draw(spriteBatch);
            TextureManager.DrawString(spriteBatch, "Press start to continue.", "LargeFont", Core.Metrics["LoadMessage", 0], Color.White, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("Page {0} of {1}",PageNumber, TOTAL_PAGES), "DefaultFont", Core.Metrics["LoadErrorCount", 0], Color.White, FontAlign.LEFT);

        }

 

        public override void PerformAction(Action action)
        {
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            var firstLoad = Core.Cookies.ContainsKey("FirstScreen") && (bool) Core.Cookies["FirstScreen"];
            string nextScreen = firstLoad ? "InitialLoad" : "MainMenu";
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
