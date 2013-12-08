using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing.Sets;

namespace WGiBeat.Drawing
{
    public abstract class LifeBar : DrawableObject 
    {
        public LifeBarSet Parent { get; set; }

        public abstract void Reset();

        public abstract void Draw( double gameTime);
    }
}
