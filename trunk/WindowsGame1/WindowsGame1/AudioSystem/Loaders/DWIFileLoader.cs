using System;
using System.Collections.Generic;
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
        private double _stopTotals;

        public DWIFileLoader()
        {
            _notes = new string[_preferredNoteOrder.Length];
        }

        public override GameSong LoadFromFile(string filename)
        {
            Log.AddMessage("Converting DWI File: " + filename, LogLevel.NOTE);

            try
            {
                _stopTotals = 0.0;
                var newSong = new GameSong{ReadOnly = true};
                string songText = File.ReadAllText(filename);
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
                            newSong.Title = value;
                            SongManager.SplitTitle(newSong);
                            break;
                        case "#ARTIST":
                            newSong.Artist = value;
                            break;
                        case "#GAP":
                            newSong.Offset = Convert.ToDouble(value) / 1000.0;
                            break;
                        case "#BPM":
                            newSong.Bpm = Convert.ToDouble(value);
                            break;

                        case "#CHANGEBPM":
                            var bpmPairs = new Dictionary<double, double>();
                            var bpmText = value.Split(',');

                            foreach (string bpmItem in bpmText)
                            {
                                double position = Convert.ToDouble(bpmItem.Substring(0, bpmItem.IndexOf("=")));
                                double bvalue = Convert.ToDouble(bpmItem.Substring(bpmItem.IndexOf("=") + 1));
                                bpmPairs.Add(position, bvalue);
                            }
                            if (bpmPairs.Keys.Count > 1)
                            {
                                Log.AddMessage(filename + " has multiple BPMs and will not work correctly in WGiBeat! ", LogLevel.WARN);
                            }
                            
                            break;

                        case "#FILE":
                            newSong.AudioFile = value;
                            break;
                        case "#SINGLE":
                            AddNotes(value);
                            break;
                        case "#FREEZE":
                            if (String.IsNullOrEmpty(value))
                            {
                                continue;
                            }
                            var stopPairs = new Dictionary<double, double>();
                            var stopText = value.Split(',');
                            
                            foreach (string stopItem in stopText)
                            {
                                double position = Convert.ToDouble(stopItem.Substring(0, stopItem.IndexOf("=")));
                                double bvalue = Convert.ToDouble(stopItem.Substring(stopItem.IndexOf("=") + 1));
                                stopPairs.Add(position, bvalue);
                                _stopTotals += bvalue;
                            }
                            if (stopPairs.Keys.Count > 0)
                            {
                                Log.AddMessage(filename + " has Stops and will not work correctly in WGiBeat! ", LogLevel.WARN);
                            }

                            break;
                    }

                }

                var selectedNotes = (from e in _notes where e != null select e).First();

                newSong.Offset += OffsetAdjust;

                CalculateLength(newSong, selectedNotes);
                newSong.Length += _stopTotals;
                newSong.Length += OffsetAdjust;
                
                //Length calculation needs the ORIGINAL offset, so this must be done after it.
                AdjustOffset(newSong, selectedNotes);

                newSong.Path = Path.GetDirectoryName(filename);
                newSong.DefinitionFile = Path.GetFileName(filename);

                if ((String.IsNullOrEmpty(newSong.AudioFile) ))
                {
                    newSong.AudioFile = FindUndefinedAudioFile(newSong.Path, newSong.DefinitionFile);
                }
                newSong.SetMD5();

                return newSong;
            }
            catch (Exception ex)
            {
                Log.AddMessage("Failed to load DWI File." + ex.Message, LogLevel.WARN);
                return null;
            }

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
            Log.AddMessage(String.Format("Song notes start at phrase {0}. Offset set to {1}. ", startPhrase, song.Offset), LogLevel.DEBUG);
        }

        private void CalculateLength(GameSong song, string notes)
        {

            var endPhrase = ParseNoteString(notes, false);
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

    }
}
