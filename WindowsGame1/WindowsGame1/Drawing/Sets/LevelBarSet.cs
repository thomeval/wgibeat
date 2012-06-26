using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class LevelBarSet : DrawableObjectSet 
    {

        private readonly LevelBar[] _levelBars;
        public readonly Color[] MaxHighlightColors = { new Color(255, 128, 128), new Color(128, 128, 255), new Color(128, 255, 128), new Color(255, 255, 128) };

        public LevelBarSet(MetricsManager metrics, Player[] players, GameType gameType)
            : base(metrics,players,gameType)
        {
            _levelBars = new LevelBar[4];
            CreateLevelBars();
        }

        private void CreateLevelBars()
        {
            var numPlayers = (from e in Players where e.Playing select e).Count();
            for (int x = 0; x < _levelBars.Length; x++)
            {
                
                _levelBars[x] = new LevelBar
                                    {
                                        Parent = this,
                                        Height = 25,
                                        Width = 216,
                                        PlayerID = x,
                                        Multiplier = _gameType == GameType.SYNC_PLUS ?  numPlayers : 1
                                    };
                _levelBars[x].Position = (_metrics["LevelBarBase", x]);
                
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
           Draw(spriteBatch,0.0);
        }
        public void Draw(SpriteBatch spriteBatch, double gameTime)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.COOPERATIVE:
                case GameType.TEAM:
                case GameType.VS_CPU:
                    for (int x = 0; x < _levelBars.Length; x++)
                    {
                        if (Players[x].Playing)
                        {
                            _levelBars[x].Draw(spriteBatch,gameTime);
                        }
                    }
                    break;

                    case GameType.SYNC_PRO:
                    case GameType.SYNC_PLUS:
                        _levelBars[0].Position = _metrics["SyncLevelBarBase", 0];
                        _levelBars[0].Draw(spriteBatch,gameTime); 
         
                    break;
            }

        }

        public void AdjustMomentum(BeatlineNoteJudgement judgement, int player)
        {
           
            if (judgement == BeatlineNoteJudgement.MISS)
            {
                MultiplyMomentum(0.8, player);
            }
            else if (judgement == BeatlineNoteJudgement.FAIL)
            {
              MultiplyMomentum(0.7,player);   
            }
            else
            {
                //Using Players[player] for Sync mode too doesn't matter, since all players will have
                //the same difficulty.
                var amount = (long)
                    (MomentumJudgementMultiplier(judgement)*
                     MomentumIncreaseByDifficulty(Players[player].PlayerOptions.PlayDifficulty));
                if (_gameType == GameType.SYNC_PRO)
                {
                 
                    SetMomentumSync(Players[0].Momentum + amount);
                }
                else
                {
                    Players[player].Momentum += amount;
                }
 
            }

        }

        public void MultiplyMomentum(double amount, int player)
        {

            if (SyncGameType)
            {
          
                SetMomentumSync((long) (Players[0].Momentum*amount));
            }
            else
            {
                Players[player].Momentum = (long)(Players[player].Momentum * amount);
            }

        }

        private void SetMomentumSync(long amount)
        {
            
            for (int x = 0; x < 4; x++)
            {
                Players[x].Momentum = amount;
            }
        }

        public void AdjustForFault(int player)
        {
            MultiplyMomentum(0.95, player);
        }

        private static long MomentumIncreaseByDifficulty(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.BEGINNER:
                    return 15;
                case Difficulty.EASY:
                    return 40;
                case Difficulty.MEDIUM:
                    return 70;
                case Difficulty.HARD:
                    return 175;
                case Difficulty.INSANE:
                    return 300;
                    case Difficulty.RUTHLESS:
                    return 750;
                default:
                    return 0;
            }
        }

        private double MomentumJudgementMultiplier(BeatlineNoteJudgement judgement)
        {
            switch (judgement)
            {
                case BeatlineNoteJudgement.IDEAL:
                    return 1.0;
                case BeatlineNoteJudgement.COOL:
                    return 2.0 / 3;
                case BeatlineNoteJudgement.OK:
                    return 1.0 / 3;
                    
            }
            return 0.0;
        }


    }
}
