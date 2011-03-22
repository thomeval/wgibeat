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
            _audioTypeSpriteMap = new SpriteMap {SpriteTexture = TextureManager.Textures("AudioTypes"), Columns = 1, Rows = _audioTypes.Length + 1 };
            _songTypeSpriteMap = new SpriteMap { SpriteTexture = TextureManager.Textures("SongTypes"), Columns = 1, Rows = _songTypes.Length + 1 };
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

        private SpriteMap _audioTypeSpriteMap;
        private SpriteMap _songTypeSpriteMap;

        private readonly string[] _audioTypes = {".mp3", ".ogg", ".wma", ".wav"};
        private readonly string[] _songTypes = {".sng", ".sm", ".dwi"};

        int _songIndex;
        int _audioIndex;

        private void SetIndexes()
        {
            _songIndex = _songTypes.IndexOf(Path.GetExtension(Song.DefinitionFile)) + 1;
            _audioIndex = _audioTypes.IndexOf(Path.GetExtension(Song.AudioFile)) + 1;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var position = this.Position.Clone();
            _audioTypeSpriteMap.Draw(spriteBatch,_audioIndex,position);
            position.Y += 1;
            _songTypeSpriteMap.Draw(spriteBatch,_songIndex,position);
        }
    }
}
