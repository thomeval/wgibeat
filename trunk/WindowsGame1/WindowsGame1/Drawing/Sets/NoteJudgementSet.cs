using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class NoteJudgementSet :DrawableObjectSet
    {
        private readonly DisplayedJudgement[] _displayedJudgements;
        private double _phraseNumber;

        public static readonly int[] JudgementCutoffs = {20, 50, 125, 250,1500};

        public NoteJudgementSet(MetricsManager metrics, Player[] players, GameType type)
            :base(metrics,players,type)
        {
            _displayedJudgements = new DisplayedJudgement[4];

        }
        public override void Draw(SpriteBatch spriteBatch)
        {

            for (int x = 0; x < _displayedJudgements.Length; x++)
            {
                if (_displayedJudgements[x] == null)
                {
                    continue;
                }
                int opacity = Convert.ToInt32(Math.Max(0, (_displayedJudgements[x].DisplayUntil - _phraseNumber) * 510));
                opacity = Math.Max(0,Math.Min(opacity, 255));
                _displayedJudgements[x].Opacity = Convert.ToByte(opacity);
                _displayedJudgements[x].Draw(spriteBatch);

                if (opacity == 0)
                {
                    _displayedJudgements[x] = null;
                }
            }

            DrawStreakCounters(spriteBatch);
        }

        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            _phraseNumber = phraseNumber;

            Draw(spriteBatch);
        }

        public double AwardJudgement(BeatlineNoteJudgement judgement, int player, int numCompleted, int numNotCompleted)
        {
            double lifeAdjust = 0;
            long scoreAdjust = 0;
            switch (judgement)
            {
                case BeatlineNoteJudgement.IDEAL:
                    Players[player].Streak++;

                    decimal multiplier = Convert.ToDecimal((9 + Math.Max(1, Players[player].Streak)));
                    multiplier /= 10;

                    scoreAdjust = (long)(1000 * (numCompleted) * multiplier);
                    lifeAdjust = (1 * numCompleted);
                    break;
                case BeatlineNoteJudgement.COOL:
                    scoreAdjust = 750 * numCompleted;
                    lifeAdjust = (0.5 * numCompleted);
                    Players[player].Streak = 0;
                    break;
                case BeatlineNoteJudgement.OK:
                    scoreAdjust = 500 * numCompleted;
                    Players[player].Streak = 0;
                    break;
                case BeatlineNoteJudgement.BAD:
                    scoreAdjust = 250 * numCompleted;
                    Players[player].Streak = 0;
                    lifeAdjust = -1 * numCompleted;
                    break;
                case BeatlineNoteJudgement.MISS:
                    lifeAdjust = Players[player].MissedBeat();
                    break;
                case BeatlineNoteJudgement.FAIL:
                    Players[player].Streak = 0;
                    lifeAdjust = 0 - (int)(1 + Players[player].PlayDifficulty) * (numNotCompleted + 1);
                    Players[player].Momentum = (long)(Players[player].Momentum * 0.7);
                    break;
                    case BeatlineNoteJudgement.COUNT:
                    //Ignore judgement
                    return 0.0;
            }

            Players[player].Judgements[(int)judgement]++;

            if (Players[player].CPU)
            {
                scoreAdjust *= NumHumanPlayers();
            }
            else
            {
                scoreAdjust *= GetBonusMultiplier();
            }
            Players[player].Score += scoreAdjust;

            var newDj = new DisplayedJudgement { DisplayUntil = _phraseNumber + 0.5, Height = 40, Width = 150, Player = player, Tier = (int)judgement };
            newDj.Position = (_metrics["Judgement", player]);
            _displayedJudgements[player] = newDj;

            return lifeAdjust;
        }


        private int GetBonusMultiplier()
        {
            if (_gameType != GameType.COOPERATIVE)
            {
                return 1;
            }
            var blazers = (from e in Players where e.IsBlazing select e).Count();
            switch (blazers)
            {
                case 4:
                    return 8;
                case 3:
                    return 4;
                case 2:
                    return 2;
                default:
                    return 1;
            }

        }
        private long NumHumanPlayers()
        {
            return (from e in Players where e.Playing && !(e.CPU) select e).Count();
        }

        private void DrawStreakCounters(SpriteBatch spriteBatch)
        {
            var streakColor = new Color(10, 123, 237, 255);
    
            for (int x = 0; x < 4; x++)
            {
                if (Players[x].Streak > 1)
                {
                    
                    if (_displayedJudgements[x] == null)
                    {
                        continue;
                    }
                    if (_displayedJudgements[x].Tier != 0)
                    {
                        continue;
                    }
                  
                    streakColor.A = _displayedJudgements[x].Opacity;
                    TextureManager.DrawString(spriteBatch,"x" + Players[x].Streak, "TwoTechLarge",_metrics["StreakText",x],streakColor,FontAlign.LEFT);
              
                }
            }
        }

        public void Reset()
        {
            for (int x = 0; x < 4; x++)
            {
                _displayedJudgements[x] = null;
            }
        }
    }
}