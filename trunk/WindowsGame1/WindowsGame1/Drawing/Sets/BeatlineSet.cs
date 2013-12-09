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

        private double _rainbowPoint;
        private readonly Beatline[] _beatlines;
        private readonly Color[] _blazingColors = {
                                                      new Color(255, 128, 128),
                                                      new Color(128, 255, 128),
                                                      new Color(128, 128, 255),
                                                      new Color(255, 128, 128)
                                                  };
        private double _bpm;

        /// <summary>
        /// Gets or sets the BPM currently used by the beatlines. Used mainly to calculate HitOffsets.
        /// </summary>
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
        public List<double> AddNotes { get; set; }
        public List<double> RemoveNotes { get; set; }
        public List<double> SuperNotes { get; set; } 

        public event EventHandler NoteMissed;
        public event EventHandler CPUNoteHit;

        public BeatlineSet(MetricsManager metrics, Player[] players, GameType gameType)
            : base(metrics, players, gameType)
        {
            _beatlines = new Beatline[4];
            var visibleCount = 0;
            AddNotes = new List<double>();
            RemoveNotes = new List<double>();
            SuperNotes = new List<double>();

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


        public override void Draw()
        {
            Draw(0.0);
        }

        public void Draw(double phraseNumber)
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

                    // The CPU should hit the beatline note if its destined to get a MISS rating.
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

        }
        

        public void AddTimingPointMarkers(GameSong song)
        {
            for (int index = 0; index < _beatlines.Length; index++)
            {
                if (!Players[index].Playing)
                {
                    continue;
                }
                AddNotes.AddRange(song.AddNotes);
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

            // Add a beatline note for every phrase (4 beats), unless its listed in the
            // RemoveNotes or SuperNotes collection.
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
                //Don't mark the starting BPM as a BPM change.
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
            foreach (Beatline beatline in _beatlines)
            {
                beatline.ClearNotes();
            }
        }

        /// <summary>
        /// Use some fancy calculations to determine how far the CPU player should be with an arrow sequence (from 0 to 1)
        /// for any specific moment in time.
        /// </summary>
        /// <param name="phraseNumber"></param>
        /// <returns></returns>
        public double GetPhraseDecimal(double phraseNumber)
        {
            int idx = -1;

            for (int x = 0; x < Players.Count(); x++)
            {
                if (!Players[x].IsCPUPlayer || !Players[x].Playing)
                {
                    continue;
                }
                idx = x;
                break;
            }
            if (idx == -1)
            {
                return 0.0;
            }
            //Are there any AddNotes in this song. They need to be considered as well.
            var addNotes = AddNotes.Where(e => e < phraseNumber).ToList();

            //Get the beatline note that was most recently passed, or the phrase number rounded down. Whichever is more recent.
            var lastNote = Math.Max(Math.Floor(phraseNumber), (addNotes.Any() ? addNotes.Max() : -1));

            //Get the nearest note not yet hit.
            var nextNote = NearestBeatlineNote(idx, phraseNumber);
            
            if (nextNote == null)
            {
                return 0.0;
            }

            var gap = nextNote.Position - lastNote;

            // Returns between 0 (at the last note) and 1 (at the next note).
           return (phraseNumber - lastNote) / gap;
        }
    }
}
