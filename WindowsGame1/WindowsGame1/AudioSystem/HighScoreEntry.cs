using System;
using WGiBeat.Players;

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

        public override string ToString()
        {
            return string.Format("{0:20} | {1:15} | {2:15} | {3:15} | {4:2} | {5:15} ", Score.ToString(), SongID.ToString(), GameType.ToString() ,Name.ToString(), Grade.ToString(), Difficulty.ToString());   
        }
    }
}
