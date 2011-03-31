using System;
using System.Linq;
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
            }

            foreach (SineSwayParticle token in _swayers)
            {
                InitializeSwayer(token);
            }

            var sorted = from s in _swayers 
                         orderby (s.Width + s.Height)
                         select s; //Forgot why I did this.

            _swayers = sorted.ToArray();

        }

        private void InitDefaultRanges() //Add into settings file.
        {

            RandomizeTextures = true;

            MinPosition = 0;
            MaxPosition = 1;

            MinFrequency = 0.01;
            MaxFrequency = 5;

            MinStepSize = 0.01 / 20;
            MaxStepSize = 0.01 / 5;

            MinShift = 0;
            MaxShift = 1;

            MinSize = 3;
            MaxSize = 15;

            MinWidth = 1; //5
            MaxWidth = 30; //30

            MinHeight = 700;
            MaxHeight = 850;

            MinY = -95;
            MaxY = -5;

            MinX = -15;
            MaxX = 800;

            Count = 520;
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

            if (RandomizeTextures) //Hacky. Must fix.
            {
                particle.ParticleType = _rand.Next(0, 5);
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (SineSwayParticle particle in _swayers)
            {
                particle.Draw(spriteBatch);
            }
        }

    }
}
