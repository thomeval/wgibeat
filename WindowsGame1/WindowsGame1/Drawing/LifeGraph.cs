using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;

namespace WGiBeat.Screens
{
    public class LifeGraph : DrawableObject
    {
        public LifeGraph()
        {
            _lineData = new float[4][];
            for (int x = 0; x < 4; x++)
            {
                _lineData[x] = new float[0];
            }
            this.Width = 360;
            this.Height = 235;
            InitSprites();
        }

        

        private int _max;
        private int _min;
        private bool _minMaxSet;

        private const int ABSOLUTE_MIN = -100;
        private const int ABSOLUTE_MAX = 200;

        public int Location { get; set; }
        
        public readonly Color[] LineColours = {Color.Red, Color.Blue, Color.Green, Color.Yellow};
        private readonly float[][] _lineData;
        private int _topLine;
        private double _drawProgress;

        public PrimitiveLine LineDrawer;
        private SpriteMap _xborder;
        private SpriteMap _yborder;
        private Sprite _backgroundSprite;
        private SpriteMap _cornerSpriteMap;

        public float[] this[int index]
        {
            get
            {
                return _lineData[index];   
            }
            set
            {
                _lineData[index] = value;
            }
        }

        public void InitSprites()
        {
            _yborder = new SpriteMap
                           {
                               Columns = 2,
                               Rows = 1,
                               SpriteTexture = TextureManager.Textures("LifegraphYBorder")
                           };
            _xborder = new SpriteMap
            {
                Columns = 1,
                Rows = 2,
                SpriteTexture = TextureManager.Textures("LifegraphXBorder")

            };
            _backgroundSprite = new Sprite()
                                    {
                                        SpriteTexture = TextureManager.Textures("LifeGraphMiddle"),
                                        
                                    };
            _cornerSpriteMap = new SpriteMap
                                   {
                                       Columns = 2, Rows = 2, SpriteTexture = TextureManager.Textures("LifeGraphCorners")
                                   };
        }
        public void CycleTopLine()
        {
            var temp = _topLine;

            do
            {
                _topLine= (_topLine + 1) % 4;

            } while ((_lineData[_topLine].Length == 0) && (temp != _topLine));
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            _drawProgress += 0.5;
            _backgroundSprite.Position = this.Position;
            _backgroundSprite.Draw(spriteBatch);
            CalculateMinMax();
            DrawAxis(spriteBatch);
            DrawPlayerLines(spriteBatch);
            DrawLabels(spriteBatch);
            DrawBorder(spriteBatch);
        }

        private void CalculateMinMax()
        {
            if (!_minMaxSet)
            {
                _minMaxSet = true;
                _min = 0;
                _max = 100;
                var actualMin = GetDataMinimum();
                var actualMax = GetDataMaximum();

                while ((_min > actualMin) && (_min > ABSOLUTE_MIN))
                {
                    _min -= 50;
                }
                while ((_max < actualMax) && (_max < ABSOLUTE_MAX))
                {
                    _max += 50;
                }
            }
        }

        private float GetDataMaximum()
        {
            float result = 0;

            for (int x = 0; x < 4; x++)
            {
                if (_lineData[x].Length > 0)
                {
                    result = Math.Max(result, _lineData[x].Max());
                }
            }
            return result;
        }

        private float GetDataMinimum()
        {
            float result = 999;

            for (int x = 0; x < 4; x++)
            {
                if (_lineData[x].Length > 0)
                {
                    result = Math.Min(result, _lineData[x].Min());
                }
            }
            return result;
        }

        private void DrawPlayerLines(SpriteBatch spriteBatch)
        {
            LineDrawer.Width = 2;
            int maxLength = (from e in _lineData select e.Length).Max() - 1;

            float tickX = (float)this.Width / maxLength;
            float tickY = (float)(this.Height - 11) / (_max - _min);
            bool loopedOnce = false;
            for (int x = _topLine; (x != _topLine) || (!loopedOnce); x = (x+1) % 4)
            {
                float posX = this.X;
                loopedOnce = true;
                LineDrawer.ClearVectors();
                LineDrawer.Colour = LineColours[x];
                var limit = Math.Min(_lineData[x].Length, _drawProgress);
                for (int y = 0; y < limit; y++)
                {
                    float posY = this.Y + this.Height - 7;
                    posY += Math.Max(_lineData[x][y] - _min, 0) * -tickY;
                    var pos = new Vector2(posX, posY);
                    posX += tickX;
                    LineDrawer.AddVector(pos);
                }
                LineDrawer.Render(spriteBatch);
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch)
        {
            LineDrawer.Colour = Color.Black;
            LineDrawer.Width = 1;
            LineDrawer.ClearVectors();
            LineDrawer.AddVector(new Vector2(this.X, this.Y));
            LineDrawer.AddVector(new Vector2(this.X + this.Width, this.Y));
            LineDrawer.AddVector(new Vector2(this.X + this.Width, this.Y + this.Height));
            LineDrawer.AddVector(new Vector2(this.X, this.Y + this.Height));
            LineDrawer.AddVector(new Vector2(this.X, this.Y));
            LineDrawer.Render(spriteBatch);

            _xborder.Draw(spriteBatch,1,20,this.Height,this.X - 20, this.Y);
            _xborder.Draw(spriteBatch,0,20, this.Height,this.X + this.Width, this.Y);
            _yborder.Draw(spriteBatch,0,this.Width,20,this.X, this.Y - 20);
            _yborder.Draw(spriteBatch,1, this.Width, 20, this.X, this.Y + this.Height);
            _cornerSpriteMap.Draw(spriteBatch,0,this.X - 20, this.Y - 20);
            _cornerSpriteMap.Draw(spriteBatch, 1, this.X + this.Width, this.Y - 20);
            _cornerSpriteMap.Draw(spriteBatch, 2, this.X - 20, this.Y +this.Height);
            _cornerSpriteMap.Draw(spriteBatch, 3, this.X + this.Width, this.Y  + this.Height);
        }

        private void DrawAxis(SpriteBatch spriteBatch)
        {
            LineDrawer.Colour = Color.White;
            LineDrawer.ClearVectors();
            LineDrawer.Width = 1;
            float tickY = (float)this.Height / (_max - _min);

            for (int x = _min; x < _max; x+=50)
            {
                if (x % 100 == 0)
                {
                    LineDrawer.ClearVectors();
                    var posY = (x - _min)*-tickY;
                    LineDrawer.AddVector(new Vector2(this.X, this.Y + this.Height + posY));
                    LineDrawer.AddVector(new Vector2(this.X + this.Width, this.Y + this.Height + posY));
                    LineDrawer.Render(spriteBatch);
                }
            }
          
        }

        private Vector2 _minLabelPosition;
        private Vector2 _maxLabelPosition;

        private void DrawLabels(SpriteBatch spriteBatch)
        {
            _maxLabelPosition.X = this.X + 7;
            _maxLabelPosition.Y = this.Y + 2;
            _minLabelPosition.X = this.X + 7;
            _minLabelPosition.Y = this.Y + this.Height - 26;
            TextureManager.DrawString(spriteBatch, "" + _min, "DefaultFont",_minLabelPosition,Color.White, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + _max, "DefaultFont", _maxLabelPosition, Color.White, FontAlign.LEFT);
        }
    }
}