using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class LifeBarSet : DrawableObjectSet
    {

        private readonly LifeBar[] _lifeBars;
        private double _lastBlazeCheck;

        public event EventHandler<ObjectEventArgs> BlazingEnded;

        public LifeBarSet(MetricsManager metrics, Player[] players, GameType gameType)
            :base(metrics,players,gameType)
        {
            _lifeBars = new LifeBar[4];
            CreateLifeBars();
        }


        private void CreateLifeBars()
        {

            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                case GameType.VS_CPU:
                    for (int x = 0; x < 4; x++)
                    {
                        _lifeBars[x] = new NormalLifeBar
                                           {
                                               Height = 30,
                                               Width = 260,
                                               PlayerID = x,
                                               Parent = this,
                                               Position = (_metrics["NormalLifeBar", x])
                                           };
                    }
                    break;
                case GameType.COOPERATIVE:
                    _lifeBars[0] = new CoopLifeBar {Height = 30, Width = 785, Parent = this};
                    ((CoopLifeBar)_lifeBars[0]).TrueCapacity = GetTotalCapacity();
                    ((CoopLifeBar)_lifeBars[0]).BaseCapacity = GetBaseCapacity();
                    break;
            }
        }

        private double GetBaseCapacity()
        {
            return (from e in Players where e.Playing select e).Count()*100;
        }

        private double GetTotalCapacity()
        {
            return (from e in Players where e.Playing select e.GetMaxLife()).Sum();
        }

        public double AdjustLife(double amount, int player)
        {
            //Adjust the life according to the 'rules' of the lifebar used, and return the new amount.
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                case GameType.VS_CPU:
                    return AdjustLifeNormal(amount, player);
                    
                case GameType.COOPERATIVE:
                    return AdjustLifeCoop(amount, player);          
                    
            }
            return Players[player].Life;
        }

        private double AdjustLifeCoop(double amount, int player)
        {
            var theLifebar = (CoopLifeBar) _lifeBars[0];
            Players[player].Life = Math.Min(Players[player].GetMaxLife(), Players[player].Life + amount);

            if (!AnyPlayerHasDisabledKO())
            {
                return amount;
            }
            if (theLifebar.TotalLife() <= 0)
            {
                for (int x = 0; x < 4; x++)
                {
                        Players[x].KO = true;
                }
            }
            return amount;
        }

        private bool AnyPlayerHasDisabledKO()
        {
            var result = (from e in Players where e.Playing && !e.CPU select e);
            return (result.Any(e => e.DisableKO));
        }


        private double AdjustLifeNormal(double amount, int player)
        {
            var old = Players[player].Life;
            if ((amount > 0) && (Players[player].Life + amount > 100))
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


            Players[player].Life = Math.Min(Players[player].GetMaxLife(), Players[player].Life);
            if ((!Players[player].CPU) && (Players[player].Life <= 0) && (!Players[player].DisableKO))
            {
                Players[player].KO = true;
                Players[player].Life = 0;
            }
            return Players[player].Life - old;
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
                    case GameType.VS_CPU:
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
                        _lifeBars[0].Position = (_metrics["CoopLifeBar", 0]);
                        _lifeBars[0].Draw(spriteBatch,gameTime);
                    }
                    if (Players[2].Playing || Players[3].Playing)
                    {
                        ((CoopLifeBar)_lifeBars[0]).SideLocationTop = true;
                        _lifeBars[0].Position = (_metrics["CoopLifeBar", 1]);
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
            _lastBlazeCheck = 0.0;
        }

        public void MaintainBlazings(double phraseNumber)
        {

            if (phraseNumber - _lastBlazeCheck <= 0.25) return;
            _lastBlazeCheck += 0.25;
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                    MaintainBlazingsNormal();
                    break;
                case GameType.COOPERATIVE:
                    MaintainBlazingsCoop();
                    break;
            }

        }

        private void MaintainBlazingsCoop()
        {
            var numBlazers = (from e in Players where e.Playing && e.IsBlazing select e).Count();
            var totalOvercharge = (from e in Players where e.Playing select  e.Life - 100).Sum();

            //Cancel blazing mode and award post-blazing life penalty to all who participated when
            //overcharge runs out.
            if (totalOvercharge <= 0)
            {
                for (int x = 0; x < 4; x++ )
                {
                    if (Players[x].IsBlazing)
                    {
                        Players[x].Life -= 25;
                        Players[x].IsBlazing = false;
                        if (BlazingEnded != null)
                        {
                            BlazingEnded(this, new ObjectEventArgs {Object = x});
                        }
                    }
                }
                return;
            }

            //In coop mode, blazing costs 1 life point per beat (1/4 beatline), times the number of blazing players.
            //The cost is distributed in proportion to the overcharge contribution of the players.
            for (int x = 0; x < 4; x++)
            {
                if (!Players[x].Playing)
                {
                    continue;
                }
                var playerOvercharge = Players[x].Life - 100;
                var reductionAmount = numBlazers*playerOvercharge/totalOvercharge;
                reductionAmount = Math.Max(0, reductionAmount);
                reductionAmount = Math.Min(numBlazers, reductionAmount);
                Players[x].Life -= Math.Max(0,reductionAmount);
            }
        }

        private void MaintainBlazingsNormal()
        {
            const int MIN_BLAZING_AMOUNT = 100;
            for (int x = 0; x < 4; x++)
            {
                if ((Players[x].IsBlazing))
                {
                    Players[x].Life--;

                    if (Players[x].Life < MIN_BLAZING_AMOUNT)
                    {
                        Players[x].Life -= 25;
                        Players[x].IsBlazing = false;
                        if (BlazingEnded != null)
                        {
                            BlazingEnded(this, new ObjectEventArgs { Object = x });
                        }
                    }
                }
            }
        }

        public void ToggleBlazing(int player)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                    if (Players[player].Life > 100)
                    {
                        Players[player].IsBlazing = true;
                    }
                    break;
                case GameType.COOPERATIVE:
                    var totalOvercharge = (from e in Players where e.Playing select e.Life - 100).Sum();
                    if (totalOvercharge > 0)
                    {
                        Players[player].IsBlazing = true;
                    }
                    break;
            }

        }
    }
}