using System;
using System.Collections.Generic;

namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// Represents a set of high scores for a single song.
    /// A score, grade and difficulty is stored for each game type.
    /// </summary>
    [Serializable]
    public class HighScoreEntry
    {
       
        public HighScoreEntry()
        {
            Scores = new Dictionary<GameType, long>();
            Grades = new Dictionary<GameType, int>();
            Difficulties = new Dictionary<GameType, Difficulty>();
        }
        public Dictionary<GameType, long> Scores { get; set; }
        public Dictionary<GameType, int> Grades { get; set; }
        public Dictionary<GameType, Difficulty> Difficulties { get; set;}

        public int SongID { get; set; }
    }
}
