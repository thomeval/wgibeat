using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1.Drawing
{
    /// <summary>
    /// A class to make primitive 2D objects out of lines.
    /// </summary>
    public class PrimitiveLine
    {
        readonly Texture2D pixel;
        readonly List<Vector2> vectors;

        /// <summary>
        /// Gets/sets the colour of the primitive line object.
        /// </summary>
        public Color Colour;

        /// <summary>
        /// Gets/sets the position of the primitive line object.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Gets/sets the render depth of the primitive line object (0 = front, 1 = back)
        /// </summary>
        public float Depth;

        /// <summary>
        /// Gets the number of vectors which make up the primtive line object.
        /// </summary>
        public int CountVectors
        {
            get
            {
                return vectors.Count;
            }
        }

        /// <summary>
        /// Creates a new primitive line object.
        /// </summary>
        /// <param name="graphicsDevice">The Graphics Device object to use.</param>
        public PrimitiveLine(GraphicsDevice graphicsDevice)
        {
            // create pixels
            pixel = new Texture2D(graphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            var pixels = new Color[1];
            pixels[0] = Color.White;
            pixel.SetData(pixels);

            Colour = Color.White;
            Position = new Vector2(0, 0);
            Depth = 0;

            vectors = new List<Vector2>();
        }

        /// <summary>
        /// Adds a vector to the primive live object.
        /// </summary>
        /// <param name="vector">The vector to add.</param>
        public void AddVector(Vector2 vector)
        {
            vectors.Add(vector);
        }

        /// <summary>
        /// Insers a vector into the primitive line object.
        /// </summary>
        /// <param name="index">The index to insert it at.</param>
        /// <param name="vector">The vector to insert.</param>
        public void InsertVector(int index, Vector2 vector)
        {
            vectors.Insert(index, vector);
        }

        /// <summary>
        /// Removes a vector from the primitive line object.
        /// </summary>
        /// <param name="vector">The vector to remove.</param>
        public void RemoveVector(Vector2 vector)
        {
            vectors.Remove(vector);
        }

        /// <summary>
        /// Removes a vector from the primitive line object.
        /// </summary>
        /// <param name="index">The index of the vector to remove.</param>
        public void RemoveVector(int index)
        {
            vectors.RemoveAt(index);
        }

        /// <summary>
        /// Clears all vectors from the primitive line object.
        /// </summary>
        public void ClearVectors()
        {
            vectors.Clear();
        }

        /// <summary>
        /// Renders the primtive line object.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use to render the primitive line object.</param>
        public void Render(SpriteBatch spriteBatch)
        {
            if (vectors.Count < 2)
                return;

            for (int i = 1; i < vectors.Count; i++)
            {
                var vector1 = vectors[i - 1];
                var vector2 = vectors[i];

                // calculate the distance between the two vectors
                var distance = Vector2.Distance(vector1, vector2);

                // calculate the angle between the two vectors
                var angle = (float)Math.Atan2(vector2.Y - vector1.Y,
                                                vector2.X - vector1.X);

                // stretch the pixel between the two vectors
                spriteBatch.Draw(pixel,
                                 Position + vector1,
                                 null,
                                 Colour,
                                 angle,
                                 Vector2.Zero,
                                 new Vector2(distance, 1),
                                 SpriteEffects.None,
                                 Depth);
            }
        }

        /// <summary>
        /// Creates a circle starting from 0, 0.
        /// </summary>
        /// <param name="radius">The radius (half the width) of the circle.</param>
        /// <param name="sides">The number of sides on the circle (the more the detailed).</param>
        public void CreateCircle(float radius, int sides)
        {
            vectors.Clear();

            const float MAX = 2 * (float)Math.PI;
            float step = MAX / sides;

            for (float theta = 0; theta < MAX; theta += step)
            {
                vectors.Add(new Vector2(radius * (float)Math.Cos(theta),
                                        radius * (float)Math.Sin(theta)));
            }

            // then add the first vector again so it's a complete loop
            vectors.Add(new Vector2(radius * (float)Math.Cos(0),
                                    radius * (float)Math.Sin(0)));
        }
    }
}