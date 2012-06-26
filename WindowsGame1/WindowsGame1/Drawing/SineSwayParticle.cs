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
        public float RotationStepSize { get; set; }
        public double Shift { get; set; }


        public int ParticleSize { get; set; }

        public SpriteMap3D ParticleSpriteMap { get; set; }
        public int ParticleType { get; set; }

        //Convert sine wave amplitude to fit width. default Vertical = true.
        public SineSwayParticle()
        {
            ParticleSpriteMap = new SpriteMap3D
            {
                Texture = TextureManager.Textures("BackgroundParticles"),
                Columns = 5,
                Rows = 1
            };
        }

        private void Step(GameTime gameTime)
        {
            ParticlePosition += StepSize * gameTime.ElapsedRealTime.TotalSeconds;

            if (ParticlePosition >= 1)
                ParticlePosition -= 1;

            this.Rotation += RotationStepSize * (float) gameTime.ElapsedRealTime.TotalSeconds;
        }

        private Vector2 GetVector()
        {
            var widthAlt  = (int) (Math.Sin((ParticlePosition + Shift) * Math.PI * 2 * Frequency) * Width);
            var heightAlt = (int) (ParticlePosition * Height);

            if (Vertical)
                return new Vector2(X + widthAlt, Y + heightAlt);

            return new Vector2(X + widthAlt, Y + heightAlt);
        }

        public void Draw(GameTime gameTime)
        {
            Step(gameTime);
            ParticleSpriteMap.Draw(ParticleType, ParticleSize, ParticleSize, (int) GetVector().X, (int) GetVector().Y);
            
        }

        public VertexPositionColorTexture[] GetVertices(GameTime gameTime)
        {
            Step(gameTime);
            return ParticleSpriteMap.GetVertices(ParticleType, (int) GetVector().X, (int)GetVector().Y, ParticleSize, ParticleSize);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(new GameTime());
        }


    }
}
