using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame1.AudioSystem;

namespace WindowsGame1.Drawing
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
            Vector2 textPosition = new Vector2(this.X + 20, this.Y + 3);
            spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], Song.Title, textPosition,Color.White);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"],Song.Artist, textPosition, Color.White);
            
        }
    }
}
