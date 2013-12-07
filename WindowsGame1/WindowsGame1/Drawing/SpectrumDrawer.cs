using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoundLineCode;

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
        private RoundLineManager _line;
        private List<RoundLine> _lineList;

        public void Init()
        {
            _line = RoundLineManager.Instance;

            _lineList = new List<RoundLine>();
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
           
            Draw(new float[1]);
        }

        public void Draw(float[] levels)
        {
            if (!_spritesInit)
            {
                _spritesInit = true;
                _barSprite = new Sprite3D { Texture = TextureManager.Textures("SpectrumBar") };
                Init();
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


                DrawLineLevels();
        }

        private void DrawLineLevels()
        {
            int posX = 0;
            _lineList.Clear();
            for (int x = 0; x < _lineLevels.Count(); x++)
            {
                

                var p0 = new Vector2(posX + 1, this.Height*_lineLevels[x]);
                var p1 = new Vector2(posX + this.Width - 1, this.Height*_lineLevels[x]);
                p0 += this.Position;
                p1 += this.Position;
                _lineList.Add(new RoundLine(p0,p1));
                posX += this.Width;
            }

            _line.Draw(_lineList,1,ColorShading,0,null);
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
