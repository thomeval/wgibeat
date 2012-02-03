using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using LogLevel = WGiBeat.Managers.LogLevel;

namespace WGiBeat.AudioSystem.Loaders
{
    public class SNGFileLoader : SongFileLoader
    {
        private GameSong _newSong;
        public override GameSong LoadFromFile(string filename, out bool valid)
        {
            
            _newSong = GameSong.LoadDefaults();
            _newSong.Path = filename.Substring(0, filename.LastIndexOf("\\"));
            _newSong.DefinitionFile = Path.GetFileName(filename);
            try
            {
                string songText = File.ReadAllText(filename);

                songText = songText.Replace("\r", "");
                songText = songText.Replace("\n", "");

                string[] rules = songText.Split(';');

                if (!rules[0].StartsWith("#SONG"))
                {
                    throw new Exception("Song is not a valid song file. It must start with #SONG- followed by the version number.");
                }
                var songVersion = rules[0].Substring(rules[0].IndexOf("-") + 1);

                foreach (string rule in rules)
                {
                    if ((rule.Length == 0) || (rule[0] == '#'))
                    {
                        continue;
                    }

                    string field = rule.Substring(0, rule.IndexOf("=")).ToUpper();
                    string value = rule.Substring(rule.IndexOf("=") + 1);

                    switch (field.ToUpper())
                    {
                        case "TITLE":
                            _newSong.Title = value;
                            break;
                        case "SUBTITLE":
                            _newSong.Subtitle = value;
                            break;
                        case "ARTIST":
                            _newSong.Artist = value;
                            break;
                        case "OFFSET":
                            _newSong.Offset = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "AUDIOSTART":
                            _newSong.AudioStart = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "LENGTH":
                            _newSong.Length = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "BPM":
                            switch (songVersion)
                            {
                                case "1.0":
                                    _newSong.StartBPM = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                                    break;
                                case "1.1":
                                    ParseBPMs(value);
                                    break;
                            }
                            break;
                        case "SONGFILE":
                        case "AUDIOFILE":
                            _newSong.AudioFile = value;
                            break;
                        case "SONGFILEMD5":
                        case "AUDIOFILEMD5":
                            _newSong.AudioFileMD5 = value;
                            break;
                        case "STOPS":
                            ParseStops(value);
                            break;
                        case "BACKGROUND":
                            _newSong.BackgroundFile = value;
                            break;
                    }
                }

                if (String.IsNullOrEmpty(_newSong.AudioFile))
                {
                    _newSong.AudioFile = FindUndefinedAudioFile(_newSong.Path, _newSong.DefinitionFile);
                }
                valid = true;
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                LastLoadError = ex.Message;
                Log.AddMessage("Failed to load song: " + ex.Message + " File: " + filename, LogLevel.WARN);
                valid = false;
            }

            return _newSong;
        }

        private void ParseBPMs(string value)
        {
            //Example: BPM=0.0:120.0,5.0:150.0,8.5:185.0
            var result = new Dictionary<double, double>();
            try
            {

                string[] pairs = value.Split(',');

                foreach (string bpmPair in pairs)
                {
                    var pieces = bpmPair.Split(':');
                    var bpmKey = Convert.ToDouble(pieces[0], CultureInfo.InvariantCulture.NumberFormat);
                    var bpmValue = Convert.ToDouble(pieces[1], CultureInfo.InvariantCulture.NumberFormat);
                    result.Add(bpmKey,bpmValue);
                }

                _newSong.BPMs = result;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load song's BPM. ", ex);
            }
        }

        private void ParseStops(string value)
        {
            //Example: STOPS=0.5:2.0,5.0:0.025,8.5:0.375
            var result = new Dictionary<double, double>();
            try
            {

                string[] pairs = value.Split(',');

                foreach (string stopPair in pairs)
                {
                    var pieces = stopPair.Split(':');
                    var stopKey = Convert.ToDouble(pieces[0], CultureInfo.InvariantCulture.NumberFormat);
                    var stopValue = Convert.ToDouble(pieces[1], CultureInfo.InvariantCulture.NumberFormat);
                    result.Add(stopKey, stopValue);
                }

                _newSong.Stops = result;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load song's Stops. ", ex);
            }
        }
    }
}
