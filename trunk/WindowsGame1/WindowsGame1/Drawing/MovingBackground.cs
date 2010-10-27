using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

 
        public override void Draw(SpriteBatch spriteBatch)
        {
            Move();

            base.DrawTiled(spriteBatch,(int) _offsetX,(int) _offsetY, Width, Height);
        }

        private void Move()
        {
            if (this.SpriteTexture != null)
            {
                _offsetX = (_offsetX + Math.Sin(Direction)*Speed) % Width;
                _offsetY = (_offsetY + Math.Cos(Direction) * Speed) % Height;
            }
        }
    }
}
