using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class SpectrumDrawer : DrawableObject
    {
        private int _levelsCount;
        public int LevelsCount
        {
            get { return _levelsCount; }
            set
            {
                _levelsCount = value;
                _dropSpeed = new float[value];
                _maxLevels = new float[value];
                _lineLevels = new float[value];
            }
        }


        public Color ColorShading;
        private Sprite3D _barSprite;
        private float[] _dropSpeed;
        private float[] _maxLevels;
        private float[] _lineLevels;
        private bool _spritesInit;
        private PrimitiveLine3D _line;

        public override void Draw(SpriteBatch spriteBatch)
        {
           
            Draw(spriteBatch, new float[1]);
        }

        public void Draw(SpriteBatch spriteBatch, float[] levels)
        {
            if (!_spritesInit)
            {
                _spritesInit = true;
                _barSprite = new Sprite3D { Texture = TextureManager.Textures("SpectrumBar") };

                _line = new PrimitiveLine3D(GameCore.Instance.GraphicsDevice)
                {
                    Colour = this.ColorShading,
                    Position = this.Position,
                    MultiLine = true

                };
            }
            if (levels.Length > LevelsCount)
            {
                throw new ArgumentException(
                    "Levels array size is too large. Set the LevelsCount property to match its length first.");
            }

          

                int posX = 0;


                for (int x = 0; x < levels.Count(); x++)
                {

                    _maxLevels[x] = Math.Max(_maxLevels[x], levels[x]);
                    levels[x] /= _maxLevels[x];

                    if (levels[x] >= _lineLevels[x])
                    {
                        _dropSpeed[x] = 0.0f;
                        _lineLevels[x] = levels[x];
                    }
                    else
                    {
                        _dropSpeed[x] += 1.5f * (float)TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds;
                        _lineLevels[x] -= _dropSpeed[x] * (float)TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds;
                    }

                    _barSprite.ColorShading = this.ColorShading;
                    _barSprite.Position = new Vector2(this.X + posX, this.Y);
                    _barSprite.Size = new Vector2(this.Width, this.Height * levels[x]);
                    _barSprite.DrawTiled(0,0,_barSprite.Texture.Width,_barSprite.Height * 2);
                    posX += this.Width;
                }


                DrawLineLevels(spriteBatch);
        }

        private void DrawLineLevels(SpriteBatch spriteBatch)
        {
            int posX = 0;
            _line.Colour = ColorShading;
            _line.ClearVectors();
            for (int x = 0; x < _lineLevels.Count(); x++)
            {
                

                _line.AddVector(new Vector2(posX + 1, this.Height*_lineLevels[x]));
                _line.AddVector(new Vector2(posX + this.Width - 1, this.Height*_lineLevels[x]));
                _line.Render(spriteBatch);
                posX += this.Width;
            }
        }

        public void ResetMaxLevels()
        {
            for (int x = 0; x < _maxLevels.Length; x++)
            {
                _maxLevels[x] = 0;
            }
        }
    }
    
}
