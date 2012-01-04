using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class MovingBackground : Sprite
    {
        public double Direction { get; set; }

        private double _speed = 1;
        public double Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        private double _offsetX;
        private double _offsetY;

 
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Move(gameTime);
            DrawTiled(spriteBatch,(int) _offsetX,(int) _offsetY, Width, Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch,new GameTime());
        }

        private void Move(GameTime gameTime)
        {
            if (this.SpriteTexture == null) return;
            var adj = Speed*gameTime.ElapsedRealTime.TotalSeconds;

            _offsetX = (_offsetX + Math.Sin(Direction) * adj) % SpriteTexture.Width;
            _offsetY = (_offsetY + Math.Cos(Direction) * adj) % SpriteTexture.Height;
        }
    }
}
