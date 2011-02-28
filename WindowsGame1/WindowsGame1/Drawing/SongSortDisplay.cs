﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;

namespace WGiBeat.Drawing
{
    public class SongSortDisplay : DrawableObject
    {

        public bool Active { get; set; }
        private byte _activeOpacity;

        private bool _initiated;
        private int SelectedSongHash { get; set;}

        private SongSortMode _songSortMode;
        public SongSortMode SongSortMode
        {
            get { return _songSortMode; }
            set 
            {
                _songSortMode = value;
                SortSongList();
            }
        }

        private void SortSongList()
        {

            switch (SongSortMode)
            {
                case SongSortMode.TITLE:
                    SongList.Sort(SortByName);
                    break;
                case SongSortMode.ARTIST:
                    SongList.Sort(SortByArtist);
                    break;
                case SongSortMode.BPM:
                    SongList.Sort(SortByBpm);
                    break;
            }
 
        }

        private int SortByName(SongListItem first, SongListItem second)
        {
            return first.Song.Title.CompareTo(second.Song.Title);
        }
        private int SortByArtist(SongListItem first, SongListItem second)
        {
            return first.Song.Artist.CompareTo(second.Song.Artist);
        }
        private int SortByBpm(SongListItem first, SongListItem second)
        {
            return first.Song.Bpm.CompareTo(second.Song.Bpm);
        }

    
        public List<SongListItem> SongList
        {
            get; set;
        }


        private Sprite _backgroundSprite;
        private Sprite _listBackgroundSprite;
        private SpriteMap _arrowSprites;
        private Vector2 _textPosition;

        private Menu _bookmarkMenu;
        private int _selectedBookmarkIndex;
        private int _bookmarkTextSize = 18;

        public int VisibleBookmarks = 10;
        private int _lastSongHash;

        public int SelectedSongIndex
        {
            get; set;
        }

        public SongSortDisplay()
        {
            this.Width = 300;
            this.Height = 50;
        }
        public void InitSprites()
        {
            _backgroundSprite = new Sprite {SpriteTexture = TextureManager.Textures("SongSortBackground")};
            _arrowSprites = new SpriteMap {SpriteTexture = TextureManager.Textures("IndicatorArrows"), Columns=4, Rows = 1};
            _listBackgroundSprite = new Sprite {SpriteTexture = TextureManager.Textures("SongSortListBackground")};
            _textPosition = new Vector2();
            _bookmarkMenu = new Menu();
 
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                _activeOpacity = (byte) Math.Min(_activeOpacity + 10, 255);
            }
            else
            {
                _activeOpacity = (byte) Math.Max(_activeOpacity - 10, 0);
            }

            SetSpritePositions();
            _backgroundSprite.Draw(spriteBatch);
            _textPosition.X = this.X + (this.Width/2);
            _textPosition.Y = this.Y;
            TextureManager.DrawString(spriteBatch,"" + SongSortMode, "TwoTechLarge",_textPosition,Color.Black, FontAlign.CENTER);

            _arrowSprites.ColorShading.A = _activeOpacity;
            _arrowSprites.Draw(spriteBatch, 1, 35, 35, this.X + 15, this.Y + 8);
            _arrowSprites.Draw(spriteBatch, 0, 35, 35, this.X + this.Width - 40, this.Y + 8);

            if (Active)
            {
                 DrawList(spriteBatch);
            }
        }

        private void SetSpritePositions()
        {
            _backgroundSprite.SetPosition(this.X,this.Y);
        }

        private void DrawList(SpriteBatch spriteBatch)
        {
            _listBackgroundSprite.Height = 40 + (_bookmarkTextSize * VisibleBookmarks);
            _listBackgroundSprite.Width = 75;
            _listBackgroundSprite.SetPosition(this.X + this.Width - 75, this.Y + this.Height);
            _listBackgroundSprite.Draw(spriteBatch);

            if (!_initiated)
            {
                _initiated = true;
                CreateBookmarkMenu();
            }
            _bookmarkMenu.Draw(spriteBatch);
        }


