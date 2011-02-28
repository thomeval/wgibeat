using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class Menu : DrawableObject
    {
        private readonly List<MenuItem> _menuItems;
        public int SelectedIndex { get; set; }

        public int MaxVisibleItems { get; set; }
        public int ItemSpacing = 25;
        public Color HighlightColor = Color.Blue;
        public Color TextColor = Color.Black;
        public Color SelectedItemBackgroundColor = Color.White;
        private string _fontName = "LargeFont";
        private int _animationOffset;

        private SpriteMap _edgeSpriteMap;
        private SpriteMap _arrowSpriteMap;
        private SpriteMap _sideSpriteMap;
        private Sprite _selectedItemSprite;


        public string FontName
        {
            get { return _fontName; }
            set { _fontName = value; }
        }

        public int ItemCount
        {
            get
            {
                return _menuItems.Count;
            }
        }
        public Menu()
        {
            SelectedIndex = 0;
            _menuItems = new List<MenuItem>();
            InitSprites();
        }

        private void InitSprites()
        {
            _edgeSpriteMap = new SpriteMap{Columns = 1, Rows = 2, SpriteTexture = TextureManager.Textures("MenuEdge")};
            _arrowSpriteMap = new SpriteMap
                                  {Columns = 4, Rows = 1, SpriteTexture = TextureManager.Textures("IndicatorArrows")};
            _sideSpriteMap = new SpriteMap {Columns = 1, Rows = 1, SpriteTexture = TextureManager.Textures("MenuSide")};
            _selectedItemSprite = new Sprite {SpriteTexture = TextureManager.Textures("MenuSelectedItem")};
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            if (MaxVisibleItems == 0)
            {
                MaxVisibleItems = 999;
            }

            var midpoint = MaxVisibleItems/2;
            var startItem = Math.Max(0,SelectedIndex - midpoint);
            var lastItem = Math.Min(_menuItems.Count - 1, SelectedIndex + midpoint);

    
            //Calculate the first and last visible menu item to draw.

            while ((startItem > 0) && ((lastItem - startItem + 1) < MaxVisibleItems))
            {
                startItem--;
            }

            while ((lastItem < _menuItems.Count - 1) && ((lastItem - startItem + 1) < MaxVisibleItems))
            {
                lastItem++;
            }
            //Adjusts start item to fix bug with even number of items.
            if (lastItem - startItem + 1 > MaxVisibleItems)
            {
                startItem++;
            }

            DrawBorder(spriteBatch,startItem, lastItem);
            DrawMenuItems(spriteBatch, startItem, lastItem);

        }

        private void DrawMenuItems(SpriteBatch spriteBatch, int startItem, int lastItem)
        {
            int xOptionOffset = CalculateXOptionOffset();
            var position = new Vector2 { X = this.X + 10, Y = this.Y + 30 };
            for (int i = startItem; i <= lastItem; i++)
            {
                MenuItem menuItem = _menuItems[i];
                Color drawColor;

                if (IsSelected(menuItem))
                {
                    drawColor = HighlightColor;
                    _selectedItemSprite.ColorShading = SelectedItemBackgroundColor;
                    _selectedItemSprite.Position = position.Clone();
                    _selectedItemSprite.Y += 3;
                    _selectedItemSprite.X -= 5;
                    _selectedItemSprite.Width = this.Width - 10;
                    _selectedItemSprite.Height = ItemSpacing + 3;
                    _selectedItemSprite.Draw(spriteBatch);
                }
                else
                {
                    drawColor = TextColor;
                }

                if (!menuItem.Enabled)
                {
                    drawColor = new Color(drawColor, (byte)(drawColor.A / 2));
                }
                TextureManager.DrawString(spriteBatch, menuItem.ItemText, FontName, position, drawColor, FontAlign.LEFT);
                position.X += xOptionOffset;

                var menuOptionText = menuItem.SelectedText();

                var scale = TextureManager.ScaleTextToFit(menuOptionText, FontName, this.Width - 20 - xOptionOffset,
                                                          1000);

                TextureManager.DrawString(spriteBatch, menuOptionText, FontName, position, scale, drawColor, FontAlign.LEFT);


                position.X -= xOptionOffset;

                position.Y += ItemSpacing;
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch, int startItem, int lastItem)
        {
            _edgeSpriteMap.Draw(spriteBatch, 0, this.Width, 30, this.X, this.Y);
            var menuBottom = Math.Min(MaxVisibleItems, _menuItems.Count);
            var bottomPosition = this.Y + (ItemSpacing * menuBottom) + 40;
            _edgeSpriteMap.Draw(spriteBatch, 1, this.Width, 30, this.X, bottomPosition);
            _animationOffset = (int)(_animationOffset * 0.4);

            _sideSpriteMap.Draw(spriteBatch, 0, 5, bottomPosition - this.Y - 30, this.X, this.Y + 30);
            _sideSpriteMap.Draw(spriteBatch, 0, 5, bottomPosition - this.Y - 30, this.X + this.Width - 5, this.Y + 30);

            if (startItem > 0)
            {
                _arrowSpriteMap.Draw(spriteBatch, 2, 25, 25, this.X + (this.Width / 2) - 12, this.Y);
            }
            if (lastItem < _menuItems.Count - 1)
            {
                _arrowSpriteMap.Draw(spriteBatch, 3, 25, 25, this.X + (this.Width / 2) - 12, bottomPosition);
            }
        }

        private bool IsSelected(MenuItem item)
        {
            return _menuItems[SelectedIndex] == item;
        }

        private int CalculateXOptionOffset()
        {
            int maxLength = 0;
            foreach (MenuItem menuItem in _menuItems)
            {
                maxLength = Math.Max(maxLength, (int) TextureManager.Fonts("LargeFont").MeasureString(menuItem.ItemText).X);
            }
            return (maxLength) + 25;
        }


        public MenuItem SelectedItem()
        {
            return  _menuItems[SelectedIndex];
        }

        public void IncrementSelected()
        {
            var temp = SelectedIndex;
            if (_menuItems.Count > 0)
            {
                SelectedIndex += 1;
                SelectedIndex %= _menuItems.Count;
            }
            _animationOffset = (SelectedIndex - temp) * 25;
        }

        public void DecrementSelected()
        {
            var temp = SelectedIndex;
            if (_menuItems.Count > 0)
            {
                SelectedIndex -= 1;
                if (SelectedIndex < 0)
                {
                    SelectedIndex = _menuItems.Count - 1;
                }
            }
            _animationOffset = (SelectedIndex - temp)*25;
        }

        public void AddItem(MenuItem item)
        {
            _menuItems.Add(item);
        }

        public void DecrementOption()
        {
            if (_menuItems.Count > 0)
            {
                _menuItems[SelectedIndex].DecrementSelected();
            }
        }
        public void IncrementOption()
        {
            if (_menuItems.Count > 0)
            {
                _menuItems[SelectedIndex].IncrementSelected();
            }
        }

        public MenuItem GetByItemText(string difficulty)
        {
            return (from e in _menuItems where e.ItemText == difficulty select e).SingleOrDefault();
        }

        public void MoveSelected(int amount)
        {
            for (int x = 0; x < amount; x++)
            {
                IncrementSelected();
            }
            for (int x = 0; x > amount; x--)
            {
                DecrementSelected();
            }
        }

        public void Clear()
        {
            _menuItems.Clear();
            SelectedIndex = 0;
        }

        public void ClearMenuOptions()
        {
            foreach (MenuItem mi in _menuItems)
            {
                mi.ClearOptions();
            }
        }

        public void SetSelectedByValue(object value)
        {
            var selected = (from e in _menuItems where value.Equals(e.ItemValue) select e).FirstOrDefault();
            if (selected != null)
            {
                SelectedIndex = _menuItems.IndexOf(selected);
            }
        }

    }
}
