using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class Sprite3D : DrawableObject
    {
        public Sprite3D()
        {
            ColorShading = Color.White;
        }

        public Color ColorShading;
        public Texture2D Texture;

        public static GraphicsDevice Device;
        private static BasicEffect _effect;
        public static bool EffectInit;
        private VertexPositionColorTexture[] _vertices;
        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;
        private static VertexDeclaration _vertexDeclaration;
        private bool _dimensionsSet;

        private void SetupPrimitives()
        {
            _vertices = new[]
                            {
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = this.X, Y = this.Y},
                                        TextureCoordinate = new Vector2 {X = 0, Y = 0},
                                        Color = ColorShading
                                    },
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = this.X + this.Width, Y = this.Y},
                                        TextureCoordinate = new Vector2 {X = 1, Y = 0},
                                        Color = ColorShading
                                    },
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = this.X + this.Width, Y = this.Y + this.Height},
                                        TextureCoordinate = new Vector2 {X = 1, Y = 1},
                                        Color = ColorShading
                                    },
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = this.X + this.Width, Y = this.Y + this.Height},
                                        TextureCoordinate = new Vector2 {X = 1, Y = 1},
                                        Color = ColorShading
                                    },
                                new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 {X = this.X, Y = this.Y + this.Height},
                                        TextureCoordinate = new Vector2 {X = 0, Y = 1},
                                        Color = ColorShading
                                    },             
            new VertexPositionColorTexture
                {
                    Position = new Vector3 {X = this.X, Y = this.Y},
                    TextureCoordinate = new Vector2 {X = 0, Y = 0},
                    Color = ColorShading
                }
        };         
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            CheckIfEffectInit();
            CheckIfDimensionsSet();
            SetupPrimitives();
            SetupTextureCoords(0, 0, 1, 1);
            Device.RenderState.CullMode = CullMode.None;
            Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
            _effect.Texture = this.Texture;
            _effect.Begin();
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, 2);
                pass.End();
            }
            _effect.End();

        }

        private void CheckIfEffectInit()
        {
           if (EffectInit)
           {
               return;
           }

            _effect = GetEffect();
        }

        public BasicEffect GetEffect()
        {
            var result = new BasicEffect(Device, null);

           
            _viewMatrix = Matrix.CreateLookAt(new Vector3(400, 300, 0), new Vector3(400, 300, 1), new Vector3(0, -1, 0));
            _projectionMatrix = Matrix.CreateOrthographic(800, 600, -10, 10);

            result.View = _viewMatrix;
            result.Projection = _projectionMatrix;

            result.TextureEnabled = true;
            result.VertexColorEnabled = true;
            EffectInit = true;

            _vertexDeclaration = new VertexDeclaration(
        Device, VertexPositionColorTexture.VertexElements);
            Device.VertexDeclaration = _vertexDeclaration;
            return result;
        }

        public void DrawTiled(int texU1, int texV1, int texU2, int texV2)
        {
            CheckIfEffectInit();
            CheckIfDimensionsSet();

            SetupPrimitives();

            SetupTextureCoords((float) texU1 / Texture.Width, (float)texV1 / Texture.Height, (float)texU2 / Texture.Width, (float)texV2 / Texture.Height);
  
            _effect.Texture = this.Texture;
            Device.RenderState.CullMode = CullMode.None;
            Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            _effect.Begin();
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, 2);
                pass.End();
            }
            _effect.End();           
        }

        private void SetupTextureCoords(float texU1, float texV1, float texU2, float texV2)
        {
            _vertices[0].TextureCoordinate = new Vector2(texU1,texV1);
            _vertices[1].TextureCoordinate = new Vector2(texU2, texV1);
            _vertices[2].TextureCoordinate = new Vector2(texU2, texV2);
            _vertices[3].TextureCoordinate = new Vector2(texU2, texV2);
            _vertices[4].TextureCoordinate = new Vector2(texU1, texV2);
            _vertices[5].TextureCoordinate = new Vector2(texU1, texV1);

        }

        /// <summary>
        /// Checks if the Width and Height of the Sprite have been set. If not, the width and height
        /// are determined from the texture itself.
        /// </summary>
        private void CheckIfDimensionsSet()
        {
           
            if (_dimensionsSet)
           {
               return;
           }
            if (Width == 0)
            {
                Width = Texture.Width;
            }
            if (Height == 0)
            {
                Height = Texture.Height;
            }
            _dimensionsSet = true;
        }
    }
    
}