        public void MoveSort(int amount)
        {
            _lastSongHash = SongList[SelectedSongIndex].GetHashCode();
            var current = (int) SongSortMode + amount;
            const int COUNT = (int) SongSortMode.COUNT;
            if (current < 0)
            {
                current += COUNT;
            }
            if (current >= COUNT)
            {
                current -= COUNT;
            }

            SongSortMode = (SongSortMode) current;
            CreateBookmarkMenu();
            SelectedSongIndex = SongList.IndexOf(GetByHashCode(_lastSongHash));
        }

        private SongListItem GetByHashCode(int hash)
        {
            return (from e in SongList where e.GetHashCode() == hash select e).FirstOrDefault();
        }

        public void MoveCurrentBookmark(int amount)
        {
            _selectedBookmarkIndex += amount;
            _selectedBookmarkIndex = Math.Max(0, _selectedBookmarkIndex);
            _selectedBookmarkIndex = Math.Min(_bookmarkMenu.ItemCount-1, _selectedBookmarkIndex);
            _bookmarkMenu.SelectedIndex = _selectedBookmarkIndex;
            JumpToBookmark();
        }

        private void JumpToBookmark()
        {
           
            switch (SongSortMode)
            {
                case SongSortMode.TITLE:
                    SelectedSongIndex = JumpBookmarkTitle(_bookmarkMenu.SelectedItem().ItemValue.ToString());
                    break;
                    case SongSortMode.ARTIST:
                    SelectedSongIndex = JumpBookmarkArtist(_bookmarkMenu.SelectedItem().ItemValue.ToString());
                    break;
                    case SongSortMode.BPM:
                    SelectedSongIndex = JumpBookmarkBPM(_bookmarkMenu.SelectedItem().ItemValue.ToString());
                    break;
            }
        }


        private int JumpBookmarkTitle(string start)
        {
            char startChar;
            if (start == "#")
            {
                for (int x = 0; x < SongList.Count; x++)
                {
                    if (Char.IsDigit(SongList[x].Song.Title[0]))
                    {
                        return x;
                    }
                }
            }
            if (start == "@")
            {
                for (int x = 0; x < SongList.Count; x++)
                {
                    if (Char.IsSymbol(SongList[x].Song.Title[0]))
                    {
                        return x;
                    }
                }
            }

            startChar = start[0];

            for (int x = 0; x < SongList.Count; x++)
            {
                if (SongList[x].Song.Title.ToUpperInvariant()[0] >= startChar)
                {
                    return x;
                }
            }
            return SongList.Count - 1;
        }
        private int JumpBookmarkArtist(string start)
        {
            char startChar;
            if (start == "#")
            {
                for (int x = 0; x < SongList.Count; x++)
                {
                    if (Char.IsDigit(SongList[x].Song.Artist[0]))
                    {
                        return x;
                    }
                }
            }
            if (start == "@")
            {
                for (int x = 0; x < SongList.Count; x++)
                {
                    if (Char.IsSymbol(SongList[x].Song.Artist[0]))
                    {
                        return x;
                    }
                }
            }

           startChar = start[0];


            for (int x = 0; x < SongList.Count; x++)
            {

                if (SongList[x].Song.Artist.ToUpperInvariant()[0] == startChar)
                {
                    return x;
                }
            }
            return SongList.Count - 1;
        }
        private int JumpBookmarkBPM(string start)
        {
            double startBpm = Convert.ToDouble(start, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

            for (int x = 0; x < SongList.Count; x++)
            {
                if (SongList[x].Song.Bpm >= startBpm)
                {
                    return x;
                }
            }
            return SongList.Count - 1;
        }
        public bool PerformAction(InputAction action)
        {
            if (!Active)
            {
                return false;
            }

            switch (action.Action)
            {
                case "LEFT":
                    MoveSort(-1);
                    break;
                case "RIGHT":
                    MoveSort(1);
                    break;
                case "UP":
                    MoveCurrentBookmark(-1);
                    break;
                case "DOWN":
                    MoveCurrentBookmark(1);
                    break;
            }

         
            return true;
        }

        private void CreateBookmarkMenu()
        {
            _bookmarkMenu = new Menu
                                {
                                    MaxVisibleItems = VisibleBookmarks,
                                    FontName = "DefaultFont",
                                    Position = _listBackgroundSprite.Position.Clone(),
                                    ItemSpacing = _bookmarkTextSize,
                                    Width = _listBackgroundSprite.Width
                                };
            foreach (MenuItem item in CreateBookmarks())
            {
                _bookmarkMenu.AddItem(item);
            }
            _selectedBookmarkIndex = 0;
        
        }

        private IEnumerable CreateBookmarks()
        {
            var result = new List<MenuItem>();
            char[] validChars = {};


            switch (SongSortMode)
            {
                case SongSortMode.TITLE:
                    validChars = (from e in SongList select e.Song.Title.ToUpper()[0]).Distinct().ToArray();
                        break;
                case SongSortMode.ARTIST:
                       validChars = (from e in SongList select e.Song.Title.ToUpper()[0]).Distinct().ToArray();
                    break;
                    case SongSortMode.BPM:
                    result = CreateBPMBookmarks();
                    break;
            }

            if (ContainsNumber(validChars))
            {
                result.Add(new MenuItem { ItemText = "0-9", ItemValue = "#" });
            }
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (validChars.Contains(c))
                {
                    result.Add(new MenuItem { ItemText = "" + c, ItemValue = "" + c });
                }
            }
            if (ContainsSymbol(validChars))
            {
                result.Add(new MenuItem {ItemText = "Sym", ItemValue = "@"});
            }

            return result;
        }

