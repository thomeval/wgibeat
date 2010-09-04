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
        public Sprite Particle { get; set; }
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

        private double _partX = 0.0;
        private double _partY = 0.0;

        public PathFollowerParticle()
        {
            Init();
        }



        private void Init()
        {
            InitDefaults();

            if (Points.Count > 0)
            {
                SegmentIndex = PositionToSegment();
                RecalcSegments();
            }
                //MoveSize = TotalLength / (1 / StepSize);
                //CORNER_SNAP_LENGTH = MoveSize / 6;
                //MoveSize = 50;
            
        }
        
        private void InitDefaults()
        {

            SegmentIndex = 0;

            Particle = new Sprite() //temp
            {
                SpriteTexture = TextureManager.Textures["Particle_1"]
            };

            Wrap = true;
            Position = 0.0;
            StepSize = 0.001;
            Points = new List<Vector2>(4);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            Particle.Draw(spriteBatch);

            if (Points.Count > 0)
                IncrementPosition();
            else
            {
                Console.WriteLine("No points");
            }

        }

        private void IncrementPosition()
        {

            Vector2 pointFrom = Points[SegmentIndex];
            Vector2 pointTo = Points[SegmentIndex + 1];


            //Console.WriteLine(MoveSize * XCoef[SegmentIndex]);
            //Console.WriteLine(MoveSize * YCoef[SegmentIndex]);

            _partX = (_partX - (MoveSize * XCoef[SegmentIndex]));
            _partY = (_partY - (MoveSize * YCoef[SegmentIndex]));
            Particle.X = (int) _partX;
            Particle.Y = (int) _partY;
            //Console.WriteLine(Particle.X + ":" + Particle.Y);

            Vector2 curVector = new Vector2(Particle.X, Particle.Y);


            if (CalcDistance(curVector, pointFrom) < CORNER_SNAP_LENGTH)
            {
                Particle.SetPosition(pointFrom);
            }

            Console.WriteLine("Dist = " + CalcDistance(curVector, pointTo));
            if (CalcDistance(curVector, pointTo) < 2) //CORNER_SNAP_LENGTH)
            {
                Console.WriteLine("Changed segment");
                Particle.SetPosition(pointTo);

                SegmentIndex += 1;
                SegmentIndex = SegmentIndex % (Segments.Count - 1);
                //Fix position
            } else
                //Console.WriteLine(SegmentIndex = PositionToSegment());

                Console.WriteLine(Position);
            Position += StepSize;
            if (Position >= 1)
            {
                Position = 0;
                SegmentIndex = 0;

                _partX = Points[0].X;
                _partY = Points[0].Y;
                Particle.X = (int)_partX;
                Particle.Y = (int)_partY;
            }

        //SegmentIndex = PositionToSegment();

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


        private Boolean CalcSnap()
        {
            //if 
            return false; //NOT IMPLEMENTED
        }

        public void RecalcSegments()
        {
            Particle.X = (int)Points[0].X;
            Particle.Y = (int)Points[0].Y;
            _partX = Points[0].X;
            _partY = Points[0].Y;
            CORNER_SNAP_LENGTH = MoveSize / 6;
            var a  = Points.Count;
            Segments = new List<double>((Points.Count - (Points.Count % 2)));
            XCoef = new List<double>(Segments.Count);
            YCoef = new List<double>(Segments.Count);

            for (int i = 0; i < a; i ++)
            {
                Segments.Add(0.0);
                XCoef.Add(0.0);
                YCoef.Add(0.0);
            }

            Double totalLength = 0;
            for (int i = 0; i < Segments.Count - 1; i++) //Working
                totalLength += RecalcSegment(i);

            TotalLength = totalLength;
            MoveSize = TotalLength / (1 / StepSize);
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
