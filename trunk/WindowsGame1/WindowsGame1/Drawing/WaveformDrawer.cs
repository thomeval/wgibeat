using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class WaveformDrawer : DrawableObject
    {

        public Color ColorShading;

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, new float[1]);
        }

        public void Draw(SpriteBatch spriteBatch, float[] levels)
        {
         
          
                var line = new PrimitiveLine(GameCore.Instance.GraphicsDevice)
                {
                    Colour = this.ColorShading,
                    Position = this.Position,
                    Width = 5
                };

                double posX = 0;


                for (int x = 0; x < levels.Count(); x++)
                {
                    line.AddVector(new Vector2((int) posX, this.Height * levels[x]));
                    posX += 1.0f * this.Width / (levels.Length-1);
                }

                line.Render(spriteBatch);

            }

    }
}
