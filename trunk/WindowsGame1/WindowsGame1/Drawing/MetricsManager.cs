using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace WindowsGame1.Drawing
{
    public class MetricsManager
    {
        private Dictionary<string, Vector2[]> _metrics;

        public MetricsManager()
        {
            _metrics = new Dictionary<string, Vector2[]>();
        }

        public static MetricsManager Load(string filename)
        {
            var mm = new MetricsManager();


            string filetext = File.ReadAllText(filename);


            if (filetext.Substring(0, 8) != "#METRICS")
            {
                throw new FileLoadException("File requested is not a valid metrics file.");
            }
            filetext = filetext.Replace("\n", "");
            filetext = filetext.Replace("\r", "");
            filetext = filetext.Substring(filetext.IndexOf(';')+1);

            string[] rules = filetext.Split(';');

            foreach (string rule in rules)
            {
                if ((rule.Length < 1) || (rule[0] == '#'))
                    continue;

                string id = rule.Substring(0,rule.IndexOf('='));

                string[] svalues = rule.Substring(rule.IndexOf('=') + 1).Split(new[] {'[', ']', ','},StringSplitOptions.RemoveEmptyEntries);

                var values = new Vector2[svalues.Count()/2];
                for (int i = 0; i < (svalues.Count()/2); i ++)
                {
                    int X = Convert.ToInt32(svalues[2*i]);
                    int Y = Convert.ToInt32(svalues[(2*i) + 1]);
                    values[i] = new Vector2(X, Y);
                    
                }
                mm[id] = values;
                
            }
            return mm;
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
