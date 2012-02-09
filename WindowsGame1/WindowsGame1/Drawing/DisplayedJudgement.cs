using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    public class DisplayedJudgement : DrawableObject
    {
        public double DisplayUntil { get; set; }
        public byte Opacity { get; set; }
        public int Player { get; set; }
        public int Tier { get; set; }

        public int Streak { get; set; }

        public Vector2 TextPosition { get; set; }

        private SpriteMap _judgementSprite;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_judgementSprite == null)
            {
                _judgementSprite = new SpriteMap {Columns = 1, Rows = (int) BeatlineNoteJudgement.COUNT, SpriteTexture = TextureManager.Textures("NoteJudgements")};
            }
            _judgementSprite.ColorShading.A = Opacity;
            var opacityScale = 0.8 + (1.0*Opacity/1500);
            var xloss = (int) (this.Width*(1 - opacityScale)/2);
 
            _judgementSprite.Draw(spriteBatch,Tier,(int) (this.Width * opacityScale), (int) (this.Height * opacityScale),this.X + xloss,this.Y);
            DrawStreakCounter(spriteBatch, opacityScale);
        }

        public void DrawStreakCounter(SpriteBatch spriteBatch, double opacityScale)
        {
            var streakColor = new Color(10, 123, 237, 255);

            if (Streak < 2)
            {
                return;
            }
            if (Tier != 0)
            {
                return;
            }

            var scale = new Vector2((float)opacityScale);
            streakColor.A = Opacity;

            TextureManager.DrawString(spriteBatch, "x" + Streak, "TwoTechLarge",
                                      TextPosition, scale, streakColor, FontAlign.CENTER);

        }
    }
}
