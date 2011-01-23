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

        private Sprite _backgroundSprite;
        private static SpriteMap _controllerNumberSpriteMap;

        public ActionBinding()
        {
            InitSprites();
        }
        private void InitSprites()
        {
            _backgroundSprite = new Sprite { SpriteTexture = TextureManager.Textures["ActionBindingBase"]};
            _controllerNumberSpriteMap = new SpriteMap
                                                           {
                                                               Columns = 4,
                                                               Rows = 1,
                                                               SpriteTexture = TextureManager.Textures["KeyOptionControllerPlayerIcons"]
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
                _controllerNumberSpriteMap.Draw(spriteBatch,ControllerNumber - 1,30,30,_iconPosition);
                TextureManager.DrawString(spriteBatch, Button.ToString(), "LargeFont", _textPosition, Color.Black, FontAlign.LEFT);
            }
            else
            {
                TextureManager.DrawString(spriteBatch,Key.ToString(),"LargeFont",_textPosition, Color.Black,FontAlign.LEFT);
            }

        }
    }
}
