using System;

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

            public void DrawVertices(VertexPositionColorTexture[] vertices)
            {
                if (vertices.Length % 3 != 0)
                {
                    throw new ArgumentException("Wrong number of vertices supplied. Should be divisible by 3.");
                }
                Device.RenderState.CullMode = CullMode.None;
                Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
                Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
                var effect = Sprite3D.GetEffect();
                Device.VertexDeclaration = Sprite3D.VertexDeclaration;
                effect.Texture = this.Texture;
                effect.Begin();
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    Device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
                    pass.End();
                }
                effect.End();
            }

            public VertexPositionColorTexture[] GetVertices(int cellnumber,  float x, float y, float width, float height)
            {
                return GetVertices(cellnumber, new Vector2(x, y), new Vector2(width,height));
            }

            public VertexPositionColorTexture[] GetVertices(int cellnumber, Vector2 position, Vector2 size)
            {
                Vector2 sourcePosition, sourceSize;
                CalculateSourceRectangle(cellnumber, out sourcePosition, out sourceSize);
                var result = SetupPrimitives(sourcePosition, sourceSize, position, size);
                return result;
            }

            public void Draw(int cellnumber, float width, float height, float x, float y)
            {
                _vertices = GetVertices(cellnumber, x, y, width, height);
                DrawVertices(_vertices);

            }

 
            public void Draw(int cellnumber, float width, float height, Vector2 position)
            {
                Draw(cellnumber, width, height, position.X, position.Y);
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
                Draw(cellnumber, 1.0f * Texture.Width / Columns, 1.0f * Texture.Height / Rows, x, y);
            }

            public void Draw(int cellnumber, Vector2 position)
            {
                Draw(cellnumber, 1.0f * Texture.Width / Columns, 1.0f * Texture.Height / Rows, position);
            }

            public void Draw(int cellnumber, Vector2 size, Vector2 position)
            {
                Draw(cellnumber, size.X, size.Y, position.X, position.Y);
            }

            private VertexPositionColorTexture[] SetupPrimitives(Vector2 sourcePosition, Vector2 sourceSize, Vector2 destPosition, Vector2 destSize)
            {
                var result = new[]
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
                return result;
            }

        
        }
    }


