using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1.Drawing
{
    public class DisplayedJudgement : DrawableObject 
    {
        public Texture2D Texture { get; set;}
        public double DisplayUntil { get; set; }
        public byte Opacity { get; set; }
        public int Player { get; set; }
        public override void Draw(SpriteBatch sb)
        {
            var judgementSprite = new Sprite
                                      {X = this.X, Y = this.Y, Height = 40, Width = 150, SpriteTexture = Texture};
            judgementSprite.ColorShading.A = Opacity;
            judgementSprite.Draw(sb);
        }
    }
}
