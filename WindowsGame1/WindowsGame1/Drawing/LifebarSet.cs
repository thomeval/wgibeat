using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class LifebarSet
    {

        public LifebarSet()
        {
            _life = new double[4];
            _lifebars = new Lifebar[4];
            Playing = new bool[4];
        }
        private readonly Lifebar[] _lifebars;
        private readonly double[] _life;

        public MetricsManager Metrics { get; set; }

        public bool[] Playing
        {
            get; set;
        }

        private GameType _gameType;
        public GameType GameType
        {
            get { return _gameType; }
            set
            {
                _gameType = value;
                CreateLifebars();
            }
        }

        private void CreateLifebars()
        {

            switch (GameType)
            {
                case GameType.NORMAL:
                    for (int x = 0; x < 4; x++)
                    {
                        _lifebars[x] = new NormalLifebar {Height = 30, Width = 260, SideLocation = x};
                        _lifebars[x].SetPosition(Metrics["NormalLifebar", x]);
                    }
                    break;
                    case GameType.COOPERATIVE:
                    break;
            }
        }

        public double AdjustLife(double amount, int player)
        {
            //Adjust the life according to the 'rules' of the lifebar used, and return the new amount.
            switch (GameType)
            {
                case GameType.NORMAL:
                    AdjustLifeNormal(amount, player);
                    _lifebars[player].SetLife(_life[player]);
                    break;
                case GameType.COOPERATIVE:
                    
                    break;
            }
            return _life[player];
        }

        private void AdjustLifeNormal(double amount, int player)
        {
            if (_life[player] + amount > 100)
            {
                if (_life[player] >= 100)
                {
                    _life[player] += amount / 3;
                }
                else
                {
                    double over = _life[player] + amount - 100;
                    _life[player] = 100 + (over / 3);
                }

            }
            else
            {
                _life[player] += amount;
            }
            
        }

        public void SetLife(double amount, int player)
        {
            _life[player] = amount;
            switch (GameType)
            {
                case GameType.NORMAL:
                    _lifebars[player].SetLife(_life[player]);
                    break;
                case GameType.COOPERATIVE:

                    break; 
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (GameType)
            {
                case GameType.NORMAL:
                    for (int x = 0; x < 4; x++)
                    {
                        if (Playing[x])
                        _lifebars[x].Draw(spriteBatch);
                    }
                    break;
                case GameType.COOPERATIVE:
                    {
                        _lifebars[0].SetPosition(Metrics["CoopLifebar", 0]);
                        _lifebars[0].Draw(spriteBatch);
                    }
                    if (Playing[2] || Playing[3])
                    {
                        _lifebars[0].SetPosition(Metrics["CoopLifebar", 1]);
                        _lifebars[0].Draw(spriteBatch);
                    }
                    break;
            }
        }
    }
}
