using System;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing.Sets
{
    public class LifeBarSet : DrawableObject
    {

        private readonly MetricsManager _metrics;
        public readonly Player[] Players; 
        private readonly GameType _gameType;
        private readonly LifeBar[] _lifeBars;

        public LifeBarSet()
        {
            _lifeBars = new LifeBar[4];
        }
        public LifeBarSet(MetricsManager metrics, Player[] players, GameType gameType)
            :this()
        {
            _metrics = metrics;
            Players = players;
            _gameType = gameType;
            CreateLifeBars();
        }


        private void CreateLifeBars()
        {

            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                    for (int x = 0; x < 4; x++)
                    {
                        _lifeBars[x] = new NormalLifeBar {Height = 30, Width = 260, PlayerID = x, Parent = this};
                        _lifeBars[x].SetPosition(_metrics["NormalLifeBar", x]);
 
                    }
                    break;
                case GameType.COOPERATIVE:
                    _lifeBars[0] = new CoopLifeBar {Height = 30, Width = 785, Parent = this};   
                    break;
            }
        }

        public double AdjustLife(double amount, int player)
        {
            //Adjust the life according to the 'rules' of the lifebar used, and return the new amount.
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.TEAM:
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
            var theLifebar = (CoopLifeBar) _lifeBars[0];
            var limit = (theLifebar.Participants() * 100) - theLifebar.TotalLife();
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
                    Players[player].Life += amount / 2;
                }
                else
                {
                    double over = Players[player].Life + amount - 100;
                    Players[player].Life = 100 + (over / 2);
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0.0);
        }
        public void Draw(SpriteBatch spriteBatch, double gameTime)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                    for (int x = 0; x < 4; x++)
                    {
                        if (Players[x].Playing)
                            _lifeBars[x].Draw(spriteBatch, gameTime);
                    }
                    break;
                case GameType.COOPERATIVE:
                    if (Players[0].Playing || Players[1].Playing)
                    {
                        ((CoopLifeBar) _lifeBars[0]).SideLocationTop = false;
                        _lifeBars[0].SetPosition(_metrics["CoopLifeBar", 0]);
                        _lifeBars[0].Draw(spriteBatch,gameTime);
                    }
                    if (Players[2].Playing || Players[3].Playing)
                    {
                        ((CoopLifeBar)_lifeBars[0]).SideLocationTop = true;
                        _lifeBars[0].SetPosition(_metrics["CoopLifeBar", 1]);
                        _lifeBars[0].Draw(spriteBatch, gameTime);
                    }
                    break;
            }
        }

        public void Reset()
        {
            for (int x = 0; x < 4; x++)
            {
                if (_lifeBars[x] != null)
                {
                    _lifeBars[x].Reset();

                }
            }
            for (int x = 0; x < 4; x++)
            {
                SetLife(Players[x].Life, x);
            }
        }

    }
}