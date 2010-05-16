using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class LifebarSet
    {

        private readonly MetricsManager _metrics;
        private readonly Player[] _players; 
        private readonly GameType _gameType;
        private readonly Lifebar[] _lifebars;

        public LifebarSet()
        {
            _lifebars = new Lifebar[4];
        }
        public LifebarSet(MetricsManager metrics, Player[] players, GameType gameType)
        :this()
        {
            _metrics = metrics;
            _players = players;
            _gameType = gameType;
            CreateLifebars();
        }


        private void CreateLifebars()
        {

            switch (_gameType)
            {
                case GameType.NORMAL:
                    for (int x = 0; x < 4; x++)
                    {
                        _lifebars[x] = new NormalLifebar {Height = 30, Width = 260, SideLocation = x};
                        _lifebars[x].SetPosition(_metrics["NormalLifebar", x]);
                        if (_players != null)
                        {
                            _lifebars[x].SetLife(_players[x].Life);
                        }
                    }
                    break;
                    case GameType.COOPERATIVE:
                    _lifebars[0] = new CoopLifebar {Height = 30, Width = 785};
     
                    for (int x = 0; x < 4; x++ )
                    {
                        ((CoopLifebar) _lifebars[0]).Playing[x] = _players[x].Playing;
                        ((CoopLifebar) _lifebars[0]).SetLife(_players[x].Life,x);
                    }
                        break;
            }
        }

        public double AdjustLife(double amount, int player)
        {
            //Adjust the life according to the 'rules' of the lifebar used, and return the new amount.
            switch (_gameType)
            {
                case GameType.NORMAL:
                    AdjustLifeNormal(amount, player);
                    _lifebars[player].SetLife(_players[player].Life);
                    break;
                case GameType.COOPERATIVE:
                    ((CoopLifebar)_lifebars[0]).AdjustLife(amount, player);           
                    break;
            }
            return _players[player].Life;
        }

        const int LIFE_MAX_NORMAL = 200;
        private void AdjustLifeNormal(double amount, int player)
        {
            if (_players[player].Life + amount > 100)
            {
                if (_players[player].Life >= 100)
                {
                    _players[player].Life += amount / 3;
                }
                else
                {
                    double over = _players[player].Life + amount - 100;
                    _players[player].Life = 100 + (over / 3);
                }

            }
            else
            {
                _players[player].Life += amount;
            }

            _players[player].Life = Math.Min(LIFE_MAX_NORMAL, _players[player].Life);
            if (_players[player].Life <= 0)
            {
                _players[player].KO = true;
                _players[player].Life = 0;
            }
            
        }

        public void SetLife(double amount, int player)
        {
            _players[player].Life = amount;
            switch (_gameType)
            {
                case GameType.NORMAL:
                    _lifebars[player].SetLife(_players[player].Life);
                    break;
                case GameType.COOPERATIVE:
                    ((CoopLifebar)_lifebars[0]).SetLife(_players[player].Life, player);   
                    break; 
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                    for (int x = 0; x < 4; x++)
                    {
                        if (_players[x].Playing)
                        _lifebars[x].Draw(spriteBatch);
                    }
                    break;
                case GameType.COOPERATIVE:
                    if (_players[0].Playing || _players[1].Playing)
                    {
                        ((CoopLifebar) _lifebars[0]).SideLocationTop = false;
                        _lifebars[0].SetPosition(_metrics["CoopLifebar", 0]);
                        _lifebars[0].Draw(spriteBatch);
                    }
                    if (_players[2].Playing || _players[3].Playing)
                    {
                        ((CoopLifebar)_lifebars[0]).SideLocationTop = true;
                        _lifebars[0].SetPosition(_metrics["CoopLifebar", 1]);
                        _lifebars[0].Draw(spriteBatch);
                    }
                    break;
            }
        }

        public void Reset()
        {
                    for (int x = 0; x < 4; x++)
                    {
                        if (_lifebars[x] != null)
                        {
                            _lifebars[x].Reset();

                        }
                    }
                    for (int x = 0; x < 4; x++)
                    {
                        SetLife(_players[x].Life, x);
                    }
        }
    }
}
