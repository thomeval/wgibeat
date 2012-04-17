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
        public readonly Color[] FullHighlightColors = {new Color(255,128,128), new Color(128,128,255), new Color(128,255,128), new Color(255,255,128)  };

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
                case GameType.SYNC:
                    for (int x = 0; x < 4; x++)
                    {
                        _lifeBars[x] = new NormalLifeBar
                                           {
                                               Height = 25,
                                               Width = 350,
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

        public void AdjustLife(double amount, int player)
        {
            //Adjust the life according to the 'rules' of the lifebar used, and return the new amount.
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                case GameType.VS_CPU:
                    AdjustLifeNormal(amount, player);
                    CheckForNormalKO(player);
                    ClipNormalMaxLife(player);
                    break;
                case GameType.COOPERATIVE:
                    AdjustLifeCoop(amount, player);
                    ClipCoopMaxLife();
                    break;
                    case GameType.SYNC:
                    AdjustLifeSync(amount,player);
                    break;
                    
            }

        }

        private void AdjustLifeCoop(double amount, int player)
        {
            
            AdjustLifeNormal(amount,player);
            CheckForCoopKO();
        }

        private void CheckForCoopKO()
        {
            if (AnyPlayerHasDisabledKO())
            {
                return;
            }
            var theLifebar = (CoopLifeBar)_lifeBars[0];
            if (theLifebar.TotalLife() <= 0)
            {
                for (int x = 0; x < 4; x++)
                {
                    Players[x].KO = true;
                }
            }
        }

        private bool AnyPlayerHasDisabledKO()
        {
            var result = (from e in Players where e.Playing && !e.CPU select e);
            return (result.Any(e => e.PlayerOptions.DisableKO));
        }


        private void AdjustLifeNormal(double amount, int player)
        {
            if ((amount > 0) && (Players[player].Life + amount > 100))
            {
                if (Players[player].Life >= 100)
                {
                    Players[player].Life += amount * 0.75;
                }
                else
                {
                    
                    double over = Players[player].Life + amount - 100;
                    Players[player].Life = 100 + (over * 0.75);

                }
            }
            else
            {
                Players[player].Life += amount;
            }
        }

        private void CheckForNormalKO(int player)
        {
            if ((!Players[player].CPU) && (Players[player].Life <= 0) && (!Players[player].PlayerOptions.DisableKO))
            {
                Players[player].KO = true;
                Players[player].Life = 0;
            }
        }

        private void ClipNormalMaxLife(int player)
        {
            Players[player].Life = Math.Min(Players[player].GetMaxLife(), Players[player].Life);   
        }

        private void ClipCoopMaxLife()
        {
            var theLifeBar = (CoopLifeBar) _lifeBars[0];
            if (theLifeBar.TotalLife() > GetTotalCapacity())
            {
                var adjustAmount = GetTotalCapacity()/theLifeBar.TotalLife();

                for (int x = 0; x < 4; x++)
                {
                    Players[x].Life *= adjustAmount;
                }
            }
        }
        private void AdjustLifeSync(double amount, int player)
        {
     
            AdjustLifeNormal(amount, player);
            ClipSyncMaxLife(0);
            for (int x = 1; x < 4; x++)
            {
                Players[x].Life = Players[0].Life;
            }

            CheckForSyncKO();
        }

        private void ClipSyncMaxLife(int player)
        {
            var maxLife = (from e in Players where e.Playing select e.GetMaxLife()).Min();
            Players[player].Life = Math.Min(maxLife, Players[player].Life);   

        }

        private void CheckForSyncKO()
        {
            if ((Players[0].Life <= 0) && (!AnyPlayerHasDisabledKO()))
            {
                for (int x = 0; x < 4; x++)
                {
                    if (!Players[x].Playing)
                    {
                        continue;
                    }
                    Players[x].KO = true;
                    Players[x].Life = 0;
                }
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
                    case GameType.SYNC:
                    if (Players[0].Playing || Players[1].Playing)
                    {
                        _lifeBars[0].Position = (_metrics["SyncLifeBar", 0]);
                        _lifeBars[0].Draw(spriteBatch, gameTime);
                    }
                    if (Players[2].Playing || Players[3].Playing)
                    {
                        _lifeBars[3].Position = (_metrics["SyncLifeBar", 1]);
                        _lifeBars[3].Draw(spriteBatch, gameTime);
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
            if (_gameType == GameType.SYNC)
            {
                var maxLife = (from e in Players where e.Playing select e.GetMaxLife()).Min();
                Players[0].Life = maxLife/2;
                AdjustLifeSync(0.0,0);
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
                    case GameType.SYNC:
                    MaintainBlazingsNormal();
                    AdjustLifeSync(0,0);
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
                    case GameType.SYNC:
                    {
                        if (Players[0].Life > 100)
                        {
                            Players[0].IsBlazing = true;
                        }
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