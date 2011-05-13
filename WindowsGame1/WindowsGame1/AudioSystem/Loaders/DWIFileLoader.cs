using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogLevel = WGiBeat.Managers.LogLevel;

namespace WGiBeat.AudioSystem.Loaders
{
    /// <summary>
    /// A loader designed to load the .dwi file format into WGiBeat. This format is the preferred file format of the
    /// DDR based rhythm game, Dance With Intensity.
    /// </summary>
    public class DWIFileLoader : SongFileLoader
    {
        private readonly string[] _notes;
        private readonly string[] _preferredNoteOrder = {"MANIAC", "SMANIAC", "ANOTHER", "BASIC", "BEGINNER", "EDIT"};
        private GameSong _newSong;
        public DWIFileLoader()
        {
            _notes = new string[_preferredNoteOrder.Length];
        }

        public override GameSong LoadFromFile(string filename, out bool valid)
        {
            _newSong = new GameSong { ReadOnly = true };
            _newSong.BPMs = new Dictionary<double, double>();
            Log.AddMessage("Converting DWI File: " + filename, LogLevel.NOTE);

            try
            {
                  
                string songText = File.ReadAllText(filename);
                ParseText(songText);

                var selectedNotes = (from e in _notes where e != null select e).First();

                _newSong.Offset += OffsetAdjust;

                CalculateLength(_newSong, selectedNotes);
                _newSong.Length += OffsetAdjust;
                
                //Length calculation needs the ORIGINAL offset, so this must be done after it.
                AdjustOffset(_newSong, selectedNotes);

                _newSong.Path = Path.GetDirectoryName(filename);
                _newSong.DefinitionFile = Path.GetFileName(filename);

                if ((String.IsNullOrEmpty(_newSong.AudioFile) ))
                {
                    _newSong.AudioFile = FindUndefinedAudioFile(_newSong.Path, _newSong.DefinitionFile);
                }
                _newSong.SetMD5();

                if (ConvertToSNG)
                {
                    _newSong.ReadOnly = false;
                    SaveToFile(_newSong);
                }
                valid = true;
                
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                Log.AddMessage("Failed to load DWI File: " + ex.Message, LogLevel.WARN);
                valid = false;
            }

                return _newSong;
        }

        #region Helpers

        private void ParseText(string songText)
        {
            songText = songText.Replace("\r", "\n");
            songText = Regex.Replace(songText, "/{2}(.)*(\\n)+", "");
            songText = songText.Replace("\n", "");

            string[] rules = songText.Split(';');

            foreach (string rule in rules)
            {
                if (rule.IndexOf('#') < 0)
                {
                    continue;
                }
                var startpoint = rule.IndexOf("#");
                string field = rule.Substring(startpoint, rule.IndexOf(":") - startpoint).ToUpper();
                string value = rule.Substring(rule.IndexOf(":") + 1);

                switch (field.ToUpper())
                {
                    case "#TITLE":
                        _newSong.Title = value;
                        Log.AddMessage("Attempting to split title: " + value, LogLevel.DEBUG);
                        SongManager.SplitTitle(_newSong);
                        break;
                    case "#ARTIST":
                        _newSong.Artist = value;
                        break;
                    case "#GAP":
                        _newSong.Offset = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat) / 1000.0;
                        break;
                    case "#BPM":
                        _newSong.StartBPM = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                        break;

                    case "#CHANGEBPM":
                        ParseBPMs(value);
                        break;

                    case "#FILE":
                        _newSong.AudioFile = value;
                        break;
                    case "#SINGLE":
                        AddNotes(value);
                        break;
                    case "#FREEZE":
                        ParseStops(value);

                        break;
                }

            }
        }

        private void ParseStops(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return;
            }
            var stopPairs = new Dictionary<double, double>();
            var stopText = value.Split(',');

