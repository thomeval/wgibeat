using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class NoteJudgementSet :DrawableObjectSet
    {
        private readonly DisplayedJudgement[] _displayedJudgements;
        private readonly LifeBarSet _lifeBarSet;
        private readonly ScoreSet _scoreSet;

        private double _phraseNumber;

        
        public static readonly int[] JudgementCutoffs = {20, 50, 125, 250,1500};

        public NoteJudgementSet(MetricsManager metrics, Player[] players, GameType type, LifeBarSet lifeBarSet, ScoreSet scoreSet)
            :base(metrics,players,type)
        {
            _displayedJudgements = new DisplayedJudgement[4];
            _lifeBarSet = lifeBarSet;
            _scoreSet = scoreSet;
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

        }

        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            _phraseNumber = phraseNumber;
            Draw(spriteBatch);
        }

        public void AwardJudgement(BeatlineNoteJudgement judgement, int player, int givenMultiplier, int numCompleted, int numNotCompleted)
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
                    lifeAdjust = 0 - (int)(1 + Players[player].PlayerOptions.PlayDifficulty) * (numNotCompleted + 1);
                    break;
                    case BeatlineNoteJudgement.COUNT:
                    //Ignore judgement
                    break;
            }

            RecordJudgement(player, judgement);


            if (Players[player].CPU)
            {
                scoreAdjust *= NumHumanPlayers();
            }

            scoreAdjust *= givenMultiplier;
            if (_gameType == GameType.COOPERATIVE)
            {
                scoreAdjust = (long) Math.Ceiling(scoreAdjust*(Player.GrooveMomentum));
            }
            if (_scoreSet != null)
            {
                _scoreSet.AdjustScore(scoreAdjust, player);
            }
            if (_lifeBarSet != null)
            {
                _lifeBarSet.AdjustLife(lifeAdjust, player);
            }

            if (Players[player].Playing)
            {
                var newDj = new DisplayedJudgement
                                {
                                    DisplayUntil = _phraseNumber + 0.5,
                                    Size = _metrics["Judgement.Size",0],
                                    Player = player,
                                    Tier = (int) judgement,
                                  
                                };
                newDj.Position = (_metrics["Judgement", player]);

                if (_gameType == GameType.SYNC)
                {
                    newDj.Position = _metrics["SyncJudgement", player];
                    newDj.Size = _metrics["SyncJudgement.Size",0].Clone();
                    newDj.Height *= (from e in Players where e.Playing select e).Count();
                }
                _displayedJudgements[player] = newDj;
            }
        }

        private void RecordJudgement(int player, BeatlineNoteJudgement judgement)
        {

            Players[player].Judgements[(int)judgement]++;

            if (_gameType != GameType.SYNC) return;
            
            for (int x = 1; x < 4; x++)
            {
                if (Players[x].Playing)
                {
                    Players[x].Judgements[(int) judgement] = Players[0].Judgements[(int) judgement];
                }
            }
        }


        private long NumHumanPlayers()
        {
            return (from e in Players where e.Playing && !(e.CPU) select e).Count();
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