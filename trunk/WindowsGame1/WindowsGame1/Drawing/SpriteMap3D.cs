using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WGiBeat.Drawing
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

  
        public class SpriteMap3D
        {
            private VertexPositionColorTexture[] _vertices;
            
            public Texture2D Texture { get; set; }
            public Color ColorShading = Color.White;
            public int Columns { get; set; }
            public int Rows { get; set; }
            public static GraphicsDevice Device { get; set; }
            public SpriteMap3D()
            {
                Columns = 1;
                Rows = 1;
            }

            public void Draw(int cellnumber, float width, float height, float x, float y)
            {
                Vector2 sourcePosition, sourceSize;
                CalculateSourceRectangle(cellnumber, out sourcePosition, out sourceSize);
                SetupPrimitives(sourcePosition,sourceSize, new Vector2(x,y), new Vector2(width,height) );
               
                Device.RenderState.CullMode = CullMode.None;
                Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
                Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
                var effect = Sprite3D.GetEffect();
                effect.Texture = this.Texture;
                effect.Begin();
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    Device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, 2);
                    pass.End();
                }
                effect.End();
            }

 
            public void Draw(int cellnumber, float width, float height, Vector2 position)
            {
                Draw(cellnumber, width, height, (int)position.X, (int)position.Y);
            }
            private void CalculateSourceRectangle(int cellnumber, out Vector2 sourcePosition, out Vector2 sourceSize)
            {

                float xOffset = 0, yOffset = 0;
                float xSize = 0, ySize = 0;

                xSize = 1.0f / Columns;
                ySize = 1.0f / Rows;


                while (cellnumber >= Columns)
                {
                    yOffset++;
                    cellnumber -= Columns;
                }
                xOffset = cellnumber;
                yOffset *= ySize;
                xOffset *= xSize;

                sourcePosition = new Vector2 {X = xOffset, Y = yOffset };
                sourceSize = new Vector2 {X = xSize, Y = ySize};
            }

            public void Draw(int cellnumber, float x, float y)
            {
                Draw(cellnumber, Texture.Width / Columns, Texture.Height / Rows, x, y);
            }

            public void Draw(int cellnumber, Vector2 position)
            {
                Draw(cellnumber, Texture.Width / Columns, Texture.Height / Rows, position);
            }

            public void Draw(int cellnumber, Vector2 size, Vector2 position)
            {
                Draw(cellnumber, size.X, size.Y, position.X, position.Y);
            }

            private void SetupPrimitives(Vector2 sourcePosition, Vector2 sourceSize, Vector2 destPosition, Vector2 destSize)
            {
                _vertices = new[]
                            {
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = destPosition.X, Y = destPosition.Y},
                                        TextureCoordinate = new Vector2 {X = sourcePosition.X, Y = sourcePosition.Y},
                                        Color = ColorShading
                                    },
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = destPosition.X + destSize.X, Y = destPosition.Y},
                                        TextureCoordinate = new Vector2 {X = sourcePosition.X + sourceSize.X, Y = sourcePosition.Y},
                                        Color = ColorShading
                                    },
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = destPosition.X + destSize.X, Y = destPosition.Y + destSize.Y},
                                        TextureCoordinate = new Vector2 {X = sourcePosition.X + sourceSize.X, Y = sourcePosition.Y + sourceSize.Y},
                                        Color = ColorShading
                                    },
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = destPosition.X + destSize.X, Y = destPosition.Y + destSize.Y},
                                        TextureCoordinate = new Vector2 {X = sourcePosition.X + sourceSize.X, Y = sourcePosition.Y + sourceSize.Y},
                                        Color = ColorShading
                                    },
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = destPosition.X, Y = destPosition.Y + destSize.Y},
                                        TextureCoordinate = new Vector2 {X = sourcePosition.X, Y = sourcePosition.Y + sourceSize.Y},
                                        Color = ColorShading
                                    },             
            new VertexPositionColorTexture
                {
                    Position = new Vector3 {X = destPosition.X, Y = destPosition.Y},
                    TextureCoordinate = new Vector2 {X = sourcePosition.X, Y = sourcePosition.Y},
                    Color = ColorShading
                }
        };
            }
        }
    }


