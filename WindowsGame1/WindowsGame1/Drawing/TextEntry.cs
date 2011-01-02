using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WGiBeat.Drawing
{
    public class TextEntry : DrawableObject
    {
        private StringBuilder result = new StringBuilder();
        public event EventHandler EntryComplete;
        public event EventHandler EntryCancelled;

        public Color TextColour = Color.Black;
        public string DescriptionText = "";

        public string EnteredText
        {
            get { return result.ToString(); }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var myPosition = this.Position.Clone();

            myPosition.X += (this.Width/2.0f);
            TextureManager.DrawString(spriteBatch, result.ToString(), "LargeFont", myPosition,
                          TextColour, FontAlign.CENTER);
            myPosition.Y += 25;
            foreach (string line in DescriptionText.Split('\n'))
            {
                TextureManager.DrawString(spriteBatch, line, "DefaultFont", myPosition,
                                          TextColour, FontAlign.CENTER);
                myPosition.Y += 25;
            }
           
            TextureManager.DrawString(spriteBatch, "Press enter when done, or escape to cancel.", "DefaultFont", myPosition,
                                      TextColour, FontAlign.CENTER);

        }

        public void PerformKey(Keys key)
        {
            
            switch (key)
            {
                case Keys.Back:
                    if (result.Length <= 0)
                    {
                        return;
                    }
                    result.Remove(result.Length - 1, 1);
                    break;
                case Keys.Enter:
                    if (EntryComplete != null)
                    {
                        EntryComplete(this, null);
                    }
                    break;
                case Keys.Escape:
                    if (EntryCancelled != null)
                    {
                        EntryCancelled(this, null);
                    }
                    break;
                default:
                    var temp = key.ToString().ToLower();
                    if (temp.Length > 1)
                    {
                        temp = ResolveSpecialKey(temp);
                    }
                    result.Append(temp);
                    break;
            }
        }

        private string ResolveSpecialKey(string temp)
        {
            if ((temp[0] == 'd') && (Char.IsDigit(temp[1])))
            {
                return temp[1] + "";
            }
            if (temp.StartsWith("space"))
            {
                return " ";
            }
            if (temp.StartsWith("numpad"))
            {
                return "" + temp[6];
            }
            if (temp == "oemperiod")
            {
                return ".";
            }
            if (temp == "oemcomma")
            {
                return ",";
            }
            return "";
        }

        public void Clear()
        {
            result = new StringBuilder();
        }
    }
}
