using System;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;


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
                    bl.AddBeatlineNote(new BeatlineNote{Player = -1 , NoteType = BeatlineNoteType.END_OF_SONG, Position = EndingPhrase});
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
                _beatlines[x] = new Beatline {Position = (_metrics["BeatlineBarBase", x]), Id = x};
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
                    _beatlines[x].Id = 4;
                    var miss = Players[x].NextCPUJudgement == BeatlineNoteJudgement.MISS;
                    if ((!miss))
                    {

                        var notesHit = _beatlines[x].AutoHit(phraseNumber);

                        for (int y = 0; y < notesHit; y++)
                        {
                            if (CPUNoteHit != null)
                            {
                                CPUNoteHit(x, null);
                            }
                        }
                    }
                }
                if (Players[x].KO)
                {
                    _beatlines[x].RemoveAll();
                }
                var missedNotes = _beatlines[x].TrimExpired(phraseNumber);
               // _beatlines[x].Id = x;
                for (int y = 0; y < missedNotes; y++)
                {
                    if (NoteMissed != null)
                    {
                        NoteMissed(x, null);
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
                        _beatlines[x].AddBeatlineNote(new BeatlineNote { Player = x, Position = _lastBeatlineNote, NoteType = BeatlineNoteType.NORMAL });
                    }
                }
            }
        }


        public void AddTimingPointMarkers(GameSong song)
        {
            double prev;

            foreach (Beatline bl in _beatlines)
            {
                prev = song.BPMs[0.0];
                foreach (double bpmKey in song.BPMs.Keys)
                {
                    if (bpmKey == 0.0)
                    {
                        continue;
                    }

                    //Don't mark a BPM change if there isn't actually a change in the BPM.
                    if (song.BPMs[bpmKey] == prev)
                    {
                        continue;
                    }

                        var noteType = song.BPMs[bpmKey] > prev
                                           ? BeatlineNoteType.BPM_INCREASE
                                           : BeatlineNoteType.BPM_DECREASE;
                    

                        bl.InsertBeatlineNote(new BeatlineNote {Player = -1, NoteType = noteType, Position = bpmKey}, 0);

                    
                    prev = song.BPMs[bpmKey];

                }

                foreach (double stopKey in song.Stops.Keys)
                {
                    bl.InsertBeatlineNote(
                        new BeatlineNote {Player = -1, NoteType = BeatlineNoteType.STOP, Position = stopKey}, 0);
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
                _beatlines[x].Speed = Players[x].PlayerOptions.BeatlineSpeed;
            }
        }

        public void Reset()
        {
            _lastBeatlineNote = -1;
            foreach (Beatline beatline in _beatlines)
            {
                beatline.ClearNotes();
            }
        }
    }
}
