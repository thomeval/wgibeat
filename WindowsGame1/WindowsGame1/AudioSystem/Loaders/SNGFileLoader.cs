using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LogLevel = WGiBeat.Managers.LogLevel;

namespace WGiBeat.AudioSystem.Loaders
{
    public class SNGFileLoader : SongFileLoader 
    {
        public override GameSong LoadFromFile(string filename)
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
                            newSong.Offset = Convert.ToDouble(value);
                            break;
                        case "AUDIOSTART":
                            newSong.AudioStart = Convert.ToDouble(value);
                            break;
                        case "LENGTH":
                            newSong.Length = Convert.ToDouble(value);
                            break;
                        case "BPM":
                            newSong.Bpm = Convert.ToDouble(value);
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
                return newSong;
            }
            catch (Exception)
            {
                Log.AddMessage("Failed to load song: " + filename + "\n", LogLevel.WARN);
                return null;
            }
        }
    }
}
