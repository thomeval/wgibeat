using System.IO;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;

namespace WGiBeat.Drawing
{
    public class SongTypeDisplay : DrawableObject
    {
        public SongTypeDisplay()
        {
            InitSprites();
        }

        private void InitSprites()
        {
            _songTypeBackgroundSpriteMap = new SpriteMap { SpriteTexture = TextureManager.Textures("SongTypeBackgrounds"), Columns = 1, Rows = _songTypes.Length + 1 };
            _songTypeIconSpriteMap = new SpriteMap { SpriteTexture = TextureManager.Textures("SongTypeIcons"), Columns = _songTypes.Length + 1, Rows = 1 };
        }

        private GameSong _song;
        public GameSong Song
        {
            get { return _song; }
            set
            {
                _song = value;
                SetIndexes();
            }
        }

        private SpriteMap _songTypeBackgroundSpriteMap;
        private SpriteMap _songTypeIconSpriteMap;

        private readonly string[] _songTypes = {".sng", ".sm", ".dwi"};

        int _songIndex;

        private void SetIndexes()
        {
            _songIndex = _songTypes.IndexOf(Path.GetExtension(Song.DefinitionFile)) + 1;

        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            var position = this.Position.Clone();
            _songTypeBackgroundSpriteMap.Draw(spriteBatch, _songIndex, this.Width, this.Height, position);
            position.Y += 1;
            position.X += 5;
            _songTypeIconSpriteMap.Draw(spriteBatch, _songIndex, this.Height -1, this.Height - 1, position);

            position.X = this.X + this.Width - 10;
            position.Y = this.Y + 1;
            var audioExt = Path.GetExtension(Song.AudioFile).ToUpper().TrimStart('.');
            var songExt = Path.GetExtension(Song.DefinitionFile).ToUpper().TrimStart('.');
            TextureManager.DrawString(spriteBatch,audioExt,"DefaultFont",position,Color.Black,FontAlign.RIGHT);
            position.Y += 16;
            TextureManager.DrawString(spriteBatch, songExt, "DefaultFont", position, Color.Black, FontAlign.RIGHT);
        }
    }
}
