﻿using System;
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
        public int X;
        public int Y;
        public int Height;
        public int Width;

        public virtual void SetPosition(Vector2 position)
        {
            X = (int) position.X;
            Y = (int) position.Y;
        }

        public virtual void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }
        public abstract void Draw(SpriteBatch sb);
    }
}