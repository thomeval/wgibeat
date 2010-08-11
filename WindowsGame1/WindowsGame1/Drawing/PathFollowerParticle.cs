using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace WGiBeat.Drawing
{
    class PathFollowerParticle : DrawableObject
    {
        public Particle Particle { get; set; }
        public Boolean Wrap { get; set; }
        public Double Position { get; set; }
        public Double StepSize { get; set; }

        public List<Vector2> Points { get; set; }
        //public LineDrawingType Type { get; set; }

        private List<Double> xCoef { get; set; }
        private List<Double> yCoef { get; set; }

        private static Double CORNER_SNAP_LENGTH = 0.01; //Or something.

        public PathFollowerParticle()
        {
            
        }
        

        public override void Draw(SpriteBatch spriteBatch)
        {
            Particle.Draw(spriteBatch);

            //update position or whatever.
        }

        public void RecalcSegments()
        {
            for (int i = 0; i < Points.Count; i++)
                RecalcSegments(i);
        }

        public void RecalcSegments(int segmentIndexHint)
        {
            
        }

        /* No ways I'm doing this.
        public enum LineDrawingType
        {
            
        }
        */
    }
}
