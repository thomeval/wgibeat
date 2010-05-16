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
        public readonly Player[] Players; 
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
            Players = players;
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
                        _lifebars[x] = new NormalLifebar {Height = 30, Width = 260, PlayerID = x, Parent = this};
                        _lifebars[x].SetPosition(_metrics["NormalLifebar", x]);
 
                    }
                    break;
                    case GameType.COOPERATIVE:
                    _lifebars[0] = new CoopLifebar {Height = 30, Width = 785, Parent = this};   
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
                    break;
                case GameType.COOPERATIVE:
                    AdjustLifeCoop(amount, player);          
                    break;
            }
            return Players[player].Life;
        }

        private void AdjustLifeCoop(double amount, int player)
        {
            var theLifebar = (CoopLifebar) _lifebars[0];
            var limit = (theLifebar.Participants() * 125) - theLifebar.TotalLife();
            Players[player].Life += Math.Min(limit, amount);

            if (theLifebar.TotalLife() <= 0)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (Players[x].Playing)
                    {
                        Players[x].KO = true;
                    }
                }
            }
        }

        const int LIFE_MAX_NORMAL = 200;
        private void AdjustLifeNormal(double amount, int player)
        {
            if (Players[player].Life + amount > 100)
            {
                if (Players[player].Life >= 100)
                {
                    Players[player].Life += amount / 3;
                }
                else
                {
                    double over = Players[player].Life + amount - 100;
                    Players[player].Life = 100 + (over / 3);
                }

            }
            else
            {
                Players[player].Life += amount;
            }

            Players[player].Life = Math.Min(LIFE_MAX_NORMAL, Players[player].Life);
            if (Players[player].Life <= 0)
            {
                Players[player].KO = true;
                Players[player].Life = 0;
            }
            
        }

        public void SetLife(double amount, int player)
        {
            Players[player].Life = amount;

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                    for (int x = 0; x < 4; x++)
                    {
                        if (Players[x].Playing)
                        _lifebars[x].Draw(spriteBatch);
                    }
                    break;
                case GameType.COOPERATIVE:
                    if (Players[0].Playing || Players[1].Playing)
                    {
                        ((CoopLifebar) _lifebars[0]).SideLocationTop = false;
                        _lifebars[0].SetPosition(_metrics["CoopLifebar", 0]);
                        _lifebars[0].Draw(spriteBatch);
                    }
                    if (Players[2].Playing || Players[3].Playing)
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
                        SetLife(Players[x].Life, x);
                    }
        }
    }
}
