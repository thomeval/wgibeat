using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WGiBeat
{
    public class SettingsManager
    {
        private Dictionary<string, object> _settings;

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

        public static SettingsManager LoadFromFile(string filename)
        {
            SettingsManager sm = new SettingsManager();

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

                if (double.TryParse(value,out doubleAttempt))
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
            return sm;
        }

        private static bool IsDecimal(double number)
        {
            return (number != Math.Floor(number));
        }

        public void SaveToFile(string filename)
        {
            string outText = "#SETTINGS-1.0;";

            foreach (string rule in _settings.Keys)
            {
                outText += "\r\n" + rule + _settings[rule] + ";";
            }

            File.WriteAllText(filename,outText);
        }
    }
}
