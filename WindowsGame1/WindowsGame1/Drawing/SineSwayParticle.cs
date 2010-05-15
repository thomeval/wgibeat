using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class SineSwayParticle : DrawableObject
    {
        public Double Position { get; set; } //Between 0 and 1.
        public Double Frequency { get; set; }
        public Boolean Vertical { get; set; }
        public Double StepSize { get; set; }
        public Double Shift { get; set; }

        public int ParticleCount { get; set; }
        public Sprite[] Particles { get; set; }

        private Sprite _particle; //temp

        //Convert sine wave amplitude to fit width. default Vertical = true.

        public SineSwayParticle() : this(0, 1, true, 0.01, 0, Color.Black, "Particle")
        {}

        public SineSwayParticle(Double startPosition, Double frequency, Boolean vertical, Double stepSize, Double shift, Color shade, String particleShape)
        {
            Position = startPosition;
            Frequency = frequency;
            Shift = shift;
            StepSize = stepSize;
            Vertical = vertical;

            _particle = new Sprite //temp
                            {
                                SpriteTexture = TextureManager.Textures[particleShape],
                                ColorShading = shade
                            };

            Vector2 tempVector = GetVector();

            _particle.X = (int) tempVector.X;
            _particle.Y = (int) tempVector.Y;
        }


        private void Step()
        {
            Position += StepSize;

            if (Position >= 1)
                Position -= 1;

        }

        private Vector2 GetVector()
        {
            int widthAlt  = (int) (Math.Sin((Position + Shift) * Math.PI * 2 * Frequency) * Width);
            int heightAlt = (int) (Position * Height);

            if (Vertical)
                return new Vector2(X + widthAlt, Y + heightAlt);

            return new Vector2(X + heightAlt, Y + widthAlt);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            _particle.Draw(spriteBatch);

            Vector2 tempVector = GetVector();

            _particle.X = (int)tempVector.X;
            _particle.Y = (int)tempVector.Y;

            /*
            foreach (Sprite token in Particles)
            {
                token.Draw(spriteBatch);

            }
            */
            //throw new NotImplementedException();

            Step();
        }


    }
}
