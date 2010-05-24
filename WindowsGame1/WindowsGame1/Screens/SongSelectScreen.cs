using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class SongSelectScreen : GameScreen
    {
        private List<SongListItem> SongList = new List<SongListItem>();
        private int _selectedIndex = 0;

        private const int LISTITEMS_DRAWN = 6;
        public SongSelectScreen(GameCore core) : base(core)
        {

        }


        public override void Initialize()
        {
            if (SongList.Count == 0)
            {
                CreateSongList();
            }
            if (Core.Settings.Exists("LastSongPlayed"))
            {
                var lastSongHash = Core.Settings.Get<int>("LastSongPlayed");
                var lastSong = (from e in SongList where e.Song.GetHashCode() == lastSongHash select e).FirstOrDefault();
                if (lastSong != null)
                {
                    _selectedIndex = SongList.IndexOf(lastSong);
                }
            }
            base.Initialize();
        }

        private void CreateSongList()
        {
            foreach (GameSong song in Core.Songs.AllSongs())
            {
                SongList.Add(new SongListItem {Height = 50, Song = song, Width = 350});
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawSongList(spriteBatch);
            DrawPlayerDifficulties(spriteBatch);
            var headerSprite = new Sprite
                                   {
                                       Height = 80,
                                       Width = 800,
                                       SpriteTexture = TextureManager.Textures["songSelectHeader"]
                                   };
            headerSprite.SetPosition(Core.Metrics["SongSelectScreenHeader",0]);
            headerSprite.Draw(spriteBatch);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], SongList[_selectedIndex].Song.Bpm + " BPM", Core.Metrics["SongBPMDisplay", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "High score: " + Core.Songs.GetHighScore(SongList[_selectedIndex].Song.GetHashCode(), Core.Settings.Get<GameType>("CurrentGameType")), Core.Metrics["SongHighScore", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "Mode: " + Core.Settings.Get<GameType>("CurrentGameType"), Core.Metrics["SelectedMode", 0], Color.Black);



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
                frameSpriteMap.Draw(spriteBatch, x,50,100,Core.Metrics["PlayerDifficultiesFrame",x]);
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

            return 1 + (int) (Core.Players[player].PlayDifficulty);
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

        private void DrawSongList(SpriteBatch spriteBatch)
        {

            DrawBackground(spriteBatch);

            var midpoint = Core.Metrics["SongListMidpoint", 0];
            SongList[_selectedIndex].SetPosition(midpoint);
            SongList[_selectedIndex].IsSelected = true;
            SongList[_selectedIndex].Draw(spriteBatch);

            //Draw SongListItems below (after) the selected one.
            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                midpoint.Y += 50;
                SongList[(_selectedIndex + x) % SongList.Count].IsSelected = false;
                SongList[(_selectedIndex + x) % SongList.Count].SetPosition(midpoint);
                SongList[(_selectedIndex + x) % SongList.Count].Draw(spriteBatch);
            }
         
            midpoint.Y -= 50 * LISTITEMS_DRAWN;
            int index = _selectedIndex;

            //Draw SongListItems above (before) the selected one.
            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                index -= 1;
                if (index < 0)
                {
                    index = SongList.Count - 1;
                }
                midpoint.Y -= 50;
                SongList[index].IsSelected = false;
                SongList[index].SetPosition(midpoint);
                SongList[index].Draw(spriteBatch);
            }
        }


        public override void PerformAction(Action action)
        {
            switch (action)
            {
                case Action.P1_UP:
                case Action.P2_UP:
                case Action.P3_UP:
                case Action.P4_UP:
                    MoveSelectionUp();
                    break;
                case Action.P1_DOWN:
                case Action.P2_DOWN:
                case Action.P3_DOWN:
                case Action.P4_DOWN:
                    MoveSelectionDown();
                    break;
                case Action.P1_START:
                case Action.P2_START:
                case Action.P3_START:
                case Action.P4_START:
                    StartSong();
                    break;
                case Action.SYSTEM_BACK:
                    Core.ScreenTransition("NewGame");
                    break;

            }
        }

        private void StartSong()
        {
            Core.Settings.Set("CurrentSong",SongList[_selectedIndex].Song);
            Core.Settings.Set("LastSongPlayed", SongList[_selectedIndex].Song.GetHashCode());
            Core.ScreenTransition("MainGame");
        }

        private void MoveSelectionUp()
        {
            _selectedIndex -= 1;
            if (_selectedIndex < 0)
            {
                _selectedIndex = SongList.Count - 1;
            }

        }

        private void MoveSelectionDown()
        {
            _selectedIndex = (_selectedIndex + 1)%SongList.Count();

        }
    }
}
