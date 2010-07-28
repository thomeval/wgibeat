using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    public class OnScreenKeyboard : DrawableObject
    {
        private readonly char[] _chars = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8','9','_'};
        public string EnteredText { get; set; }
        public Vector2 EnteredTextPosition { get; set; }

        public Color HighlightColor { get; set; }
        public Color BaseColor  {get; set;}
        public int Columns { get; set; }
        public int SpacingX { get; set; }
        public int SpacingY { get; set; }
        public int MaxLength { get; set; }
        private int _selectedIndex;
        private SpriteMap _specialChars;

        public event EventHandler EntryComplete;
        public event EventHandler EntryCancelled;

        private int _totalItems {
            get { return _chars.Count() + 3; }
        }
        public OnScreenKeyboard()
        {
            EnteredText = "";
            EnteredTextPosition = new Vector2(0, 0);
            HighlightColor = Color.Blue;
            BaseColor = Color.Black;
            Columns = 10;
            SpacingX = 40;
            SpacingY = 30;

            InitSprites();
        }

        private void InitSprites()
        {
            _specialChars = new SpriteMap
                                {Columns = 3, Rows = 1, SpriteTexture = TextureManager.Textures["KeyboardIcons"]};
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
           TextureManager.DrawString(spriteBatch,EnteredText,"TwoTech", EnteredTextPosition, BaseColor,FontAlign.CENTER);

            DrawKeyboard(spriteBatch);
        }

        private void DrawKeyboard(SpriteBatch spriteBatch)
        {
            var drawPosition = new Vector2(this.X, this.Y);
            int counter = 0;
            foreach (char c in _chars)
            {
                drawPosition.X = (this.X) + (SpacingX*(counter%Columns));
                drawPosition.Y = (this.Y) + (SpacingY*(counter/Columns));
                if (counter == _selectedIndex)
                {
                    spriteBatch.DrawString(TextureManager.Fonts["TwoTech"], c.ToString(), drawPosition, HighlightColor);
                }
                else
                {
                    spriteBatch.DrawString(TextureManager.Fonts["TwoTech"], c.ToString(), drawPosition, BaseColor);                  
                }

                counter++;
            }
            drawPosition.Y += SpacingY;

            for (int x = 0; x < 3; x++)
            {
                _specialChars.ColorShading = (_selectedIndex == _chars.Count() + x) ? HighlightColor : BaseColor;
                _specialChars.Draw(spriteBatch,x,25,25, (SpacingX * x) + 5, (int) drawPosition.Y);
            }
        }

        public void MoveSelection(NoteDirection dir)
        {
            switch (dir)
            {
                case NoteDirection.LEFT:
                    _selectedIndex--;
                    if (_selectedIndex < 0)
                    {
                        _selectedIndex = _totalItems - 1;
                    }
                    break;
                case NoteDirection.RIGHT:
                    _selectedIndex++;
                    if (_selectedIndex >= _totalItems)
                    {
                        _selectedIndex = 0;
                    }
                    break;
                case NoteDirection.UP:
                    _selectedIndex -= this.Columns;
                    if (_selectedIndex < _totalItems)
                    {
                        _selectedIndex += _totalItems;
                    }
                    break;
                case NoteDirection.DOWN:
                    _selectedIndex += this.Columns;
                    if (_selectedIndex >= _totalItems)
                    {
                        _selectedIndex -= _totalItems;
                    }
                    break;
            }
        }

        public void PickSelection()
        {
            if (_selectedIndex < _chars.Count())
            {
                if (EnteredText.Length < MaxLength)
                {
                    EnteredText += _chars[_selectedIndex];
                }
            }
            //Backspace
            if (_selectedIndex == _chars.Count())
            {
                if (EnteredText.Length > 0)
                {
                    EnteredText = EnteredText.Substring(0, EnteredText.Length - 1);
                }
            }
            //Cancel
            if (_selectedIndex == _chars.Count() + 1)
            {
                if (EntryCancelled != null)
                {
                    EntryCancelled(this, null);
                }
            }
            //Decide
            if (_selectedIndex == _chars.Count() + 2)
            {
                if (EntryComplete != null)
                {
                    EntryComplete(this, null);
                }
            }
        }  
    }
}
