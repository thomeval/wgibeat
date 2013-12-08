using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class MovingBackground : Sprite3D
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

 
        public void Draw( GameTime gameTime)
        {
            Move(gameTime);
            DrawTiled( (float) _offsetX, (float) _offsetY,  (float) (_offsetX + Width), (float) (_offsetY + Height));
        }



        private void Move(GameTime gameTime)
        {
            if (this.Texture == null) return;
            var adj = Speed*gameTime.ElapsedRealTime.TotalSeconds;

            _offsetX = (_offsetX + Math.Sin(Direction) * adj) % Texture.Width;
            _offsetY = (_offsetY + Math.Cos(Direction) * adj) % Texture.Height;
        }
    }
}
