using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoundLineCode;

namespace WGiBeat.Drawing
{
    public class WaveformDrawer : DrawableObject
    {
        public Color ColorShading;
        private RoundLineManager _roundLine;
        private List<RoundLine> _lineList;

        public void Init()
        {
            _roundLine = RoundLineManager.Instance;
            _lineList = new List<RoundLine>();
        }
        public override void Draw()
        {
            Draw(new float[1]);
        }

        public void Draw(float[] levels)
        {
            if (levels.Length < 2)
            {
                return;
            }
            _lineList.Clear();
            var step = 1.0f * this.Width / (levels.Length - 1);
            float posX = 0;


            for (int x = 1; x < levels.Count(); x++)
            {
                var p0 = new Vector2(posX, (this.Height * levels[x - 1]));
                var p1 = new Vector2(posX + step, (this.Height * levels[x]));
                p0 += this.Position;
                p1 += this.Position;

                _lineList.Add(new RoundLine(p0, p1));
                posX += step;
            }

            _roundLine.Draw(_lineList,2,ColorShading,0,null);
        }

    }
}
