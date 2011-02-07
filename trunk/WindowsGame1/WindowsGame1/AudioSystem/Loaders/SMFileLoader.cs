using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LogLevel = WGiBeat.Managers.LogLevel;

namespace WGiBeat.AudioSystem.Loaders
{
    public class SMFileLoader : SongFileLoader
    {
        private readonly string[] _notes;
        private readonly string[] _preferredNoteOrder = {"Hard", "Challenge", "Medium", "Easy", "Beginner", "Edit"};

        public SMFileLoader()
        {
            _notes = new string[_preferredNoteOrder.Length];
        }

        public override GameSong LoadFromFile(string filename)
        {
            Log.AddMessage("Converting SM File: " + filename, LogLevel.NOTE);

            try
            {
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
                            break;
                        case "#SUBTITLE":
                            newSong.Subtitle = value;
                            break;
                        case "#ARTIST":
                            newSong.Artist = value;
                            break;
                        case "#OFFSET":
                            newSong.Offset = -1 * Convert.ToDouble(value);
                            break;
                        case "#MUSICLENGTH":
                            //TODO: Some songs don't have this.
                            newSong.Length = Convert.ToDouble(value);
                            break;
                        case "#BPMS":
                            var bpmPairs = new Dictionary<double, double>();
                            var bpmText = value.Split(',');

                            foreach (string bpmItem in bpmText)
                            {
                                double position = Convert.ToDouble(bpmItem.Substring(0, bpmItem.IndexOf("=")));
                                double bvalue = Convert.ToDouble(bpmItem.Substring(bpmItem.IndexOf("=") + 1));
                                bpmPairs.Add(position,bvalue);
                            }
                            if (bpmPairs.Keys.Count > 1)
                            {
                             Log.AddMessage(filename + " has multiple BPMs and will not work correctly in WGiBeat! ", LogLevel.WARN);   
                            }
                            newSong.Bpm = bpmPairs[0.0];
                            break;
                        case "#MUSIC":
                            newSong.AudioFile = value;
                            break;
                        case "#NOTES":
                            AddNotes(value);
                            break;
                    }

                }

                var selectedNotes = (from e in _notes where e != null select e).First();


                var startPhrase = AdjustOffset(newSong, selectedNotes);
                newSong.Offset += OffsetAdjust;
                if (newSong.Length == 0)
                {
                    CalculateLength(newSong, selectedNotes, startPhrase);
                }
                newSong.Path = Path.GetDirectoryName(filename);
                newSong.DefinitionFile = Path.GetFileName(filename);

                return newSong;
            }
            catch (Exception ex)
            {
                Log.AddMessage("Failed to load SM File." + ex.Message, LogLevel.WARN);
                return null;
            }

        }

        private void AddNotes(string value)
        {
            value = value.Replace(" ", "");
            var parts = value.Split(':');
            //Only consider Single steps
            if ((parts.Length < 6) || (!parts[0].Equals("dance-single",StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            //Add the note chart found to the dictionary (indexed by their difficulty).
            _notes[_preferredNoteOrder.IndexOf(parts[2])] = parts[5];
        }

        private int AdjustOffset(GameSong song, string notes)
        {
            var lines = notes.Split(',');
            var idx = 0;
            for (int x = 0; x < lines.Length; x++)
            {
                //Extension method
                if (lines[x].ToCharArray().ContainsAny('1','2'))
                {
                    idx = x;
                    break;
                }
            }
            song.Offset = song.ConvertPhraseToMS(idx) / 1000.0;
            Log.AddMessage(String.Format("Song notes start at phrase {0}. Offset set to {1}. ",idx,song.Offset),LogLevel.DEBUG);
            return idx;
        }

        private void CalculateLength(GameSong song, string notes, double startPhrase)
        {
            var breaks = (from e in notes where e == ',' select e).Count();
            song.Length = song.ConvertPhraseToMS(breaks - startPhrase + 0.5) / 1000.0;
            Log.AddMessage(String.Format("Song notes end at phrase {0}. Length set to {1}. ", breaks, song.Length), LogLevel.DEBUG);
        }
    }
}