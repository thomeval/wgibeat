using System;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Notes;


namespace WGiBeat.Drawing.Sets
{
    public class BeatlineSet : DrawableObjectSet
    {

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

        private bool _large;
        public bool Large
        {
            get { return _large; }
            set
            {
                _large = value;
                for (int x = 0; x <4; x++)
                {
                    _beatlines[x].Large = value;
                }
                if (Large)
                {
                    _beatlines[1].Position = (_metrics["BeatlineBarBase",0]);
                    _beatlines[3].Position = (_metrics["BeatlineBarBase",2]);
                }
                else
                {
                    _beatlines[1].Position = (_metrics["BeatlineBarBase", 1]);
                    _beatlines[3].Position = (_metrics["BeatlineBarBase", 3]);
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
        public event EventHandler CPUNoteHit;


        public BeatlineSet(MetricsManager metrics, Player[] players, GameType gameType)
            : base(metrics,players,gameType)
        {
            _beatlines = new Beatline[4];



            for (int x = 0; x < 4; x++)
            {
                _beatlines[x] = new Beatline();
                _beatlines[x].Position = (_metrics["BeatlineBarBase", x]);
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
                if (Players[x].CPU)
                {
                    var notesHit = _beatlines[x].AutoHit(phraseNumber);

                    for (int y = 0; y < notesHit; y++)
                    {
                        if (CPUNoteHit != null)
                        {
                            CPUNoteHit(_beatlines[x], null);
                        }
                    }
                }
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

        public void Reset()
        {
            _lastBeatlineNote = -1;
        }
    }
}
