using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class SineSwayParticleField : DrawableObject
    {
        private SineSwayParticle[] _swayers;
        public int Count { get; set; }

        public double MinPosition { get; set; }
        public double MaxPosition { get; set; }

        public double MinFrequency { get; set; }
        public double MaxFrequency { get; set; }

        public double MinStepSize { get; set; }
        public double MaxStepSize { get; set; }

        public double MinShift { get; set; }
        public double MaxShift { get; set; }

        public int MinWidth { get; set; }
        public int MaxWidth { get; set; }

        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }

        public int MinX { get; set; } public int MinY { get; set; }
        public int MaxX { get; set; } public int MaxY { get; set; }

        public int MinSize { get; set; }
        public int MaxSize { get; set; }

        private readonly Random _rand = new Random();

        private bool RandomizeTextures { get; set; }

        private VertexPositionColorTexture[] _vertices;
  
        public SineSwayParticleField()
        {
            InitDefaultRanges();
            InitializeSwayers();
        }

        private void InitializeSwayers()
        {
            _swayers = new SineSwayParticle[Count];

            for (int i = 0; i < Count; i++)
            {
                _swayers[i] = new SineSwayParticle();
                InitializeSwayer(_swayers[i]);
            }

        }

        private void InitDefaultRanges() 
        {

            RandomizeTextures = true;

            MinPosition = 0;
            MaxPosition = 1;

            MinFrequency = 0.01;
            MaxFrequency = 5;

            MinStepSize = 0.6 / 20;
            MaxStepSize = 0.6 / 5;

            MinShift = 0;
            MaxShift = 1;

            MinSize = 3;
            MaxSize = 15;

            MinWidth = 3; //5
            MaxWidth = 30; //30

            MinHeight = 700;
            MaxHeight = 850;

            MinY = -95;
            MaxY = -5;

            MinX = -15;
            MaxX = 800;

            Count = 520;
            _vertices = new VertexPositionColorTexture[Count*6];
        }

        private void InitializeSwayer(SineSwayParticle particle)
        {
            particle.ParticleSize = _rand.Next(MinSize, MaxSize);

            particle.Frequency = MinFrequency + (_rand.NextDouble() * (MaxFrequency - MinFrequency));
            particle.Shift = MinShift + (_rand.NextDouble() * (MaxShift - MinShift));
            particle.ParticlePosition = MinPosition + (_rand.NextDouble() * (MaxPosition - MinPosition));

            particle.X = _rand.Next(MinX, MaxX);
            particle.Y = _rand.Next(MinY, MaxY);

            particle.Width = _rand.Next(MinWidth, MaxWidth);
            particle.Height = _rand.Next(MinHeight, MaxHeight);
 
            particle.StepSize = MinStepSize + (_rand.NextDouble() * (MaxStepSize - MinStepSize));
            particle.RotationStepSize = 0.75f;

            if (RandomizeTextures)
            {
                particle.ParticleType = _rand.Next(0, 5);
            }
        }

        public void Draw(GameTime gameTime)
        {
            Draw(gameTime,0.0);
        }
        public void Draw(GameTime gameTime, double phraseNumber)
        {
            if (Count == 0)
            {
                return;
            }
            int pos = 0;
            //Calculate vertices for every single particle.
            foreach (SineSwayParticle particle in _swayers)
            {
                var beatOffset = (4* phraseNumber) - Math.Floor(4* phraseNumber);
                if (beatOffset < 0.5)
                {
                    beatOffset = 1 - beatOffset;
                }
                var amount = (int) Math.Min((Math.Tan(beatOffset*Math.PI/2)*1), 15);
                particle.Width += amount;
                var result = particle.GetVertices(gameTime);
                for (int x = 0; x < result.Length; x++ )
                {
                    _vertices[pos + x] = result[x];
                }
                    pos += result.Length;
                particle.Width -= amount;
            }

            //Send the entire batch of vertices to SpriteMap3D for drawing (any Particle object's SpriteMap3D will do).
            _swayers[0].ParticleSpriteMap.DrawVertices(_vertices);
        }


        public override void Draw()
        {
            Draw(new GameTime());
        }
    }
}