            foreach (string stopItem in stopText)
            {
                double position = Convert.ToDouble(stopItem.Substring(0, stopItem.IndexOf("=")), CultureInfo.InvariantCulture.NumberFormat);
                double bvalue = Convert.ToDouble(stopItem.Substring(stopItem.IndexOf("=") + 1), CultureInfo.InvariantCulture.NumberFormat);
                stopPairs[position/16.0] =  bvalue;
            }
        }

        private void ParseBPMs(string value)
        {
            var bpmPairs = new Dictionary<double, double>();
            var bpmText = value.Split(',');

            foreach (string bpmItem in bpmText)
            {
                double position = Convert.ToDouble(bpmItem.Substring(0, bpmItem.IndexOf("=")), CultureInfo.InvariantCulture.NumberFormat);
                double bvalue = Convert.ToDouble(bpmItem.Substring(bpmItem.IndexOf("=") + 1), CultureInfo.InvariantCulture.NumberFormat);
                bpmPairs[position / 16.0] = bvalue;
                if ((bvalue <= 0.0) && (!AllowProblematic))
                {
                    throw new NotSupportedException("This .dwi file uses negative BPMs and will not work correctly in WGiBeat!");
                }
            }
            bpmPairs[0.0] = _newSong.StartBPM;
            _newSong.BPMs = bpmPairs;

        }


        private void AddNotes(string value)
        {
            value = value.Replace(" ", "");
            var parts = value.Split(':');
            if (parts.Length < 3)
            {
                return;
            }

            //Add the note chart found to the dictionary (indexed by their difficulty).
            _notes[_preferredNoteOrder.IndexOf(parts[0])] = parts[2];
        }

        private void AdjustOffset(GameSong song, string notes)
        {
            var startPhrase = ParseNoteString(notes, true);
            song.Offset = song.ConvertPhraseToMS(startPhrase) / 1000.0;
            AdjustBpmChanges(song,startPhrase);
            Log.AddMessage(String.Format("Song notes start at phrase {0}. Offset set to {1}. ", startPhrase, song.Offset), LogLevel.DEBUG);
        }

        private void AdjustBpmChanges(GameSong song, int idx)
        {
            var newBPMs = new Dictionary<double, double>();

            foreach (double key in song.BPMs.Keys)
            {
                if (key == 0.0)
                {
                    newBPMs[key] = song.BPMs[key];
                }
                else
                {
                    newBPMs[key - idx] = song.BPMs[key];
                }
            }
            song.BPMs = newBPMs;

            var newStops = new Dictionary<double, double>();
            foreach (double key in song.Stops.Keys)
            {
                newStops[key - idx] = song.Stops[key];
            }
            song.Stops = newStops;
        }

        private void CalculateLength(GameSong song, string notes)
        {

            var endPhrase = ParseNoteString(notes, false);

            //TODO: Likely incorrect.
            song.Length = song.ConvertPhraseToMS(endPhrase + 0.5) / 1000.0;
            Log.AddMessage(String.Format("Song notes end at phrase {0}. Length set to {1}. ", endPhrase, song.Length), LogLevel.DEBUG);
        }

        public int ParseNoteString(string notes, bool start)
        {
            //Remove 'complex' notes into simpler ones.
            notes = Regex.Replace(notes, "<(.)+>", "1");

            var startPoint = 0.0;
            var phraseNumber = 0.0;
            var increment = 0.125;
            var skipNext = false;
            foreach (char note in notes)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                switch (note)
                {
                    case '!':
                        skipNext = true;
                        break;
                    case '(':
                        increment = 1.0/16;
                        break;
                    case '[':
                        increment = 1.0/24;
                        break;
                    case '{':
                        increment = 1.0/64;
                        break;
                    case '`':
                        increment = 1.0/192;
                        break;

                    case ')':
                    case ']':
                    case '}':
                    case '\'':                       
                        increment = 1.0 / 8;
                        break;
                        default:

                        if ((note != '0') && start)
                        {
                            startPoint = phraseNumber;
                            return (int)Math.Floor(startPoint);
                        }
                        phraseNumber += increment;
                        break;

                }             
            }

            return (int)Math.Floor(phraseNumber);
        }

        #endregion
    }
}
