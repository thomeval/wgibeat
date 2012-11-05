using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
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
        private double _rainbowPoint;

        private readonly Beatline[] _beatlines;

        private readonly Color[] _blazingColors = {
                                                      new Color(255, 128, 128),
                                                      new Color(128, 255, 128),
                                                      new Color(128, 128, 255),
                                                      new Color(255, 128, 128)
                                                  };

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


        public double EndingPhrase { get; set; }
        public IEnumerable<double> AddNotes { get; set; }
        public IEnumerable<double> RemoveNotes { get; set; }
        public IEnumerable<double> SuperNotes { get; set; } 

        public event EventHandler NoteMissed;
        public event EventHandler CPUNoteHit;


        public BeatlineSet(MetricsManager metrics, Player[] players, GameType gameType)
            : base(metrics, players, gameType)
        {
            _beatlines = new Beatline[4];
            var visibleCount = 0;
            AddNotes = new double[0];
            RemoveNotes = new double[0];
            SuperNotes = new double[0];

            for (int x = 0; x < 4; x++)
            {
              
                _beatlines[x] = new Beatline
                                    {
                                        //Beatlines are arranged differently for SYNC_PRO mode.
                                        Position = SyncGameType ? _metrics["SyncBeatlineBarBase",visibleCount]: (_metrics["BeatlineBarBase", x]),
                                        Size = new Vector2(350, 125),
                                        Id = x,
                                        IdentifierSize = _metrics["BeatlinePlayerIdentifiers.Size", 0],
                                        EffectIconSize = _metrics["BeatlineEffectIcons.Size", 0]

                                    };
                if (Players[x].Playing)
                {
                    visibleCount++;
                }
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
                    SetBeatlineSpeed(x);
                    _beatlines[x].Colour = GetBeatlineColour(x);
                    _beatlines[x].DisablePulse = Players[x].KO;
                    _beatlines[x].Draw(phraseNumber);
                }
            }

        }

        private Color GetBeatlineColour(int player)
        {
            if (Players[player].IsBlazing || SyncGameType && Players[0].IsBlazing)
            {
                _rainbowPoint = (_rainbowPoint + TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * 2) % 3;
                return Color.Lerp(_blazingColors[(int)Math.Floor(_rainbowPoint)], _blazingColors[(int)Math.Ceiling(_rainbowPoint)],
                                                      (float)(_rainbowPoint - Math.Floor(_rainbowPoint)));
            }

            return Color.White;
        }

        private void SetBeatlineSpeed(int x)
        {
            if (_gameType == GameType.COOPERATIVE)
            {
                _beatlines[x].Speed = Player.GrooveMomentum;
            }
            else
            {
                _beatlines[x].Speed = Players[x].PlayerOptions.BeatlineSpeed;
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
            
                for (int y = 0; y < missedNotes; y++)
                {
                    if (NoteMissed != null)
                    {
                        NoteMissed(x, null);
                    }
                }
            }
//
  //          GenerateMoreBeatlineNotes(phraseNumber);

        }

        private void GenerateMoreBeatlineNotes(double phraseNumber)
        {
            //Extend method using switch block if different rules are required.
            //Keep generating until end of song.
            while ((phraseNumber + 2 > _lastBeatlineNote) && (_lastBeatlineNote + 1 < EndingPhrase))
            {
                _lastBeatlineNote++;
                for (int x = 0; x < 4; x++)
                {
                    if ((!Players[x].KO) && (Players[x].Playing) && (!RemoveNotes.Contains(_lastBeatlineNote)))
                    {
                        _beatlines[x].AddBeatlineNote(new BeatlineNote {Position = _lastBeatlineNote, NoteType = BeatlineNoteType.NORMAL });
                    }
                }
            }
        }
        

        public void AddTimingPointMarkers(GameSong song)
        {
            for (int index = 0; index < _beatlines.Length; index++)
            {
                if (!Players[index].Playing)
                {
                    continue;
                }
                var bl = _beatlines[index];
                GenerateStandardNotes(song, bl);
                GenerateSongEndNote(song, bl);
                GenerateBPMChangeNotes(song, bl);
                GenerateStopNotes(song, bl);
            }
        }

        public BeatlineNote NearestBeatlineNote(int player, double phraseNumber)
        {
            return _beatlines[player].NearestBeatlineNote(phraseNumber);
        }
        private void GenerateStandardNotes(GameSong song, Beatline bl)
        {

            for (int beat = 0; beat < song.GetEndingTimeInPhrase(); beat++ )
            {
                if (song.RemoveNotes.Contains(beat))
                {
                    continue;
                }
                if (song.SuperNotes.Contains(beat))
                {
                    continue;
                }
               bl.AddBeatlineNote(new BeatlineNote { Position = beat});

            }
             
            foreach (var pos in song.AddNotes)
            {
                bl.AddBeatlineNote(new BeatlineNote{Position = pos});
            }

            foreach (var pos in song.SuperNotes)
            {
                bl.AddBeatlineNote(new BeatlineNote { Position = pos, NoteType=BeatlineNoteType.SUPER });
            }
                  
        }

        private void GenerateSongEndNote(GameSong song, Beatline bl)
        {
                bl.AddBeatlineNote(new BeatlineNote
                                       {
                                           NoteType = BeatlineNoteType.END_OF_SONG,
                                           Position = song.GetEndingTimeInPhrase()
                                       });
 
        }

        private static void GenerateBPMChangeNotes(GameSong song, Beatline bl)
        {
            double prev = song.BPMs[0.0];
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


                bl.InsertBeatlineNote(new BeatlineNote {NoteType = noteType, Position = bpmKey}, 0);


                prev = song.BPMs[bpmKey];
            }
        }

        private static void GenerateStopNotes(GameSong song, Beatline bl)
        {
            foreach (var stopKey in song.Stops.Keys)
            {
                bl.InsertBeatlineNote(
                    new BeatlineNote {NoteType = BeatlineNoteType.STOP, Position = stopKey }, 0);
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
