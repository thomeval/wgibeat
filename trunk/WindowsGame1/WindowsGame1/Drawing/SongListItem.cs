using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class SongListItem : DrawableObject
    {
        public GameSong Song { get; set; }
        public bool IsSelected { get; set; }

        private readonly SpriteMap _itemSpriteMap;

        public SongListItem()
        {
            _itemSpriteMap = new SpriteMap
                                {Columns = 1, Rows = 2, SpriteTexture = TextureManager.Textures("SongListItem")};
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            
            //Draw Base
            var idx = IsSelected ? 1 : 0;
            _itemSpriteMap.Draw(spriteBatch,idx,this.X,this.Y);
            //Draw Text
            var textPosition = new Vector2(this.X + 20, this.Y + 3);
            spriteBatch.DrawString(TextureManager.Fonts("LargeFont"), Song.Title, textPosition,Color.Black);
            textPosition.Y += 20;
            spriteBatch.DrawString(TextureManager.Fonts("DefaultFont"), Song.Artist, textPosition, Color.Black);
            
        }
    }
}
