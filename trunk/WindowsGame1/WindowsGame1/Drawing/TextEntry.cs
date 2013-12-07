using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WGiBeat.Drawing
{
    public class TextEntry : DrawableObject
    {
        private StringBuilder _result = new StringBuilder();
        private Sprite3D _capsLockSprite;
        private Sprite3D _shiftSprite;
        private SpriteMap3D _barSideSpriteMap;
        private Sprite3D _barMiddleSprite;

        public event EventHandler EntryComplete;
        public event EventHandler EntryCancelled;

        private readonly char[] _lowercaseChars = {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '[', ']', ';', ',', '.', '/', '\'', '`','\\'};
        private readonly char[] _uppercaseChars = {'!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '{', '}', ':', '<', '>', '?', '\"', '~','|' };

        public bool Shift { get; set; }
        public bool CapsLock { get; set; }
        public Color TextColour = Color.Black;
        public string DescriptionText = "";

        public TextEntry()
        {
            InitSprites();
        }

        private void InitSprites()
        {
            _capsLockSprite = new Sprite3D { Texture = TextureManager.Textures("TextEntryCaps") };
            _shiftSprite = new Sprite3D { Texture = TextureManager.Textures("TextEntryShift") };
            _barSideSpriteMap = new SpriteMap3D
                                    {
                                        Texture = TextureManager.Textures("TextEntryBarSide"),
                                        Rows = 2,
                                        Columns = 1
                                    };
            _barMiddleSprite = new Sprite3D {Texture = TextureManager.Textures("TextEntryBarMiddle")};
        }

        public string EnteredText
        {
            get { return _result.ToString(); }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var myPosition = this.Position.Clone();

            DrawEnteredTextBar(myPosition.Clone());
            myPosition.X += (this.Width/2.0f);
            
            TextureManager.DrawString(spriteBatch, _result.ToString(), "LargeFont", myPosition,
                          TextColour, FontAlign.Center);
            myPosition.Y += 40;
            foreach (string line in DescriptionText.Split('\n'))
            {
                TextureManager.DrawString(spriteBatch, line, "DefaultFont", myPosition,
                                          TextColour, FontAlign.Center);
                myPosition.Y += 25;
            }
           
            TextureManager.DrawString(spriteBatch, "Press enter when done, or escape to cancel.", "DefaultFont", myPosition,
                                      TextColour, FontAlign.Center);
            myPosition.Y += 25;

            if (Shift)
            {
                _shiftSprite.Y = (int) myPosition.Y;
                _shiftSprite.X = (int) myPosition.X - 50;
                _shiftSprite.Draw();
            }
            if (CapsLock)
            {
                _capsLockSprite.Y = (int)myPosition.Y;
                _capsLockSprite.X = (int)myPosition.X + 50;
               _capsLockSprite.Draw();
            }

        }

        private void DrawEnteredTextBar(Vector2 position)
        {
            position.X += 25;
            _barSideSpriteMap.Draw(0,position);
            position.X += 35;
            _barMiddleSprite.Width = this.Width - 120;
            _barMiddleSprite.Position = position;
            _barMiddleSprite.Draw();
            position.X += this.Width - 120;
            _barSideSpriteMap.Draw(1,position);

        }

        public void PerformKey(Keys key)
        {
            
            switch (key)
            {
                case Keys.Back:
                    if (_result.Length <= 0)
                    {
                        return;
                    }
                    _result.Remove(_result.Length - 1, 1);
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
                case Keys.LeftShift:
                case Keys.RightShift:
                    {
                        Shift = !Shift;
                    }
                    break;
                case Keys.CapsLock:
                    {
                        CapsLock = !CapsLock;
                    }
                    break;
                default:
                    var temp = key.ToString().ToLower();

                    if (temp.Length > 1)
                    {
                        temp = ResolveSpecialKey(temp);
                    }
                    if (temp.Length < 1)
                    {
                        return;
                    }
                    //Use uppercase letters if needed.
                    if (Shift || CapsLock)
                    {
                        temp = temp.ToUpper();
                    }

                    //Mimic Shift behaviour by changing inputs that correspond to Shift-capable keys.
                    if (Shift && _lowercaseChars.Contains(temp[0]))
                    {
                        var idx = Array.IndexOf(_lowercaseChars, temp[0]);
                        temp = "" + _uppercaseChars[idx];
                    }
                        
                    _result.Append(temp);
                    Shift = false;
                    break;
            }
        }

        private readonly string[] _encryptedChars = {"space", "oemperiod", "oemcomma", "oemsemicolon","oemquotes", "oemopenbrackets", "oemclosebrackets", "oemminus", "oemplus","oemquestion", "oemblackslash", "oemtilde", "oembackslash", "oempipe"};
        private readonly char[] _decryptedChars = { ' ', '.', ',', ';', '\'', '[',']', '-', '=','/', '\'','`', '\\','\\' };
        private string ResolveSpecialKey(string temp)
        {
            if ((temp[0] == 'd') && (Char.IsDigit(temp[1])))
            {
                return temp[1] + "";
            }
               if (temp.StartsWith("numpad"))
            {
                return "" + temp[6];
            }
            if (temp.StartsWith("space"))
            {
                return " ";
            }
            if (_encryptedChars.Contains(temp))
            {
                var idx = Array.IndexOf(_encryptedChars, temp);
                return "" + _decryptedChars[idx];
            }

            return "";
        }

        public void Clear()
        {
            _result = new StringBuilder();
        }
    }
}
