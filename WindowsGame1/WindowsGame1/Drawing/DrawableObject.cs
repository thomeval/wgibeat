using Microsoft.Xna.Framework;

namespace WGiBeat.Drawing
{
    public abstract class DrawableObject
    {

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public float Rotation { get; set; }


        public virtual Vector2 Position
        {
            get { return new Vector2(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public virtual Vector2 Size
        {
            get { return new Vector2(Width, Height); }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }


        public abstract void Draw();
    }
}
