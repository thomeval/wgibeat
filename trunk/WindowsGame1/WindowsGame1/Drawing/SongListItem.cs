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

        private readonly SpriteMap3D _itemSpriteMap;
        private readonly SpriteMap3D _clearSpriteMap;
        public byte Opacity;
        public int ClearColour { get; set; }
        public static Vector2 ClearIndicatorSize { get; set; }
        
        private readonly Color _lockedColour = new Color(255,160,160);
        private readonly Color _unlockedColor = new Color(255,255,192);
        
        public SongListItem()
        {
            _itemSpriteMap = new SpriteMap3D
                                {Columns = 1, Rows = 2, Texture = TextureManager.Textures("SongListItem")};
            _clearSpriteMap = new SpriteMap3D{Columns = 1, Rows= 6, Texture = TextureManager.Textures("SongListItemClearIndicator")};
            ClearColour = -1;
        }

        private Color _textDrawColor = Color.Black;
        public static int PlayerLevel { get; set; }
        
        //TODO: Batch draw NonText then Text for performance improvement.
        public override void Draw()
        {
            //Draw Base
            DrawNonText();

            DrawText();
        }
        public void DrawNonText()
        {
            var idx = IsSelected ? 1 : 0;
            _itemSpriteMap.ColorShading = GetBaseColour();

            _itemSpriteMap.ColorShading.A = Opacity;
            _textDrawColor.A = Opacity;
            _itemSpriteMap.Draw(idx, this.Size,this.Position);

            if (ClearColour >= 0)
            {
                _clearSpriteMap.ColorShading.A = Opacity;
                _clearSpriteMap.Draw(ClearColour,ClearIndicatorSize,this.Position);
            }
        }
        public void DrawText()
        {
//Draw Text
            var textPosition = this.Position.Clone();
            textPosition += new Vector2(45,1);
            Vector2 scale = FontManager.ScaleTextToFit(Song.Title, "LargeFont", _textMaxWidth, (int) this.Height);
            FontManager.DrawString(Song.Title, "LargeFont", textPosition, scale, _textDrawColor, FontAlign.Left);
            textPosition.Y += 22;
            scale = FontManager.ScaleTextToFit(Song.Artist, "DefaultFont", TextMaxWidth, (int) this.Height);
            FontManager.DrawString(Song.Artist, "DefaultFont", textPosition, scale, _textDrawColor,
                                      FontAlign.Left);
        }

        private Color GetBaseColour()
        {
            if (Song.RequiredLevel <= 1)
            {
                return Color.White;
            }
            if (PlayerLevel >= Song.RequiredLevel)
            {
                return _unlockedColor;
            }
            return _lockedColour;
        }
    }
}
