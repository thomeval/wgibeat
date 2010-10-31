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
        public Menu()
        {
            SelectedIndex = 0;
            _menuItems = new List<MenuItem>();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (MaxVisibleItems == 0)
            {
                MaxVisibleItems = 999;
            }
            int xOptionOffset = CalculateXOptionOffset();
            var position = new Vector2 {X = this.X, Y = this.Y};

            var midpoint = MaxVisibleItems/2;
            var startItem = Math.Max(0,SelectedIndex - midpoint);
            var lastItem = Math.Min(_menuItems.Count - 1, SelectedIndex + midpoint);

            
            while ((lastItem < _menuItems.Count -1) && ((lastItem - startItem + 1) < MaxVisibleItems))
            {
                lastItem++;
            }

            while ((startItem > 0) && ((lastItem - startItem + 1) < MaxVisibleItems))
            {
                startItem--;
            }

            for (int i = startItem; i <= lastItem; i++)
            {
                MenuItem menuItem = _menuItems[i];
                Color drawColor = (IsSelected(menuItem)) ? Color.Blue : Color.Black;

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], menuItem.ItemText, position, drawColor);
                position.X += xOptionOffset;
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], menuItem.SelectedText(), position, drawColor);
                position.X -= xOptionOffset;

                position.Y += 25;
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
                maxLength = Math.Max(maxLength, menuItem.ItemText.Length);
            }
            return (maxLength * 15) + 10;
        }


        public MenuItem SelectedItem()
        {
            return  _menuItems[SelectedIndex];
        }

        public void IncrementSelected()
        {
            if (_menuItems.Count > 0)
            {
                SelectedIndex += 1;
                SelectedIndex %= _menuItems.Count;
            }
        }

        public void DecrementSelected()
        {
            if (_menuItems.Count > 0)
            {
                SelectedIndex -= 1;
                if (SelectedIndex < 0)
                {
                    SelectedIndex = _menuItems.Count - 1;
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
    }
}
