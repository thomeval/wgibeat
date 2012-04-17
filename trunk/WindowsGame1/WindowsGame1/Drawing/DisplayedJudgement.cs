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
                _judgementSprite = new SpriteMap {Columns =(int) BeatlineNoteJudgement.COUNT, Rows = 1, SpriteTexture = TextureManager.Textures("NoteJudgements")};
            }
            _judgementSprite.ColorShading.A = Opacity;

            _judgementSprite.Draw(spriteBatch,Tier,this.Width, this.Height,this.X,this.Y);
            DrawStreakCounter(spriteBatch, 1.0);
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
