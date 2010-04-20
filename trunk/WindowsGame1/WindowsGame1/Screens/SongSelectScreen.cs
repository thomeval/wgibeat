using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame1.AudioSystem;
using WindowsGame1.Drawing;

namespace WindowsGame1.Screens
{
    public class SongSelectScreen : GameScreen
    {
        private List<SongListItem> SongList = new List<SongListItem>();
        private int _selectedIndex = 0;

        private const int LISTITEMS_DRAWN = 8;
        public SongSelectScreen(GameCore core) : base(core)
        {

        }


        public override void Initialize()
        {
            CreateSongList();
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
            var midpoint = Core.Metrics["SongListMidpoint", 0];
            SongList[_selectedIndex].SetPosition(midpoint);
            SongList[_selectedIndex].IsSelected = true;
            SongList[_selectedIndex].Draw(spriteBatch);

            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                midpoint.Y += 50;
                SongList[(_selectedIndex + x) % SongList.Count].IsSelected = false;
                SongList[(_selectedIndex + x) % SongList.Count].SetPosition(midpoint);
                SongList[(_selectedIndex + x) % SongList.Count].Draw(spriteBatch);
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
            }
        }

        private void StartSong()
        {
            Core.Settings.Set("CurrentSong",SongList[_selectedIndex].Song);
            Core.Settings.Set("LastSongPlayed", SongList[_selectedIndex].Song.Title);
            Core.ScreenTransition("MainGame");
        }

        private void MoveSelectionUp()
        {
            //SongList[_selectedIndex].IsSelected = false;
            _selectedIndex -= 1;
            if (_selectedIndex < 0)
            {
                _selectedIndex = SongList.Count - 1;
            }

        }

        private void MoveSelectionDown()
        {
           // SongList[_selectedIndex].IsSelected = false;
            _selectedIndex = (_selectedIndex + 1)%SongList.Count();

        }
    }
}
