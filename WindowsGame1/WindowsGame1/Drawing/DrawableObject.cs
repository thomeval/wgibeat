using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace WGiBeat.Drawing
{
    public abstract class DrawableObject
    {
        public virtual int X { get; set;}
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public float Rotation { get; set; }

        public virtual Vector2 Position
        {
            get
            {
                return new Vector2(X,Y);
            }
            set
            {
                X = (int) value.X;
                Y = (int) value.Y;
            }
        }

        public void SetPosition(float nX, float nY)
        {
            X = (int) nX;
            Y = (int) nY;
        }
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
