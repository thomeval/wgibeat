using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    public class OnScreenKeyboard : DrawableObject
    {
        private readonly char[] _chars = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8','9','_'};
        public string EnteredText { get; set; }

        public Color HighlightColor { get; set; }
        public Color BaseColor  {get; set;}
        public int Columns { get; set; }
        public int SpacingX { get; set; }
        public int SpacingY { get; set; }
        public int MaxLength { get; set; }
        private int _selectedIndex;
        private SpriteMap _specialChars;
        private SpriteMap _barSideSpriteMap;
        private Sprite _barMiddleSprite;

        public event EventHandler EntryComplete;
        public event EventHandler EntryCancelled;

        private int _totalItems {
            get { return _chars.Count() + 3; }
        }

        public int Id
        {
            get; set;
        }

        public OnScreenKeyboard()
        {
            EnteredText = "";
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
                                {Columns = 3, Rows = 1, SpriteTexture = TextureManager.Textures("KeyboardIcons")};
            _barSideSpriteMap = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures("TextEntryBarSide"),
                Rows = 2,
                Columns = 1
            };
            _barMiddleSprite = new Sprite { SpriteTexture = TextureManager.Textures("TextEntryBarMiddle") };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawEnteredTextBar(spriteBatch,this.Position.Clone());
            var enteredTextPosition = this.Position.Clone();
            enteredTextPosition.X += this.Width/2;
           TextureManager.DrawString(spriteBatch,EnteredText,"TwoTech", enteredTextPosition, BaseColor,FontAlign.CENTER);

            DrawKeyboard(spriteBatch);
        }

        private void DrawEnteredTextBar(SpriteBatch spriteBatch, Vector2 position)
        {
            position.X += 25;
            _barSideSpriteMap.Draw(spriteBatch, 0, position);
            position.X += 35;
            _barMiddleSprite.Width = this.Width - 120;
            _barMiddleSprite.Position = position;
            _barMiddleSprite.Draw(spriteBatch);
            position.X += this.Width - 120;
            _barSideSpriteMap.Draw(spriteBatch, 1, position);

        }

        private void DrawKeyboard(SpriteBatch spriteBatch)
        {
            var initialPosition = this.Position.Clone();
            var drawPosition = this.Position.Clone();
            initialPosition.Y += 35;
            int counter = 0;
            foreach (char c in _chars)
            {
                drawPosition.X = (initialPosition.X) + (SpacingX*(counter%Columns));
                drawPosition.Y = (initialPosition.Y) + (SpacingY*(counter/Columns));
                if (counter == _selectedIndex)
                {
                    TextureManager.DrawString(spriteBatch, c.ToString(), "TwoTech", drawPosition, HighlightColor, FontAlign.LEFT);
                }
                else
                {
                    TextureManager.DrawString(spriteBatch, c.ToString(), "TwoTech", drawPosition, BaseColor,FontAlign.LEFT);                  
                }

                counter++;
            }

            for (int x = 0; x < 3; x++)
            {
                drawPosition.X = (initialPosition.X) + (SpacingX * (counter % Columns));
                drawPosition.Y = (initialPosition.Y) + (SpacingY * (counter / Columns));
                _specialChars.ColorShading = (_selectedIndex == _chars.Count() + x) ? HighlightColor : BaseColor;
                _specialChars.Draw(spriteBatch,x,25,25, (int) drawPosition.X, (int) drawPosition.Y);
                counter++;
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
                    if (_selectedIndex < 0)
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
