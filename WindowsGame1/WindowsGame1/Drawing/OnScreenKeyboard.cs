using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
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
        private SpriteMap3D _specialChars;
        private SpriteMap3D _barSideSpriteMap;
        private Sprite3D _barMiddleSprite;

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
            _specialChars = new SpriteMap3D
                                {Columns = 3, Rows = 1, Texture = TextureManager.Textures("KeyboardIcons")};
            _barSideSpriteMap = new SpriteMap3D
            {
                Texture = TextureManager.Textures("TextEntryBarSide"),
                Rows = 2,
                Columns = 1
            };
            _barMiddleSprite = new Sprite3D { Texture = TextureManager.Textures("TextEntryBarMiddle") };
        }

        public override void Draw()
        {
            DrawEnteredTextBar(this.Position.Clone());
            var enteredTextPosition = this.Position.Clone();
            enteredTextPosition.X += this.Width/2;
           FontManager.DrawString(EnteredText,"TwoTech24", enteredTextPosition, BaseColor,FontAlign.Center);

            DrawKeyboard();
        }

        private void DrawEnteredTextBar(Vector2 position)
        {
            position.X += 25;
            _barSideSpriteMap.Draw( 0, position);
            position.X += 35;
            _barMiddleSprite.Width = this.Width - 120;
            _barMiddleSprite.Position = position;
            _barMiddleSprite.Draw();
            position.X += this.Width - 120;
            _barSideSpriteMap.Draw( 1, position);

        }

        private void DrawKeyboard()
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
                    FontManager.DrawString("" +c, "TwoTech24", drawPosition, HighlightColor, FontAlign.Left);
                }
                else
                {
                    FontManager.DrawString("" + c, "TwoTech24", drawPosition, BaseColor,FontAlign.Left);                  
                }
                counter++;
            }

            for (int x = 0; x < 3; x++)
            {
                drawPosition.X = (initialPosition.X) + (SpacingX * (counter % Columns));
                drawPosition.Y = (initialPosition.Y) + (SpacingY * (counter / Columns));
                _specialChars.ColorShading = (_selectedIndex == _chars.Count() + x) ? HighlightColor : BaseColor;
                _specialChars.Draw(x,25,25, (int) drawPosition.X, (int) drawPosition.Y);
                counter++;
            }
        }

        public void MoveSelection(string dir)
        {
            switch (dir)
            {
                case "LEFT":
                    
                    _selectedIndex--;
                    if (_selectedIndex < 0)
                    {
                        _selectedIndex = _totalItems - 1;
                    }
                    break;
                case "RIGHT":
                    _selectedIndex++;
                    if (_selectedIndex >= _totalItems)
                    {
                        _selectedIndex = 0;
                    }
                    break;
                case "UP":
                    _selectedIndex -= this.Columns;
                    if (_selectedIndex < 0)
                    {
                        _selectedIndex += _totalItems;
                    }
                    break;
                case "DOWN":
                    _selectedIndex += this.Columns;
                    if (_selectedIndex >= _totalItems)
                    {
                        _selectedIndex -= _totalItems;
                    }
                    break;
                case "START":
                    PickSelection();
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
