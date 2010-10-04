using System;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;


namespace WGiBeat.Drawing.Sets
{
    public class BeatlineSet : DrawableObject
    {
        private readonly MetricsManager _metrics;
        public readonly Player[] Players;
        private readonly GameType _gameType;
        private double _lastBeatlineNote = -1;
        private readonly Beatline[] _beatlines;

        private double _bpm;
        public double Bpm
        {
            get { return _bpm; }
            set
            {
                _bpm = value;
                for (int x = 0; x < 4; x++)
                {
                    _beatlines[x].Bpm = value;
                }
            }
        }

        private double _endingPhrase;
        public double EndingPhrase
        {
            get { return _endingPhrase; }
            set
            {
                _endingPhrase = value;
                foreach (Beatline bl in _beatlines)
                {
                    bl.EndPhrase = EndingPhrase;
                }
            }
        }

        public event EventHandler NoteMissed;
        public BeatlineSet()
        {
            _beatlines = new Beatline[4];
        }

        public BeatlineSet(MetricsManager metrics, Player[] players, GameType gameType)
            : this()
        {
            _metrics = metrics;
            Players = players;
            _gameType = gameType;


            for (int x = 0; x < 4; x++)
            {
                _beatlines[x] = new Beatline();
                _beatlines[x].SetPosition(_metrics["BeatlineBarBase", x]);
                _beatlines[x].Id = x;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0.0);
        }

        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            for (int x = 0; x < 4; x++)
            {
                if (Players[x].Playing)
                {
                    _beatlines[x].DisablePulse = Players[x].KO;
                    _beatlines[x].Draw(spriteBatch, phraseNumber);
                }
            }

        }

        public void MaintainBeatlineNotes(double phraseNumber)
        {

            for (int x = 0; x < 4; x++)
            {
                if (Players[x].KO)
                {
                    _beatlines[x].RemoveAll();
                }
                var missedNotes = _beatlines[x].TrimExpired(phraseNumber);

                for (int y = 0; y < missedNotes; y++)
                {
                    if (NoteMissed != null)
                    {
                        NoteMissed(_beatlines[x], null);
                    }
                }
            }

            GenerateMoreBeatlineNotes(phraseNumber);

        }

        private void GenerateMoreBeatlineNotes(double phraseNumber)
        {
            //Extend method using switch block if different rules are required.
            //Keep generating until end of song.
            if ((phraseNumber + 2 > _lastBeatlineNote) && (_lastBeatlineNote + 1 < EndingPhrase))
            {
                _lastBeatlineNote++;
                for (int x = 0; x < 4; x++)
                {
                    if ((!Players[x].KO) && (Players[x].Playing))
                    {
                        _beatlines[x].AddBeatlineNote(new BeatlineNote { Player = x, Position = _lastBeatlineNote });
                    }
                }
            }
        }


        public BeatlineNoteJudgement AwardJudgement(double phraseNumber, int player, bool completed)
        {
            var result = _beatlines[player].DetermineJudgement(phraseNumber, completed);
            return result;
        }


        public double CalculateHitOffset(int player, double phraseNumber)
        {
            return _beatlines[player].CalculateHitOffset(phraseNumber);
        }


        public void SetSpeeds()
        {
            for (int x = 0; x < 4; x++)
            {
                _beatlines[x].Speed = Players[x].BeatlineSpeed;
            }
        }
    }
}
