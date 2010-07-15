﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class ModeSelectScreen : GameScreen
    {
        private int _selectedGameType = 0;
        private SpriteMap _optionBaseSpriteMap;
        private SpriteMap _optionsSpriteMap;
        private SpriteMap _frameSpriteMap;
        private SpriteMap _iconSpriteMap;
        private Sprite _background;
        private Sprite _headerSprite;

         public ModeSelectScreen(GameCore core)
             : base(core)
        {
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
                SpriteTexture = TextureManager.Textures["allBackground"],
                Width = Core.Window.ClientBounds.Width,
                X = 0,
                Y = 0
            };

            _headerSprite = new Sprite { SpriteTexture = TextureManager.Textures["modeSelectHeader"] };

            _optionBaseSpriteMap = new SpriteMap
            {
                Columns = 2,
                Rows = 1,
                SpriteTexture = TextureManager.Textures["modeOptionBase"]
            };

            _optionsSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = (int)GameType.COUNT,
                SpriteTexture = TextureManager.Textures["modeOptions"]
            };


            _frameSpriteMap = new SpriteMap
                                 {
                                     Columns = 4,
                                     Rows = 1,
                                     SpriteTexture = TextureManager.Textures["playerDifficultiesFrame"]
                                 };

            _iconSpriteMap = new SpriteMap
                                {
                                    Columns = 1,
                                    Rows = (int)Difficulty.COUNT + 1,
                                    SpriteTexture = TextureManager.Textures["playerDifficulties"]
                                };
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawPlayerDifficulties(spriteBatch);

            _headerSprite.SetPosition(Core.Metrics["ModeSelectScreenHeader", 0]);
            _headerSprite.Draw(spriteBatch);

            DrawModeOptions(spriteBatch);
           
        }

        private void DrawModeOptions(SpriteBatch spriteBatch)
        {
            var posX = (int) Core.Metrics["ModeSelectOptions", 0].X;
            var posY = (int) Core.Metrics["ModeSelectOptions", 0].Y;

            for (int x = 0; x < (int) GameType.COUNT; x++)
            {
                int selected = (x == _selectedGameType) ? 1 : 0;
                _optionBaseSpriteMap.Draw(spriteBatch, selected,270,270,posX,posY);

                _optionsSpriteMap.Draw(spriteBatch,x,248,248,posX + 11, posY + 11);
                posX += 275;
            }
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            _background.Draw(spriteBatch);
        }

        private void DrawPlayerDifficulties(SpriteBatch spriteBatch)
        {
            int playerCount = 0;
            for (int x = 0; x < 4; x++)
            {
                if (Core.Players[x].Playing)
                {
                    _frameSpriteMap.Draw(spriteBatch, x, 50, 100, Core.Metrics["PlayerDifficultiesFrame", playerCount]);
                    int idx = GetPlayerDifficulty(x);
                    _iconSpriteMap.Draw(spriteBatch, idx, 40, 40, Core.Metrics["PlayerDifficulties", playerCount]);
                    playerCount++;
                }
            }
        }

        private int GetPlayerDifficulty(int player)
        {
            if (!Core.Players[player].Playing)
            {
                return 0;
            }

            return 1 + (int)(Core.Players[player].PlayDifficulty);
        }

        public override void PerformAction(Action action)
        {
            switch (action)
            {
                case Action.P1_LEFT:
                case Action.P2_LEFT:
                case Action.P3_LEFT:
                case Action.P4_LEFT:
                    _selectedGameType -= 1;
                    if (_selectedGameType < 0)
                    {
                        _selectedGameType = (int) GameType.COUNT - 1;
                    }
                    break;
                case Action.P1_RIGHT:
                case Action.P2_RIGHT:
                case Action.P3_RIGHT:
                case Action.P4_RIGHT:
                    _selectedGameType += 1;
                    if (_selectedGameType == (int) GameType.COUNT)
                    {
                        _selectedGameType = 0;
                    }
                    break;
                case Action.P1_UP:
                case Action.P2_UP:
                case Action.P3_UP:
                case Action.P4_UP:

                    break;
                case Action.P1_DOWN:
                case Action.P2_DOWN:
                case Action.P3_DOWN:
                case Action.P4_DOWN:

                    break;
                case (Action.SYSTEM_BACK):
                    Core.ScreenTransition("NewGame");
                    break;

                case Action.P1_START:
                case Action.P2_START:
                case Action.P3_START:
                case Action.P4_START:
                case Action.P1_BEATLINE:
                case Action.P2_BEATLINE:
                case Action.P3_BEATLINE:
                case Action.P4_BEATLINE:
                    DoAction();
                    break;
                   
            }
        }

        private void DoAction()
        {
            Core.Settings.Set("CurrentGameType",(GameType) _selectedGameType);
            Core.ScreenTransition("SongSelect");
        }
    }
}