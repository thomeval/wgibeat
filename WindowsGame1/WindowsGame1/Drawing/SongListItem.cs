using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class SongListItem : DrawableObject
    {
        public GameSong Song { get; set; }
        public bool IsSelected { get; set; }
        public override void Draw(SpriteBatch spriteBatch)
        {
            
            //Draw Base
            var baseSprite = new Sprite {Height = this.Height, Width = this.Width, X = this.X, Y = this.Y};
            baseSprite.SpriteTexture = IsSelected ? TextureManager.Textures["mainMenuOptionSelected"] : TextureManager.Textures["mainMenuOption"];
            
            baseSprite.Draw(spriteBatch);
            //Draw Text
            var textPosition = new Vector2(this.X + 20, this.Y + 3);
            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], Song.Title, textPosition,Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], Song.Artist, textPosition, Color.Black);
            
        }
    }
}
