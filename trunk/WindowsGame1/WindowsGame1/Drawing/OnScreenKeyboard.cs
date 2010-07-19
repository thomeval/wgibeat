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
        private char[] _chars = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8','9','_',(char) 13};
        public string EnteredText { get; set; }
        public bool Active { get; set; }
        public Vector2 EnteredTextPosition { get; set; }

        public Color HighlightColor { get; set; }
        public Color BaseColor  {get; set;}
        public int Columns { get; set; }
        public int SpacingX { get; set; }
        public int SpacingY { get; set; }
        private int _selectedIndex;

        public OnScreenKeyboard()
        {
            EnteredText = "";
            Active = false;
            EnteredTextPosition = new Vector2(0, 0);
            HighlightColor = Color.Blue;
            BaseColor = Color.Black;
            Columns = 10;
            SpacingX = 40;
            SpacingY = 30;
            
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
           spriteBatch.DrawString(TextureManager.Fonts["TwoTech"],EnteredText, EnteredTextPosition, BaseColor);

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
            drawPosition.Y += 30;
        }

        public void MoveSelection(NoteDirection dir)
        {
            switch (dir)
            {
                case NoteDirection.LEFT:
                    _selectedIndex--;
                    if (_selectedIndex < 0)
                    {
                        _selectedIndex = _chars.Count() - 1;
                    }
                    break;
                case NoteDirection.RIGHT:
                    _selectedIndex++;
                    if (_selectedIndex >= _chars.Count())
                    {
                        _selectedIndex = 0;
                    }
                    break;
                case NoteDirection.UP:
                    break;
                case NoteDirection.DOWN:
                    break;
            }
        }

          
    }
}
