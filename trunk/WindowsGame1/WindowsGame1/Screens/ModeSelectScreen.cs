﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;

namespace WGiBeat.Screens
{
    public class ModeSelectScreen : GameScreen
    {
        private int _selectedGameType = 0;

         public ModeSelectScreen(GameCore core)
             : base(core)
        {
            BuildMenu();
        }

        private void BuildMenu()
        {

        }

        public override void Initialize()
        {

            base.Initialize();
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawPlayerDifficulties(spriteBatch);

            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"],"" + (GameType) _selectedGameType, new Vector2(150,250),Color.Black);
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

        private void DrawPlayerDifficulties(SpriteBatch spriteBatch)
        {
            var frameSpriteMap = new SpriteMap
            {
                Columns = 4,
                Rows = 1,
                SpriteTexture = TextureManager.Textures["playerDifficultiesFrame"]
            };
            var iconSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = 5,
                SpriteTexture = TextureManager.Textures["playerDifficulties"]
            };

            //Draw for all players, even if not playing.
            for (int x = 0; x < 4; x++)
            {
                frameSpriteMap.Draw(spriteBatch, x, 50, 100, Core.Metrics["PlayerDifficultiesFrame", x]);
                int idx = GetPlayerDifficulty(x);
                iconSpriteMap.Draw(spriteBatch, idx, 40, 40, Core.Metrics["PlayerDifficulties", x]);
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