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
            _lineData = new double[4][];
            for (int x = 0; x < 4; x++)
            {
                _lineData[x] = new double[0];
            }
            this.Width = 360;
            this.Height = 235;
            InitSprites();
        }  

        private float _max;
        private float _min;
        private bool _minMaxSet;


        public float Min;
        public float Max = 1;
        public float Tick = 50; 


        public int CPUPlayerID { get; set; }

        public LegendStyle LegendStyle { get; set; }

        public readonly Color[] LineColours = {Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Gray};
        private readonly double[][] _lineData;
        private int _topLine;
        private double _drawProgress;

        public RoundLineManager LineDrawer;
        private List<RoundLine>[] _playerLines;
        private List<RoundLine> _axisLineList; 
        private SpriteMap _xborder;
        private SpriteMap _yborder;
        private Sprite _backgroundSprite;
        private SpriteMap _legendSpriteMap;
        private SpriteMap _teamLegendSpriteMap;
        private SpriteMap _cornerSpriteMap;

        public double[] this[int index]
        {
            get
            {
                return _lineData[index];   
            }
            set
            {
                
                _lineData[index] = value ?? new double[0];
                ResetDrawing();
            }
        }

        private void ResetDrawing()
        {
            _minMaxSet = false;
            _axisLineList = null;
            _playerLines = null;
            _drawProgress = 0;
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
            _teamLegendSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = 2,
                SpriteTexture = TextureManager.Textures("TeamIdentifiers")
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
         
           
            var legendPosition = this.Position.Clone();
            var legendItemSize = GameCore.Instance.Metrics["GraphLegendItem.Size", 0];
            
         
            _legendSpriteMap.ColorShading.A = _teamLegendSpriteMap.ColorShading.A =  96;

            switch (LegendStyle)
            {
                case LegendStyle.NORMAL:
                    legendPosition.Y += this.Height - legendItemSize.Y - 5;
                    legendPosition.X += this.Width - legendItemSize.X - 5;
                    for (int x = 3; x >= 0; x--)
                    {
                        if (_lineData[x].Length <= 0)
                        {
                            continue;
                        }

                        var colorID = (x == CPUPlayerID) ? 4 : x;

                        _legendSpriteMap.Draw(spriteBatch, colorID, legendItemSize, legendPosition);
                        legendPosition.X -= legendItemSize.X + 5;
                    }
                    break;
                    case LegendStyle.TEAMS:
                
                    var tlegendItemSize = GameCore.Instance.Metrics["GraphLegendItem.Size", 1];
                    legendPosition.Y += this.Height - tlegendItemSize.Y - 5;
                    legendPosition.X += this.Width - tlegendItemSize.X - 5;
                    _teamLegendSpriteMap.Draw(spriteBatch, 1, tlegendItemSize, legendPosition);
                    legendPosition.X -= tlegendItemSize.X + 5;
                    _teamLegendSpriteMap.Draw(spriteBatch, 0, tlegendItemSize, legendPosition);
                    break;

            }
      
        }

        private void CalculateMinMax()
        {
            if (_minMaxSet)
            {
                return;
            }

            _minMaxSet = true;
            _min = Max;
            _max = Min;
            var actualMin = GetDataMinimum();
            var actualMax = GetDataMaximum();

            while ((_min > actualMin) && (_min > Min))
            {
                _min -= Tick;
            }
            while ((_max < actualMax) && (_max < Max))
            {
                _max += Tick;
            }
        }

        private double GetDataMaximum()
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

        private double GetDataMinimum()
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

        public void SetMinMaxTick(float min, float max, float tick)
        {
            Min = min;
            Max = max;
            Tick = tick;
        }
        private void CalculatePlayerLines()
        {
            _playerLines = new List<RoundLine>[4];
            int maxLength = (from e in _lineData select e.Length).Max() - 1;
            float tickX = (float)this.Width / maxLength;
            float tickY = (this.Height - 11) / (_max - _min);

            for (int x = 0; x < 4; x++)
            {
                _playerLines[x] = new List<RoundLine>();
                float posX = this.X;
               
       
                
                for (int y = 1; y < _lineData[x].Length; y++)
                {
                    float posY = this.Y + this.Height - 7;
                    posY += (float) (Math.Min(_max - _min, Math.Max(_lineData[x][y-1] - _min, 0)) * -tickY);
                    var p0 = new Vector2(posX, posY);
                    posX += tickX;
                    posY = this.Y + this.Height - 7;
                    posY += (float) (Math.Min(_max - _min, Math.Max(_lineData[x][y] - _min, 0)) * -tickY);
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
                var colour = (x == CPUPlayerID && LegendStyle == LegendStyle.NORMAL) ? LineColours[4] : LineColours[x];
                LineDrawer.Draw(_playerLines[x].Take(limit),1.5f,colour,0,null);
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
                float tickY = this.Height / (_max - _min);

                for (float x = _min; x < _max; x += Tick)
                {
   
                        var posY = (x - _min) * -tickY;
                        _axisLineList.Add(new RoundLine(new Vector2(this.X, this.Y + this.Height + posY), new Vector2(this.X + this.Width, this.Y + this.Height + posY)));
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

    public enum LegendStyle
    {
        NORMAL,
        TEAMS,
        NONE
    }
}