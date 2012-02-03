﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace WGiBeat.Managers
{
    /// <summary>
    /// Stores all current settings relevent to the game. These settings should be accessible from anywhere in the program, and
    /// can be saved to or loaded from file.
    /// </summary>
    public class SettingsManager : Manager
    {
        private readonly Dictionary<string, object> _settings;

        public SettingsManager()
        {
            _settings = new Dictionary<string, object>();
        }
        public object this[string id]
        {
            get
            {
                return _settings[id];
            }
            set
            {
                _settings[id] = value;
            }
        }

        public bool Exists(string id)
        {
            return _settings.ContainsKey(id);
        }

     
        public T Get<T>(string id)
        {
            return (T) _settings[id];
        }
        public void Set<T>(string id, T value)
        {
            _settings[id] = value;
        }

        public static SettingsManager LoadDefaults()
        {       
            var sm = new SettingsManager();

            sm["SongPreview"] = true;
            sm["SongVolume"] = 0.8;
            sm["SongFolder"] = "Songs";
            sm["FullScreen"] = false;
            sm["SongDebug"] = false;
            sm["LastSongPlayed"] = 0;
            sm["RunOnce"] = false;
            sm["ProfileFolder"] = "Profiles";
            sm["SongMD5Behaviour"] = 1;
            sm["SaveLog"] = true;
            sm["LogLevel"] = 2;
            sm["Theme"] = "Default";
            sm["LoaderOffsetAdjustment"] = 0.13;
            sm["AllowProblematicSongs"] = false;
            sm["ConvertToSNG"] = false;
            sm["CheckForUpdates"] = false;
            sm["ScreenResolution"] = "800x600";
            sm["AllowPDA"] = false;
            sm["EnableMenuSounds"] = true;
            sm["LastSortMode"] = 0;
            return sm;
        }

        public static SettingsManager LoadFromFile(string filename, LogManager log)
        {
            log.AddMessage("Loading default settings...", LogLevel.INFO);
            var sm = LoadDefaults();
            sm.Log = log;

            sm.Log.AddMessage("Loading saved settings from: " + filename + "...", LogLevel.INFO);

            if (!File.Exists(filename))
            {
                sm.Log.AddMessage("Could not load settings - file not found.", LogLevel.WARN);
                return sm;
            }

            string filetext = File.ReadAllText(filename);

            if (filetext.Substring(0, 9) != "#SETTINGS")
            {
                throw new FileLoadException("File requested is not a valid settings file.");
            }
            filetext = filetext.Replace("\n", "");
            filetext = filetext.Replace("\r", "");
            filetext = filetext.Substring(filetext.IndexOf(';') + 1);

            string[] rules = filetext.Split(';');

            foreach (string rule in rules)
            {
                if ((rule.Length < 1) || (rule[0] == '#'))
                    continue;

                string id = rule.Substring(0, rule.IndexOf('='));
                string value = rule.Substring(rule.IndexOf('=') + 1);
                double doubleAttempt;

                //Convert , to . for cultural friendliness.
                if (value.IndexOf(',') != -1 && value.IndexOf('.') == -1)
                {
                    value = value.Replace(',', '.');
                }

               
                bool boolAttempt;
                if (bool.TryParse(value, out boolAttempt))
                {
                    sm[id] = boolAttempt;
                }
                   
                else if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out doubleAttempt))
                {
                    if (IsDecimal(doubleAttempt))
                    {
                        sm[id] = doubleAttempt;
                    }
                    else
                    {
                        sm[id] = (int) doubleAttempt;
                    }
                }
                else
                {
                    sm[id] = value;     
                }

            }
            sm.Log.AddMessage("Settings loaded successfully.", LogLevel.INFO);
            return sm;
        }

        private static bool IsDecimal(double number)
        {
            return (number != Math.Floor(number));
        }

        public void SaveToFile(string filename)
        {
            string outText = "#SETTINGS-1.1;";

            foreach (string rule in _settings.Keys)
            {
                string ruleValue = _settings[rule] + "";
                if (_settings[rule] is double)
                {
                    ruleValue = ((double) _settings[rule]).ToString(CultureInfo.InvariantCulture);
                }
                outText += string.Format("\r\n{0}={1};",rule,ruleValue);
            }

            File.WriteAllText(filename,outText);
        }
    }
}