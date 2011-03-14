﻿using System;
using System.Globalization;
using System.IO;
using LogLevel = WGiBeat.Managers.LogLevel;

namespace WGiBeat.AudioSystem.Loaders
{
    public class SNGFileLoader : SongFileLoader 
    {
        public override GameSong LoadFromFile(string filename, out bool valid)
        {
            var newSong = GameSong.LoadDefaults();

            newSong.Path = filename.Substring(0, filename.LastIndexOf("\\"));
            newSong.DefinitionFile = Path.GetFileName(filename);
            try
            {
                string songText = File.ReadAllText(filename);

                songText = songText.Replace("\r", "");
                songText = songText.Replace("\n", "");

                string[] rules = songText.Split(';');

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
                            newSong.Title = value;
                            break;
                        case "SUBTITLE":
                            newSong.Subtitle = value;
                            break;
                        case "ARTIST":
                            newSong.Artist = value;
                            break;
                        case "OFFSET":
                            newSong.Offset = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "AUDIOSTART":
                            newSong.AudioStart = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "LENGTH":
                            newSong.Length = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "BPM":
                            newSong.Bpm = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "SONGFILE":
                        case "AUDIOFILE":
                            newSong.AudioFile = value;
                            break;
                        case "SONGFILEMD5":
                        case "AUDIOFILEMD5":
                            newSong.AudioFileMD5 = value;
                            break;
                    }
                }

                if (String.IsNullOrEmpty(newSong.AudioFile))
                {
                    newSong.AudioFile = FindUndefinedAudioFile(newSong.Path, newSong.DefinitionFile);
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

            return newSong;
        }
    }
}