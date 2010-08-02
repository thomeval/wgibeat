﻿using System;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;

namespace WGiBeat.Drawing.Sets
{
    public class NoteJudgementSet :DrawableObject
    {
        private readonly Player[] _players;
        private readonly DisplayedJudgement[] _displayedJudgements;
        private readonly MetricsManager _metrics;
        private readonly GameType _gameType;
        private double _phraseNumber;

        private GraphicNumber _streakNumbers;

        public readonly int[] JudgementCutoffs = {20, 50, 125, 250};
        public NoteJudgementSet()
        {
            _displayedJudgements = new DisplayedJudgement[4];
        }
        public NoteJudgementSet(MetricsManager metrics, Player[] players, GameType type)
            :this()
        {
            _metrics = metrics;
            _players = players;
            _gameType = type;

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
            switch (judgement)
            {
                case BeatlineNoteJudgement.IDEAL:
                    _players[player].Streak++;

                    decimal multiplier = Convert.ToDecimal((9 + Math.Max(1, _players[player].Streak)));
                    multiplier /= 10;

                    _players[player].Score += (long)(1000 * numCompleted * multiplier);
                    lifeAdjust = (1 * numCompleted);
                    break;
                case BeatlineNoteJudgement.COOL:
                    _players[player].Score += 750 * numCompleted;
                    lifeAdjust = (0.5 * numCompleted);
                    _players[player].Streak = 0;
                    break;
                case BeatlineNoteJudgement.OK:
                    _players[player].Score += 500 * numCompleted;
                    _players[player].Streak = 0;
                    break;
                case BeatlineNoteJudgement.BAD:
                    _players[player].Score += 250 * numCompleted;
                    _players[player].Streak = 0;
                    lifeAdjust = -1 * numCompleted;
                    break;
                case BeatlineNoteJudgement.MISS:
                    lifeAdjust = _players[player].MissedBeat();
                    break;
                default:
                    //FAIL
                    _players[player].Streak = 0;
                    lifeAdjust = 0 - (int)(1 + _players[player].PlayDifficulty) * (numNotCompleted + 1);
                    _players[player].Momentum = (long)(_players[player].Momentum * 0.7);
                    break;
            }

            _players[player].Judgements[(int)judgement]++;

            var newDj = new DisplayedJudgement { DisplayUntil = _phraseNumber + 0.5, Height = 40, Width = 150, Player = player, Tier = (int)judgement };
            newDj.SetPosition(_metrics["Judgement", player]);
            _displayedJudgements[player] = newDj;

            return lifeAdjust;
        }

        private void DrawStreakCounters(SpriteBatch spriteBatch)
        {
            Color streakColor = new Color(10, 123, 237, 255);
            if (_streakNumbers == null)
            {
                _streakNumbers = new GraphicNumber
                {
                    SpacingAdjustment = -1,
                    SpriteMap = new SpriteMap
                    {
                        Columns = 3,
                        Rows = 4,
                        SpriteTexture = TextureManager.Textures["streakNumbers"]
                    }
                };
            }
            for (int x = 0; x < 4; x++)
            {
                if (_players[x].Streak > 1)
                {
                    
                    if (_displayedJudgements[x] == null)
                    {
                        continue;
                    }
                    if (_displayedJudgements[x].Tier != 0)
                    {
                        continue;
                    }
                    //_streakNumbers.SpriteMap.ColorShading.A = _displayedJudgements[x].Opacity;
                    streakColor.A = _displayedJudgements[x].Opacity;
                    spriteBatch.DrawString(TextureManager.Fonts["TwoTechLarge"], "x" + _players[x].Streak,_metrics["StreakText",x],streakColor);
                   // _streakNumbers.DrawNumber(spriteBatch, _players[x].Streak, _metrics["StreakText", x], 30, 40);
                }
            }
        }
    }
}