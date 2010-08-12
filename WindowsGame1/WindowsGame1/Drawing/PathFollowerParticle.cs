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
        public Double MoveSize { get; set; }

        public List<Vector2> Points { get; set; }
        
        
        //public LineDrawingType Type { get; set; }

        private List<Double> XCoef { get; set; }
        private List<Double> YCoef { get; set; }
        private List<Double> Segments { get; set; }
        private Double TotalLength { get; set; }
        private int SegmentIndex { get; set; }
        private Double CORNER_SNAP_LENGTH = 0.01; //Or something. StepSize / 4 best guess.



        public PathFollowerParticle()
        {
            Init();
        }

        private void Init()
        {
            SegmentIndex = PositionToSegment();
            RecalcSegments();
            MoveSize = TotalLength / (1 / StepSize);
            CORNER_SNAP_LENGTH = MoveSize / 6;
        }
        

        public override void Draw(SpriteBatch spriteBatch)
        {

            Particle.Draw(spriteBatch);

            IncrementPosition();

        }

        private void IncrementPosition()
        {
            try
            {
                Vector2 pointFrom = Points[SegmentIndex];
                Vector2 pointTo = Points[SegmentIndex + 1];

                Particle.X = (int) (pointFrom.X + (MoveSize * XCoef[SegmentIndex]));
                Particle.Y = (int) (pointFrom.Y + (MoveSize * XCoef[SegmentIndex]));


                Vector2 curVector = new Vector2(Particle.X, Particle.Y);


                if (CalcDistance(curVector, pointFrom) < CORNER_SNAP_LENGTH)
                {
                    Particle.SetPosition(pointFrom);
                }

                if (CalcDistance(curVector, pointTo) < CORNER_SNAP_LENGTH)
                {
                    Particle.SetPosition(pointTo);
                    SegmentIndex += 1;
                    //Fix position
                }

                Position += StepSize;
                if (Position >= 0)
                    Position = 0;

  
            }
            catch
            {}
        }



        private Double CalcDistance(Vector2 p1, Vector2 p2)
        {
            return Math.Sqrt(Math.Pow((p1.X - p2.X), 2) + Math.Pow(p1.Y - p2.Y, 2));
        }


        private int PositionToSegment()
        {
            Double curLength = 0;
            Double targetPosition = TotalLength * Position;

            for (int i = 0; i < Points.Count; i++)
            {
                if (targetPosition < curLength)
                    return i;

                curLength += Math.Sqrt(Math.Pow((Points[i].X - Points[i + 1].X), 2) + Math.Pow(Points[i].Y - Points[i + 1].Y, 2));
            }

            return -1; //Something went really really bad here :p
        }


        public Boolean CalcSnap()
        {
            //if 
            return false; //NOT IMPLEMENTED
        }

        public void RecalcSegments()
        {
            Double totalLength = 0;
            for (int i = 0; i < Points.Count; i++) //Working
                totalLength += RecalcSegment(i);

            TotalLength = totalLength;
            
        }

        public Double RecalcSegment(int segmentIndexHint)
        {
            if ((segmentIndexHint >= 0) && (segmentIndexHint < Points.Count))
            {
                Vector2 temp1 = Points[segmentIndexHint];
                Vector2 temp2 = Points[segmentIndexHint + 1];

                double horiDif = Math.Abs(temp1.X - temp2.X); //Meh... More elegant, damn it!!
                double vertDif = Math.Abs(temp1.Y - temp2.Y);

                Segments[segmentIndexHint] = Math.Sqrt(Math.Pow(vertDif, 2) + Math.Pow(horiDif, 2));

                XCoef[segmentIndexHint] = (temp1.X - temp2.X) / Segments[segmentIndexHint];
                YCoef[segmentIndexHint] = (temp1.Y - temp2.Y) / Segments[segmentIndexHint];

                return Segments[segmentIndexHint];

            }

            throw new IndexOutOfRangeException("Index " + segmentIndexHint + " is out of the valid range.");
        }

        /* No ways I'm doing this.
        public enum LineDrawingType
        {
            
        }
        */
    }
}
