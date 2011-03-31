using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class SineSwayParticle : DrawableObject
    {
        public double ParticlePosition { get; set; } //Between 0 and 1.
        public double Frequency { get; set; }
        public bool Vertical { get; set; }
        public double StepSize { get; set; }
        public double Shift { get; set; }


        public int ParticleSize { get; set; }

        public SpriteMap ParticleSpriteMap { get; set; }
        public int ParticleType { get; set; }

        //Convert sine wave amplitude to fit width. default Vertical = true.
        public SineSwayParticle()
        {
            ParticleSpriteMap = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures("BackgroundParticles"),
                Columns = 5,
                Rows = 1
            };
        }

        private void Step()
        {
            ParticlePosition += StepSize;

            if (ParticlePosition >= 1)
                ParticlePosition -= 1;

            this.Rotation += 0.0075f;
        }

        private Vector2 GetVector()
        {
            var widthAlt  = (int) (Math.Sin((ParticlePosition + Shift) * Math.PI * 2 * Frequency) * Width);
            var heightAlt = (int) (ParticlePosition * Height);

            if (Vertical)
                return new Vector2(X + widthAlt, Y + heightAlt);

            return new Vector2(X + widthAlt, Y + heightAlt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
           // this.Width = ParticleSize;
          //  this.Height = ParticleSize;
            ParticleSpriteMap.Draw(spriteBatch, ParticleType, ParticleSize, ParticleSize, (int) GetVector().X, (int) GetVector().Y, Rotation);
            Step();
        }


    }
}
