using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WGiBeat.Drawing
{
    public class ActionBinding : DrawableObject
    {
        public int ControllerNumber { get; set; }
        public Keys Key { get; set; }
        public Buttons Button { get; set; }

        private static Sprite _backgroundSprite;
        private static Sprite _keyboardIcon;
        private static SpriteMap _controllerNumberSpriteMap;
        private static SpriteMap _controllerButtonsSpriteMap;

        private readonly Buttons[] _buttonsLookup = {
                                               Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LeftShoulder,
                                               Buttons.RightShoulder, Buttons.LeftTrigger, Buttons.RightTrigger,
                                               Buttons.Back, Buttons.Start, Buttons.DPadDown, Buttons.DPadLeft,
                                               Buttons.DPadUp, Buttons.DPadRight, Buttons.LeftStick,
                                               Buttons.LeftThumbstickDown, Buttons.LeftThumbstickLeft,
                                               Buttons.LeftThumbstickUp, Buttons.LeftThumbstickRight, Buttons.RightStick, 
                                               Buttons.RightThumbstickDown, Buttons.RightThumbstickLeft,
                                               Buttons.RightThumbstickUp, Buttons.RightThumbstickRight
                                           };
        public ActionBinding()
        {
            InitSprites();
        }
        private void InitSprites()
        {
            _backgroundSprite = new Sprite { SpriteTexture = TextureManager.Textures("ActionBindingBase")};
            _controllerNumberSpriteMap = new SpriteMap
                                                           {
                                                               Columns = 4,
                                                               Rows = 1,
                                                               SpriteTexture = TextureManager.Textures("KeyOptionControllerPlayerIcons")
                                                           };
            _controllerButtonsSpriteMap = new SpriteMap
                                              {
                                                  Columns = 5,
                                                  Rows = 5,
                                                  SpriteTexture =
                                                      TextureManager.Textures("KeyOptionControllerButtonIcons")
                                              };
            _keyboardIcon = new Sprite
                                {
                                    SpriteTexture = TextureManager.Textures("KeyOptionKeyboardIcon"),
                                    Width = 35,
                                    Height = 30
                                };
        }

        private Vector2 _textPosition;
        private Vector2 _iconPosition;
        public override void Draw(SpriteBatch spriteBatch)
        {
           
            _backgroundSprite.Width = this.Width;
            _backgroundSprite.Height = this.Height;
            _backgroundSprite.Position = this.Position;
            _backgroundSprite.Draw(spriteBatch);

            //Determine positions.
            _textPosition = this.Position.Clone();
            _textPosition.X += 50;
            _textPosition.Y += 10;
            _iconPosition = this.Position.Clone();
            _iconPosition.X += 10;
            _iconPosition.Y += 7;

            //Draw controller Icon
            if (ControllerNumber > 0)
            {
                _controllerNumberSpriteMap.Draw(spriteBatch, ControllerNumber - 1, 30, 30, _iconPosition);
                if (_buttonsLookup.Contains(Button))
                {
                    _textPosition.Y -= 3;
                    var btnIndex = _buttonsLookup.IndexOf(Button);
                    _controllerButtonsSpriteMap.Draw(spriteBatch,btnIndex,30,30,_textPosition);
                }
                else
                {
                    TextureManager.DrawString(spriteBatch, Button.ToString(), "LargeFont", _textPosition, Color.Black, FontAlign.LEFT);
                }
                
            }
            else
            {
                _keyboardIcon.Position = _iconPosition;
                _keyboardIcon.Draw(spriteBatch);
                TextureManager.DrawString(spriteBatch,Key.ToString(),"LargeFont",_textPosition, Color.Black,FontAlign.LEFT);
            }

        }
    }
}
