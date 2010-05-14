using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WGiBeat.AudioSystem
{
    [Serializable]
    public class HighScoreEntry
    {
        public HighScoreEntry()
        {
            Scores = new Dictionary<GameType, long>();
        }
        public Dictionary<GameType, long> Scores { get; set; }

        public int SongID { get; set; }
    }
}
