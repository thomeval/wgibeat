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

        private static Sprite3D _backgroundSprite;
        private static Sprite3D _keyboardIcon;
        private static SpriteMap3D _controllerNumberSpriteMap;
        private static SpriteMap3D _controllerButtonsSpriteMap;

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
            _backgroundSprite = new Sprite3D { Texture = TextureManager.Textures("ActionBindingBase")};
            _controllerNumberSpriteMap = new SpriteMap3D
                                                           {
                                                               Columns = 4,
                                                               Rows = 1,
                                                               Texture = TextureManager.Textures("KeyOptionControllerPlayerIcons")
                                                           };
            _controllerButtonsSpriteMap = new SpriteMap3D
                                              {
                                                  Columns = 5,
                                                  Rows = 5,
                                                  Texture =
                                                      TextureManager.Textures("KeyOptionControllerButtonIcons")
                                              };
            _keyboardIcon = new Sprite3D
                                {
                                    Texture = TextureManager.Textures("KeyOptionKeyboardIcon"),
                                    Width = 35,
                                    Height = 30
                                };
        }

        private Vector2 _textPosition;
        private Vector2 _iconPosition;
        public override void Draw()
        {
           
            _backgroundSprite.Width = this.Width;
            _backgroundSprite.Height = this.Height;
            _backgroundSprite.Position = this.Position;
            _backgroundSprite.Draw();

            //Determine positions.
            _textPosition = this.Position.Clone();
            _textPosition.X += 60;
            _textPosition.Y += 13;
            _iconPosition = this.Position.Clone();
            _iconPosition.X += 10;
            _iconPosition.Y += 12;

            //Draw controller Icon
            if (ControllerNumber > 0)
            {
                _controllerNumberSpriteMap.Draw(ControllerNumber - 1, 30, 30, _iconPosition);
                if (_buttonsLookup.Contains(Button))
                {
                    _textPosition.Y -= 3;
                    var btnIndex = _buttonsLookup.IndexOf(Button);
                    _controllerButtonsSpriteMap.Draw(btnIndex,30,30,_textPosition);
                }
                else
                {
                    FontManager.DrawString(Button.ToString(), "LargeFont", _textPosition, Color.Black, FontAlign.Left);
                }
                
            }
            else
            {
                _keyboardIcon.Position = _iconPosition;
                _keyboardIcon.Draw();
                FontManager.DrawString(Key.ToString(),"LargeFont",_textPosition, Color.Black,FontAlign.Left);
            }

        }
    }
}
