using System;
using System.Collections.Generic;
using System.IO;

namespace WGiBeat.Managers
{
    public class TextManager : Manager
    {

        private Dictionary<string, string> _strings;

        public TextManager()
        {
            _strings = new Dictionary<string, string>();
        }

        public string this[string id]
        {
            get
            {
                if (!_strings.ContainsKey(id))
                {
                    return "TV says donuts are high in fat.";
                }
                return _strings[id]; 
            }
            set
            {
                if (!_strings.ContainsKey(id))
                {
                    _strings.Add(id, value);
                }
                else
                {
                    _strings[id] = value;
                }
               
            }
        }
        public static TextManager LoadFromFile(string filename, LogManager log)
        {
            log.AddMessage(String.Format("Initializing Text Manager"), LogLevel.INFO);
            var tm = new TextManager {Log = log};
            tm.AddResource(filename);
            return tm;
        }

        public void AddResource(string filename)
        {
            Log.AddMessage(String.Format("Loading Text resources from {0} ...", filename), LogLevel.INFO);

            try
            {
                string[] lines = File.ReadAllLines(filename);
                if (!lines[0].StartsWith("#TEXT"))
                {
                    throw new Exception("File specified is not a valid text resource file.");
                }

                foreach (string line in lines)
                {
                    if ((line.IndexOf("=") == -1) || (line.Length-1 == line.IndexOf("=")))
                    {
                        continue;
                    }

                    var key = line.Substring(0, line.IndexOf("="));
                    var value = line.Substring(line.IndexOf("=") + 1);
                    value = value.Replace("\\n", "\n");

                    this[key] = value;
                }

            }
            catch (Exception ex)
            {

                Log.AddMessage("Failed to load text resources from: " + ex.Message, LogLevel.ERROR);
            }
        }
    }
}
