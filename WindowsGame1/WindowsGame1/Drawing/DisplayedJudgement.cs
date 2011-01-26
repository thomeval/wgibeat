using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    public class DisplayedJudgement : DrawableObject 
    {
        public double DisplayUntil { get; set; }
        public byte Opacity { get; set; }
        public int Player { get; set; }
        public int Tier {get; set; }
        private SpriteMap _judgementSprite;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_judgementSprite == null)
            {
                _judgementSprite = new SpriteMap {Columns = 1, Rows = (int) BeatlineNoteJudgement.COUNT, SpriteTexture = TextureManager.Textures("noteJudgements")};
            }
            _judgementSprite.ColorShading.A = Opacity;

            _judgementSprite.Draw(spriteBatch,Tier,this.Width, this.Height,this.X,this.Y);
        }
    }
}
