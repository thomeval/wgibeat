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
        private int _selectedItem;

        public Menu()
        {
            _selectedItem = 0;
            _menuItems = new List<MenuItem>();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int xOptionOffset = CalculateXOptionOffset();
            var position = new Vector2 {X = this.X, Y = this.Y};
            foreach (MenuItem menuItem in _menuItems)
            {
                Color drawColor = (IsSelected(menuItem)) ? Color.Blue : Color.Black;

                    spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], menuItem.ItemText, position, drawColor);
                position.X += xOptionOffset;
                    spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], menuItem.SelectedText(),position, drawColor);
                position.X -= xOptionOffset;

                position.Y += 25;
            }
        }

        private bool IsSelected(MenuItem item)
        {
            return _menuItems[_selectedItem] == item;
        }

        private int CalculateXOptionOffset()
        {
            int maxLength = 0;
            foreach (MenuItem menuItem in _menuItems)
            {
                maxLength = Math.Max(maxLength, menuItem.ItemText.Length);
            }
            return (maxLength * 15) + 10;
        }


        public MenuItem SelectedItem()
        {
            return  _menuItems[_selectedItem];
        }

        public void IncrementSelected()
        {
            if (_menuItems.Count > 0)
            {
                _selectedItem += 1;
                _selectedItem %= _menuItems.Count;
            }
        }

        public void DecrementSelected()
        {
            if (_menuItems.Count > 0)
            {
                _selectedItem -= 1;
                if (_selectedItem < 0)
                {
                    _selectedItem = _menuItems.Count - 1;
                }
            }
        }

        public void AddItem(MenuItem item)
        {
            _menuItems.Add(item);
        }

        public void DecrementOption()
        {
            if (_menuItems.Count > 0)
            {
                _menuItems[_selectedItem].DecrementSelected();
            }
        }
        public void IncrementOption()
        {
            if (_menuItems.Count > 0)
            {
                _menuItems[_selectedItem].IncrementSelected();
            }
        }

        public MenuItem GetByItemText(string difficulty)
        {
            return (from e in _menuItems where e.ItemText == difficulty select e).SingleOrDefault();
        }
    }
}
