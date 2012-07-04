using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoundLineCode;
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
        private const int ABSOLUTE_MAX = 400;


        public int CPUPlayerID { get; set; }
        public readonly Color[] LineColours = {Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Gray};
        private readonly float[][] _lineData;
        private int _topLine;
        private double _drawProgress;

        public RoundLineManager LineDrawer;
        private List<RoundLine>[] _playerLines;
        private List<RoundLine> _axisLineList; 
        private SpriteMap _xborder;
        private SpriteMap _yborder;
        private Sprite _backgroundSprite;
        private SpriteMap _legendSpriteMap;
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
                               Columns = 1,
                               Rows = 2,
                               SpriteTexture = TextureManager.Textures("LifegraphYBorder")
                           };
            _xborder = new SpriteMap
            {
                Columns = 2,
                Rows = 1,
                SpriteTexture = TextureManager.Textures("LifegraphXBorder")

            };
            _backgroundSprite = new Sprite
                                    {
                                        SpriteTexture = TextureManager.Textures("LifeGraphMiddle"),
                                        
                                    };
            _cornerSpriteMap = new SpriteMap
                                   {
                                       Columns = 2, Rows = 2, SpriteTexture = TextureManager.Textures("LifeGraphCorners")
                                   };
            _legendSpriteMap = new SpriteMap
                                   {
                                       Columns = 1,
                                       Rows = 5,
                                       SpriteTexture = TextureManager.Textures("PlayerIdentifiers")
                                   };
            LineDrawer = RoundLineManager.Instance;
    
        }
        public void CycleTopLine()
        {
            var temp = _topLine;

            do
            {
                _topLine= (_topLine + 1) % 4;

            } while ((_lineData[_topLine].Length == 0) && (temp != _topLine));
        }

        private const int LINE_DRAW_SPEED = 30;
        public override void Draw(SpriteBatch spriteBatch)
        {
            
            _drawProgress += TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds*LINE_DRAW_SPEED;
            _backgroundSprite.Position = this.Position;
            _backgroundSprite.Draw(spriteBatch);
            CalculateMinMax();
            DrawAxis();
            DrawLegend(spriteBatch);
            DrawPlayerLines();
            DrawLabels(spriteBatch);
            DrawBorder(spriteBatch);
        }

        private void DrawLegend(SpriteBatch spriteBatch)
        {
            const int LEGEND_ITEM_HEIGHT = 30;
            const int LEGEND_ITEM_WIDTH = 55;
            var legendPosition = this.Position.Clone();
            legendPosition.X += this.Width - LEGEND_ITEM_WIDTH - 5;
            legendPosition.Y += this.Height - LEGEND_ITEM_HEIGHT - 5;
            _legendSpriteMap.ColorShading.A = 128;
            for (int x = 3; x >= 0; x--)
            {
                if (_lineData[x].Length <= 0)
                {
                    continue;
                }

                var colorID = (x == CPUPlayerID) ? 4 : x;

                _legendSpriteMap.Draw(spriteBatch, colorID, LEGEND_ITEM_WIDTH, LEGEND_ITEM_HEIGHT, legendPosition);
                legendPosition.X -= LEGEND_ITEM_WIDTH + 5;
            }
        }

        private void CalculateMinMax()
        {
            if (_minMaxSet)
            {
                return;
            }

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

        private float GetDataMaximum()
        {
            try
            {
                return (from e in _lineData where e.Length > 0 select e.Max()).Max();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private float GetDataMinimum()
        {

            try
            {
              return  (from e in _lineData where e.Length > 0 select e.Min()).Min();
            }
            catch (Exception)
            {
                return 9999;
            }

        }

        private void CalculatePlayerLines()
        {
            _playerLines = new List<RoundLine>[4];
            int maxLength = (from e in _lineData select e.Length).Max() - 1;
            float tickX = (float)this.Width / maxLength;
            float tickY = (float)(this.Height - 11) / (_max - _min);

            for (int x = 0; x < 4; x++)
            {
                _playerLines[x] = new List<RoundLine>();
                float posX = this.X;
               
       
                
                for (int y = 1; y < _lineData[x].Length; y++)
                {
                    float posY = this.Y + this.Height - 7;
                    posY += Math.Min(_max - _min, Math.Max(_lineData[x][y-1] - _min, 0)) * -tickY;
                    var p0 = new Vector2(posX, posY);
                    posX += tickX;
                    posY = this.Y + this.Height - 7;
                    posY += Math.Min(_max - _min, Math.Max(_lineData[x][y] - _min, 0)) * -tickY;
                    var p1 = new Vector2(posX, posY);
                    _playerLines[x].Add(new RoundLine(p0,p1));
                }
            }
        }
        
        private void DrawPlayerLines()
        {
           if (_playerLines == null)
           {
               CalculatePlayerLines();
               return;
           }
            //TODO: Refactor
            LineDrawer.BlurThreshold = LineDrawer.ComputeBlurThreshold(1.0f, LineDrawer.ViewProjMatrix,800);
            bool loopedOnce = false;

            for (int x = _topLine; (x != _topLine) || (!loopedOnce); x = (x+1) % 4)
            {
                loopedOnce = true;
                var limit = (int) Math.Min(_lineData[x].Length, _drawProgress);
                var colour = (x == CPUPlayerID) ? LineColours[4] : LineColours[x];
                LineDrawer.Draw(_playerLines[x].Take(limit),1,colour,0,null);
            }

        }

        private void DrawBorder(SpriteBatch spriteBatch)
        {


            _xborder.Draw(spriteBatch,1,20,this.Height,this.X - 20, this.Y);
            _xborder.Draw(spriteBatch,0,20, this.Height,this.X + this.Width, this.Y);
            _yborder.Draw(spriteBatch,1,this.Width,20,this.X, this.Y - 20);
            _yborder.Draw(spriteBatch,0, this.Width, 20, this.X, this.Y + this.Height);
            _cornerSpriteMap.Draw(spriteBatch,0,this.X - 20, this.Y - 20);
            _cornerSpriteMap.Draw(spriteBatch, 1, this.X + this.Width, this.Y - 20);
            _cornerSpriteMap.Draw(spriteBatch, 2, this.X - 20, this.Y +this.Height);
            _cornerSpriteMap.Draw(spriteBatch, 3, this.X + this.Width, this.Y  + this.Height);
        }

        private void DrawAxis()
        {
            if (_axisLineList == null)
            {
                _axisLineList = new List<RoundLine>();
                float tickY = (float)this.Height / (_max - _min);

                for (int x = _min; x < _max; x += 50)
                {
                    if (x % 100 == 0)
                    {

                        var posY = (x - _min) * -tickY;
                        _axisLineList.Add(new RoundLine(new Vector2(this.X, this.Y + this.Height + posY), new Vector2(this.X + this.Width, this.Y + this.Height + posY)));

                    }
                }
            }
    
            LineDrawer.Draw(_axisLineList, 0.8f, Color.White, 0, null);

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