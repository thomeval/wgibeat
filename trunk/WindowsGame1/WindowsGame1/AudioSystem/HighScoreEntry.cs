using System;

namespace WGiBeat.AudioSystem
{
    /// <summary>
    /// Represents a set of high scores for a single song.
    /// A score, grade and difficulty is stored for each game type.
    /// </summary>
    [Serializable]
    public class HighScoreEntry
    {
        public long Score { get; set; }
        public int Grade { get; set; }
        public Difficulty Difficulty {get; set;}
        public string Name { get; set; }
        public int SongID { get; set; }
        public GameType GameType { get; set; }
    }
}
