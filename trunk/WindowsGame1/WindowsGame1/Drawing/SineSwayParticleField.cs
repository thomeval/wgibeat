using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class SineSwayParticleField : DrawableObject
    {
        private SineSwayParticle[] _swayers;
        public int Count { get; set; }

        public Double MinPosition { get; set; }
        public Double MaxPosition { get; set; }

        public Double MinFrequency { get; set; }
        public Double MaxFrequency { get; set; }

        public Double MinStepSize { get; set; }
        public Double MaxStepSize { get; set; }

        public Double MinShift { get; set; }
        public Double MaxShift { get; set; }

        public int MinWidth { get; set; }
        public int MaxWidth { get; set; }

        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }

        public int MinX { get; set; } public int MinY { get; set; }
        public int MaxX { get; set; } public int MaxY { get; set; }

        public int MinSize { get; set; }
        public int MaxSize { get; set; }



        private Random _rand = new Random();

        public SineSwayParticleField(int x, int y, int width, int height)
        {
            InitDefaultRanges();
            InitializeSwayers();
        }

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
                         select s;

            _swayers = sorted.ToArray();

        }

        private void InitDefaultRanges()
        {
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

            MinWidth = 15;
            MaxWidth = 100;

            MinHeight = 700;
            MaxHeight = 850;

            MinY = -95;
            MaxY = 0;

            MinX = -15;
            MaxX = 800;

            Count = 220;
        }

        private void InitializeSwayer(SineSwayParticle particle)
        {
            particle.ParticleSize = _rand.Next(MinSize, MaxSize);

            particle.Width = _rand.Next(MinWidth, MaxWidth);
            //particle.Height = _rand.Next(MinHeight, MaxHeight);
            particle.Height = _rand.Next(MinHeight, MaxHeight);

            particle.Frequency = MinFrequency + (_rand.NextDouble() * (MaxFrequency - MinFrequency));
            particle.Shift = MinShift + (_rand.NextDouble() * (MaxShift - MinShift));
            particle.Position = MinPosition + (_rand.NextDouble() * (MaxPosition - MinPosition));

            particle.X = _rand.Next(MinX, MaxX);
            particle.Y = _rand.Next(MinY, MaxY);

            particle.StepSize = MinStepSize + (_rand.NextDouble() * (MaxStepSize - MinStepSize));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (SineSwayParticle token in _swayers)
            {
                token.Draw(spriteBatch);
            }
        }

    }
}
