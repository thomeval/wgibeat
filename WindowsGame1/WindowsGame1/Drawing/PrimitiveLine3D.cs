using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
      /// <summary>
        /// A class to make primitive 3D objects out of lines.
        /// </summary>
    class PrimitiveLine3D
    {
      
      
            private readonly List<VertexPositionColorTexture> vectors;

            /// <summary>
            /// Gets/sets the colour of the primitive line object.
            /// </summary>
            public Color Colour;

            /// <summary>
            /// Gets/sets the position of the primitive line object.
            /// </summary>
            public Vector2 Position;

            public bool MultiLine { get; set; }

            public int Width;

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
            public PrimitiveLine3D(GraphicsDevice graphicsDevice)
            {
                // create pixels

                Colour = Color.White;
                Position = new Vector2(0, 0);
        
                Width = 1;
                vectors = new List<VertexPositionColorTexture>();
            }

            /// <summary>
            /// Adds a vector to the primive live object.
            /// </summary>
            /// <param name="vector">The vector to add.</param>
            public void AddVector(Vector2 vector)
            {

                vectors.Add(new VertexPositionColorTexture(new Vector3(vector.X + Position.X, vector.Y + Position.Y, 0), Colour,new Vector2(0,0)));
            }

            /// <summary>
            /// Insers a vector into the primitive line object.
            /// </summary>
            /// <param name="index">The index to insert it at.</param>
            /// <param name="vector">The vector to insert.</param>
            public void InsertVector(int index, Vector2 vector)
            {
                vectors.Insert(index, new VertexPositionColorTexture(new Vector3(vector.X + Position.X, vector.Y + Position.Y, 0), Colour, new Vector2(0, 0)));
            }

            /// <summary>
            /// Removes a vector from the primitive line object.
            /// </summary>
            /// <param name="vector">The vector to remove.</param>
            public void RemoveVector(Vector2 vector)
            {
                var item =
                    (from e in vectors where e.Position.X == vector.X && e.Position.Y == vector.Y select e).
                        FirstOrDefault();
                vectors.Remove(item);
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
                {
                    return;
                }
                var effect = Sprite3D.GetEffect();
                effect.TextureEnabled = false;
            
            //    Sprite3D.Device.RenderState.PointSize = Width;

                effect.Begin();
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                   
                    if (!MultiLine)
                    {
                        Sprite3D.Device.DrawUserPrimitives(PrimitiveType.LineStrip, vectors.ToArray(), 0,
                                                           vectors.Count - 1);
                    }
                    else
                    {
                        Sprite3D.Device.DrawUserPrimitives(PrimitiveType.LineList, vectors.ToArray(), 0,
                                                         vectors.Count / 2); 
                    }
                    pass.End();
                }
                effect.End();
                effect.TextureEnabled = true;
            }

        }
    }
