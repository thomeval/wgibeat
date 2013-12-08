using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    public class DisplayedJudgement : DrawableObject
    {
        public double DisplayUntil { get; set; }
        public byte Opacity { get; set; }
        public int Player { get; set; }
        public int Tier { get; set; }

        public int TextureSet { get; set; }

        private SpriteMap3D _judgementSprite;

        public override void Draw()
        {
            if (_judgementSprite == null)
            {
                _judgementSprite = new SpriteMap3D {Columns =(int) BeatlineNoteJudgement.COUNT, Rows = 1, Texture = TextureManager.Textures("NoteJudgements" + TextureSuffix)};
            }
            _judgementSprite.ColorShading.A = Opacity;

            _judgementSprite.Draw(Tier,this.Width, this.Height,this.X,this.Y);
        }

        private string TextureSuffix
        {
            get
            {
                if (TextureSet < 2)
                {
                    return "";
                }
                return TextureSet + "x";
            }

        }
    }
}
