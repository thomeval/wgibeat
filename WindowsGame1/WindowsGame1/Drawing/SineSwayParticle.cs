using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class SineSwayParticle : DrawableObject
    {
        public Double ParticlePosition { get; set; } //Between 0 and 1.
        public Double Frequency { get; set; }
        public Boolean Vertical { get; set; }
        public Double StepSize { get; set; }
        public Double Shift { get; set; }

        public int ParticleCount { get; set; }
        //public Sprite[] Particles { get; set; }

        public int ParticleSize { get; set; }

        public Sprite Particle { get; set; }

        //Convert sine wave amplitude to fit width. default Vertical = true.

        public SineSwayParticle()
            : this(0, 1, true, 0.01, 0, new Color((float) 233 / 255, (float) 255 / 255, (float) 251 / 255), "Particle_1", 25)
        {}

        public SineSwayParticle(Double startPosition, Double frequency, Boolean vertical, Double stepSize, Double shift, Color shade, String particleShape, int particleSize)
        {
            ParticlePosition = startPosition;
            Frequency = frequency;
            Shift = shift;
            StepSize = stepSize;
            Vertical = vertical;

            Particle = new Sprite //temp
                            {
                                SpriteTexture = TextureManager.Textures(particleShape),
                                ColorShading = shade
                            };

            Vector2 tempVector = GetVector();

            ParticleSize = particleSize;

            Particle.X = (int) tempVector.X;
            Particle.Y = (int) tempVector.Y;
        }


        private void Step()
        {
            ParticlePosition += StepSize;

            if (ParticlePosition >= 1)
                ParticlePosition -= 1;

        }

        private Vector2 GetVector()
        {
            var widthAlt  = (int) (Math.Sin((ParticlePosition + Shift) * Math.PI * 2 * Frequency) * Width);
            var heightAlt = (int) (ParticlePosition * Height);

            if (Vertical)
                return new Vector2(X + widthAlt, Y + heightAlt);

            return new Vector2(X + heightAlt, Y + widthAlt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Particle.Width = ParticleSize;
            Particle.Height = ParticleSize;

            Particle.Draw(spriteBatch);


            Vector2 tempVector = GetVector();

            Particle.X = (int) tempVector.X;
            Particle.Y = (int) tempVector.Y;

            Step();
        }


    }
}
