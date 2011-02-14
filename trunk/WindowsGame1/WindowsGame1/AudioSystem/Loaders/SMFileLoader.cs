using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LogLevel = WGiBeat.Managers.LogLevel;

namespace WGiBeat.AudioSystem.Loaders
{


    /// <summary>
    /// A loader designed to load the .sm file format into WGiBeat. This format is the preferred file format of the
    /// advanced open-source rhythm game, Stepmania.
    /// </summary>
    public class SMFileLoader : SongFileLoader
    {
        private readonly string[] _notes;
        private readonly string[] _preferredNoteOrder = {"Hard", "Challenge", "Medium", "Easy", "Beginner", "Edit"};
        private GameSong _newSong;
        private double _stopTotals;
        public SMFileLoader()
        {
            _notes = new string[_preferredNoteOrder.Length];
        }

        public override GameSong LoadFromFile(string filename)
        {
            Log.AddMessage("Converting SM File: " + filename, LogLevel.NOTE);

            try
            {
                _stopTotals = 0.0;
                _newSong = new GameSong{ReadOnly = true};
                string songText = File.ReadAllText(filename);
                ParseText(songText,filename);

                var selectedNotes = (from e in _notes where e != null select e).First();
                
                _newSong.Offset += OffsetAdjust;

                if (_newSong.Length == 0)
                {
                    CalculateLength(_newSong, selectedNotes);
                    _newSong.Length += _stopTotals;
                    _newSong.Length += OffsetAdjust;
                } 
                
                //Length calculation needs the ORIGINAL offset, so this must be done after it.
                AdjustOffset(_newSong, selectedNotes);

                _newSong.Path = Path.GetDirectoryName(filename);
                _newSong.DefinitionFile = Path.GetFileName(filename);
                _newSong.SetMD5();

                if (String.IsNullOrEmpty(_newSong.AudioFile))
                {
                    _newSong.AudioFile = FindUndefinedAudioFile(_newSong.Path, _newSong.DefinitionFile);
                }

                if (ConvertToSNG)
                {
                    _newSong.ReadOnly = false;
                    SaveToFile(_newSong);
                }
                return _newSong;
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                Log.AddMessage("Failed to load SM File." + ex.Message, LogLevel.WARN);
                return null;
            }

        }

        #region Helpers

        private void ParseText(string songText, string filename)
        {
            songText = songText.Replace("\r", "\n");
            //Remove comments from the file text.
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
                        break;
                    case "#SUBTITLE":
                        _newSong.Subtitle = value;
                        break;
                    case "#ARTIST":
                        _newSong.Artist = value;
                        break;
                    case "#OFFSET":
                        _newSong.Offset = -1 * Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "#MUSICLENGTH":
                        _newSong.Length = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "#BPMS":
                        ParseBPMs(value,filename);
                        break;
                    case "#MUSIC":
                        _newSong.AudioFile = value;
                        break;
                    case "#NOTES":
                        AddNotes(value);
                        break;
                    case "#STOPS":
                      ParseStops(value,filename);
                        break;
                }

            }

        }

        private void ParseStops(string value, string filename)
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
                stopPairs[position] =  bvalue;
                _stopTotals += bvalue;
            }
            if (stopPairs.Keys.Count > 0)
            {
                if (!AllowProblematic)
                    throw new Exception(filename + " has Stops and will not work correctly in WGiBeat!");
            }

        }

        private void ParseBPMs(string value, string filename)
        {
            var bpmPairs = new Dictionary<double, double>();
            var bpmText = value.Split(',');

            foreach (string bpmItem in bpmText)
            {
                double position = Convert.ToDouble(bpmItem.Substring(0, bpmItem.IndexOf("=")), CultureInfo.InvariantCulture.NumberFormat);
                double bvalue = Convert.ToDouble(bpmItem.Substring(bpmItem.IndexOf("=") + 1), CultureInfo.InvariantCulture.NumberFormat);
                bpmPairs[position] =  bvalue;
            }
            if (bpmPairs.Keys.Count > 1)
            {
                if (!AllowProblematic)
                    throw new Exception(filename + " has multiple BPMs and will not work correctly in WGiBeat!");
            }
            _newSong.Bpm = bpmPairs[0.0];
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

        private void AdjustOffset(GameSong song, string notes)
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
        }

        private void CalculateLength(GameSong song, string notes)
        {
            var lines = notes.Split(',');
            var idx = 0;
            for (int x = lines.Length -1; x >= 0; x--)
            {
                //Extension method
                if (lines[x].ToCharArray().ContainsAny('1', '2'))
                {
                    idx = x;
                    break;
                }
            }

            song.Length = song.ConvertPhraseToMS(idx + 0.5) / 1000.0;
            Log.AddMessage(String.Format("Song notes end at phrase {0}. Length set to {1}. ", idx, song.Length), LogLevel.DEBUG);
        }

        #endregion

    }
}