        private readonly string[] _bpmTexts = {
                                         "Slow", "80", "90", "100", "110", "120", "135", "150", "165", "180", "200", "Fast"
                                     };

        private readonly int[] _bpmValues = {0,80,90,100,110,120,135,150,165,180,200,999};
        private List<MenuItem> CreateBPMBookmarks()
        {
            var result = new List<MenuItem>();
            for (int x = 0; x < _bpmTexts.Length; x++)
            {
                result.Add(new MenuItem{ItemText = _bpmTexts[x],ItemValue = _bpmValues[x]});
            }
            return result;
        }

        private bool ContainsSymbol(IEnumerable<char> chars)
        {
            foreach (char c in chars)
            {
                if (Char.IsSymbol(c))
                {
                    return true;
                }
                
            }
            return false;
        }

        private bool ContainsNumber(IEnumerable<char> chars)
        {
            foreach (char c in chars)
            {
                if (Char.IsNumber(c))
                {
                    return true;
                }

            }
            return false;
        }

        public void SetBookmark(int index)
        {
            object value = "";
            switch (SongSortMode)
            {
                case SongSortMode.TITLE:
                    value = SongList[index].Song.Title.ToUpperInvariant()[0] + "";
                    value = CheckForSymbolOrDigit(value.ToString());
                    break;
                case SongSortMode.ARTIST:
                    value = SongList[index].Song.Artist.ToUpperInvariant()[0] + "";
                    break;
                case SongSortMode.BPM:
                    var temp = SongList[index].Song.Bpm;
                    value = (from e in _bpmValues where e >= temp select e).First();                   
                    break;
            }

            _bookmarkMenu.SetSelectedByValue(value);
            _selectedBookmarkIndex = _bookmarkMenu.SelectedIndex;
        }

        private string CheckForSymbolOrDigit(string value)
        {
            if (Char.IsDigit(value[0]))
            {
                return "#";
            }
            if (Char.IsSymbol(value[0]))
            {
                return "@";
            }
            return value;
        }
    }
       public enum SongSortMode
    {
        TITLE = 0,
        ARTIST = 1,
        BPM = 2,
        COUNT = 3
    }
}
