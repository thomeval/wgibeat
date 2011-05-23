using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class SongListItem : DrawableObject
    {
        public GameSong Song { get; set; }
        public bool IsSelected { get; set; }
        private int _textMaxWidth = 9999;
        public int TextMaxWidth
        {
            get { return _textMaxWidth; }
            set { _textMaxWidth = value; }
        }

        private readonly SpriteMap _itemSpriteMap;
        public byte Opacity;

        public SongListItem()
        {
            _itemSpriteMap = new SpriteMap
                                {Columns = 1, Rows = 2, SpriteTexture = TextureManager.Textures("SongListItem")};
        }

        private Color _textDrawColor = Color.Black;
        public override void Draw(SpriteBatch spriteBatch)
        {
            //Draw Base
            var idx = IsSelected ? 1 : 0;
            _itemSpriteMap.ColorShading.A = Opacity;
            _textDrawColor.A = Opacity;
            _itemSpriteMap.Draw(spriteBatch,idx,this.X,this.Y);
            //Draw Text
            var textPosition = new Vector2(this.X + 20, this.Y + 3);
            Vector2 scale = TextureManager.ScaleTextToFit(Song.Title, "LargeFont", _textMaxWidth, this.Height);
            TextureManager.DrawString(spriteBatch, Song.Title, "LargeFont", textPosition, scale, _textDrawColor, FontAlign.LEFT);
            textPosition.Y += 20;
            scale = TextureManager.ScaleTextToFit(Song.Artist, "DefaultFont", TextMaxWidth, this.Height);
            TextureManager.DrawString(spriteBatch, Song.Artist, "DefaultFont", textPosition, scale, _textDrawColor, FontAlign.LEFT);
            
        }
    }
}
