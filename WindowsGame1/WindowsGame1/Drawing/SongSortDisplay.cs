using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;

namespace WGiBeat.Drawing
{
    public class SongSortDisplay : DrawableObject
    {

        public bool Active { get; set; }
        private byte _activeOpacity;

        public SongSortMode SongSortMode { get; set;}
        
        private Sprite _backgroundSprite;
        private Sprite _listBackgroundSprite;
        private SpriteMap _arrowSprites;
        private Vector2 _textPosition;

        private Menu _bookmarkMenu;
        private int _selectedBookmarkIndex;
        private int _bookmarkTextSize = 15;

        public int VisibleBookmarks = 12;

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

            _bookmarkMenu.Draw(spriteBatch);
        }


        public void MoveSort(int amount)
        {
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
        }

        public void MoveCurrentBookmark(int amount)
        {
            _selectedBookmarkIndex += amount;
            _selectedBookmarkIndex = Math.Max(0, _selectedBookmarkIndex);
            _selectedBookmarkIndex = Math.Min(_bookmarkMenu.ItemCount-1, _selectedBookmarkIndex);
            _bookmarkMenu.SelectedIndex = _selectedBookmarkIndex;
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
                                    ItemSpacing = 15,
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

            switch (SongSortMode)
            {
                case SongSortMode.TITLE:
                    for (char c = 'A'; c <= 'Z'; c++ )
                    {
                        result.Add(new MenuItem {ItemText = "" + c});
                    }
                        break;
                case SongSortMode.ARTIST:
                        for (char c = 'A'; c <= 'Z'; c++)
                        {
                            result.Add(new MenuItem { ItemText = "" + c });
                        }
                    break;
                    case SongSortMode.BPM:
                    break;
            }
            return result;
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
