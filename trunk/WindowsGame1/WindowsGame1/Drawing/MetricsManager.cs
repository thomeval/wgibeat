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
            Log.AddMessage("INFO: Loading Metrics from " + filename + "...");
            try
            {
                string filetext = File.ReadAllText(filename);
                LoadFromText(filetext);
            }
            catch (FileNotFoundException)
            {
                Log.AddMessage("WARN: Failed to load metrics due to file not found error: " + filename);
            }

            Log.AddMessage("INFO: Metrics loaded successfully.");
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

                }
                this[id] = values;

            }
        }
        public Vector2 this [string id, int player]
    {
            get
            {
                return _metrics[id][player];
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
