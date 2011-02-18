using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using WGiBeat.Managers;

namespace WGiBeat.Drawing
{
    public class MetricsManager : Manager
    {
        private readonly Dictionary<string, Vector2[]> _metrics;

        public MetricsManager()
        {
            _metrics = new Dictionary<string, Vector2[]>();
        }

        public void LoadFromFile(string filename)
        {
            Log.AddMessage("Loading Metrics from " + filename + "...", LogLevel.INFO);
            try
            {
                string filetext = File.ReadAllText(filename);
                LoadFromText(filetext);
            }
            catch (FileNotFoundException)
            {
                Log.AddMessage("Failed to load metrics due to file not found error: " + filename, LogLevel.WARN);
            }

            Log.AddMessage("Metrics loaded successfully.", LogLevel.INFO);
        }
        private void LoadFromText(string text)
        {

 
            if (text.Substring(0, 8) != "#METRICS")
            {
                throw new FileLoadException("File requested is not a valid metrics file.");
            }
            text = text.Replace("\n", "");
            text = text.Replace("\r", "");
            text = text.Substring(text.IndexOf(';') + 1);

            string[] rules = text.Split(';');

            foreach (string rule in rules)
            {
                
                if ((rule.Length < 1) || (rule[0] == '#'))
                    continue;

                string id = rule.Substring(0, rule.IndexOf('='));
                string[] svalues = rule.Substring(rule.IndexOf('=') + 1).Split(new[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);

                var values = new Vector2[svalues.Count() / 2];
                for (int i = 0; i < (svalues.Count() / 2); i++)
                {
                    int x = Convert.ToInt32(svalues[2 * i]);
                    int y = Convert.ToInt32(svalues[(2 * i) + 1]);
                    values[i] = new Vector2(x, y);

                    x = (int) (1.5 * x);
                    y = (int) (1.5*y);

                }
                this[id] = values;

                
            }

        }

        public Vector2 this[string id, int player]
        {
            get
            {
                try
                {
                    return _metrics[id][player];
                }
                catch (Exception)
                {
                    //TODO: Let this only happen once.
                    Log.AddMessage(String.Format("Metrics entry {0}[{1}] doesn't exist in the metrics file.",id,player),LogLevel.ERROR);
                }
                return new Vector2();
                
            }
            set
            {
                if (!_metrics.ContainsKey(id))
                {
                    _metrics[id] = new Vector2[4];
                }
                _metrics[id][player] = value;
            }

        }
        private Vector2[] this[string id]
        {
            get
            {
                return _metrics[id];
            }
            set
            {
                _metrics[id] = value;
            }
        }
    }
}
