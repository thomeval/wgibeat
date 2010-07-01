using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing.Sets;

namespace WGiBeat.Drawing
{
    public abstract class Lifebar : DrawableObject 
    {
        public LifebarSet Parent { get; set; }

        public abstract void Reset();

        public abstract void Draw(SpriteBatch spriteBatch, double gameTime);
    }
}
