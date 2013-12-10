using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class Sprite3D
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
        
        private static Matrix _viewMatrix;
        private static Matrix _projectionMatrix;
       
        private bool _dimensionsSet;

        private static VertexDeclaration _vertexDeclaration;
        public static VertexDeclaration VertexDeclaration
        {
            get
            {
                if (_vertexDeclaration == null)
                {
                    _vertexDeclaration = new VertexDeclaration(Device, VertexPositionColorTexture.VertexElements);
                }
                return _vertexDeclaration;
            }
        }

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

        private VertexPositionColorTexture[] SetupPrimitives()
        {
            var topLeft = new VertexPositionColorTexture
                                    {
                                        Position = new Vector3 { X = this.X, Y = this.Y },
                                        TextureCoordinate = new Vector2 { X = 0, Y = 0 },
                                        Color = ColorShading
                                    };
            var topRight = new VertexPositionColorTexture
                               {
                                   Position = new Vector3 { X = this.X + this.Width, Y = this.Y },
                                   TextureCoordinate = new Vector2 { X = 1, Y = 0 },
                                   Color = ColorShading
                               };
            var bottomLeft = new VertexPositionColorTexture
                                 {
                                     Position = new Vector3 { X = this.X, Y = this.Y + this.Height },
                                     TextureCoordinate = new Vector2 { X = 0, Y = 1 },
                                     Color = ColorShading
                                 };
            var bottomRight = new VertexPositionColorTexture
                                  {
                                      Position = new Vector3 { X = this.X + this.Width, Y = this.Y + this.Height },
                                      TextureCoordinate = new Vector2 { X = 1, Y = 1 },
                                      Color = ColorShading
                                  };
            var result = new[]
                            {
                               topLeft,
                               topRight,
                                bottomRight,
                                    bottomRight,
                                bottomLeft,             
          topLeft
        };
            return result;
        }

        public VertexPositionColorTexture[] GetVertices()
        {
            CheckIfDimensionsSet();
            var result = SetupPrimitives();
            Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
            return result;
        }

        public void DrawVertices(VertexPositionColorTexture[] vertices)
        {
            if (vertices.Length % 3 != 0)
            {
                throw new ArgumentException("Wrong number of vertices supplied. Should be divisible by 3.");
            }
            Device.RenderState.CullMode = CullMode.None;

            CheckIfEffectInit();
            _effect.Texture = this.Texture;
            _effect.Begin();
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                Device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, vertices.Length / 3);
                pass.End();
            }
            _effect.End();
        }

        public void Draw()
        {
            _vertices = GetVertices();
            DrawVertices(_vertices);
        }

        private void CheckIfEffectInit()
        {
            Device.VertexDeclaration = VertexDeclaration;
            if (EffectInit)
            {
                return;
            }

            _effect = GetEffect();
        }

        public static BasicEffect GetEffect()
        {
            if (EffectInit)
            {
                return _effect;
            }
            var result = new BasicEffect(Device, null);

            result.View = _viewMatrix;
            result.Projection = _projectionMatrix;



            result.TextureEnabled = true;
            result.VertexColorEnabled = true;
            EffectInit = true;

            
            return result;
        }

        public void DrawTiled(float texU1, float texV1, float texU2, float texV2)
        {
            _vertices = GetVerticesTiled(texU1 / Texture.Width, texV1 / Texture.Height, texU2 / Texture.Width, texV2 / Texture.Height);

            DrawVertices(_vertices);

        }
        public VertexPositionColorTexture[] GetVerticesTiled(float texU1, float texV1, float texU2, float texV2)
        {
            CheckIfDimensionsSet();
            var result = SetupPrimitives();
            SetupTextureCoords(ref result, texU1, texV1, texU2, texV2);
            Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            return result;
        }

        private void SetupTextureCoords(ref VertexPositionColorTexture[] vertices, float texU1, float texV1, float texU2, float texV2)
        {
            vertices[0].TextureCoordinate = new Vector2(texU1, texV1);
            vertices[1].TextureCoordinate = new Vector2(texU2 + texU1, texV1);
            vertices[2].TextureCoordinate = new Vector2(texU2 + texU1, texV2 + texV1);
            vertices[3].TextureCoordinate = new Vector2(texU2 + texU1, texV2 + texV1);
            vertices[4].TextureCoordinate = new Vector2(texU1, texV2 + texV1);
            vertices[5].TextureCoordinate = new Vector2(texU1, texV1);
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

        public static Matrix GetViewProjMatrix(int width, int height)
        {
            if (!EffectInit)
            {
                _viewMatrix = Matrix.CreateLookAt(new Vector3(width /2, height / 2, 0), new Vector3(width / 2, height / 2, 1), new Vector3(0, -1, 0));
                _projectionMatrix = Matrix.CreateOrthographic(width, height, -10, 10);
            }
            return _viewMatrix*_projectionMatrix;
        }
    }

}
