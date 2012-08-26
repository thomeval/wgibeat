using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Managers;

namespace WGiBeat.Screens
{
    public class CreditsScreen : GameScreen
    {
        private Sprite3D _background;
        private Sprite3D _baseSprite;
        public int PageNumber = 1;
        public const int TOTAL_PAGES = 2;
        private readonly Sprite3D[] _creditsPages = new Sprite3D[TOTAL_PAGES];
        private const string WEBSITE = "http://code.google.com/p/wgibeat/wiki/SongCredits";
        private SineSwayParticleField _field;
        public CreditsScreen(GameCore core)
            : base(core)
        {

        }

        private void InitSprites()
        {
            _background = new Sprite3D { Texture = TextureManager.Textures("AllBackground") };
            for (int x = 0; x < TOTAL_PAGES; x++)
            {
                _creditsPages[x] = new Sprite3D { Texture = TextureManager.Textures("CreditsPage" + (x + 1)), Size= new Vector2(800,600) };
            }
            _baseSprite = new Sprite3D
                              {
                                  Texture = TextureManager.Textures("LoadingMessageBase"),
                                  Position = (Core.Metrics["LoadMessageBase", 0])
                              };
            _field = new SineSwayParticleField();
        }

        public override void Initialize()
        {
            PageNumber = 1;
            base.Initialize();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_background == null)
            {
                InitSprites();
                Debug.Assert(_background != null);
            }
            _background.Draw();
            _field.Draw(gameTime);
            _baseSprite.Draw();
            _creditsPages[PageNumber - 1].Draw();
            TextureManager.DrawString(spriteBatch, "Press start to continue.", "LargeFont", Core.Metrics["LoadMessage", 0], Color.White, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("Page {0} of {1}", PageNumber, TOTAL_PAGES), "DefaultFont", Core.Metrics["LoadErrorCount", 0], Color.White, FontAlign.LEFT);

        }

        public override void PerformAction(InputAction inputAction)
        {
            switch (inputAction.Action)
            {
                case "START":
                    if (PageNumber < TOTAL_PAGES)
                    {
                        PageNumber++;
                    }
                    else
                    {
                        Core.ScreenTransition("MainMenu");
                    }
                    break;
                case "BACK":
                    Core.ScreenTransition("MainMenu");
                    break;
                case "BEATLINE":
                    var thread = new Thread(LaunchBrowser);
                    thread.Start();
                    break;
            }
        }

        private void LaunchBrowser()
        {
            try
            {
                Process.Start(WEBSITE);
            }
            catch (Exception ex)
            {
                Core.Log.AddMessage(ex.Message, LogLevel.ERROR);
                Core.Log.AddException(ex);
            }
        }
    }
}

