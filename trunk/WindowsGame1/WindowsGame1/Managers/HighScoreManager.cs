using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using WGiBeat.AudioSystem;
using WGiBeat.Players;

namespace WGiBeat.Managers
{
    public class HighScoreManager : Manager
    {
        public GameSong CurrentSong { get; set; }

        private List <HighScoreEntry> _highScoreEntries = new List <HighScoreEntry>();

        public HighScoreEntry GetHighScoreEntry(int songHashCode, GameType gameType)
        {
            var highScoreEntry =
                (from e in _highScoreEntries where (e.SongID == songHashCode) && (e.GameType == gameType) select e).
                    SingleOrDefault();

            return highScoreEntry;
        }

        public void SetHighScoreEntry(int songHashCode, GameType gameType, long score, int grade, Difficulty difficulty, string name)
        {
            gameType = TranslateGameType(gameType);
            var highScoreEntry =
     (from e in _highScoreEntries where (e.SongID == songHashCode) && (e.GameType == gameType) select e).
         SingleOrDefault() ?? new HighScoreEntry { SongID = songHashCode, GameType = gameType };


            highScoreEntry.Difficulty = difficulty;
            highScoreEntry.Grade = grade;
            highScoreEntry.Score = score;
            highScoreEntry.Name = name;
            if (!_highScoreEntries.Contains(highScoreEntry))
            {
                _highScoreEntries.Add(highScoreEntry);
            }
        }

        private int DetermineHighScore(int songHashCode, Player[] players, GameType gameType)
        {
            gameType = TranslateGameType(gameType);
            var entry = GetHighScoreEntry(songHashCode,gameType);
            long highest;

            if (entry == null)
            {
                highest = 0;
            }
            else
            {
                highest = entry.Score;
            }

            int awardedPlayer = -1;
            long currentTotal;
            switch (gameType)
            {
                case GameType.NORMAL:
                    for (int x = 0; x < 4; x++)
                    {
                        if (players[x].IsHumanPlayer && (players[x].Score > highest))
                        {
                            highest = Math.Max(highest, players[x].Score);
                            awardedPlayer = x;
                        }
                    }
                    break;
                case GameType.COOPERATIVE:
                    currentTotal = (from e in players where e.Playing select e.Score).Sum();
                    if (currentTotal > highest)
                    {
                        awardedPlayer = 4;
                    }
                    break;
                    case GameType.SYNC_PRO:
                    case GameType.SYNC_PLUS:
                    currentTotal = (from e in players where e.Playing select e.Score).First();
                    if (currentTotal > highest)
                    {
                        awardedPlayer = 5;
                    }
                    break;
            }

            return awardedPlayer;
        }

        /// <summary>
        /// Updates the stored high score of the current song and game type, if the score achieved
        /// is higher.
        /// </summary>
        /// <param name="songHashCode">The hashcode of the song played.</param>
        /// <param name="players">The players of the current game.</param>
        /// <param name="gameType">The GameType used in the current game.</param>
        /// <param name="grades">The GameType used in the current game.</param>
        /// <returns>-1 if no player beat the stored high score, 4 if all players as a team
        /// beat the high score, or the player index (0 to 3) if a single player beat the high score.</returns>
        public int UpdateHighScore(int songHashCode, Player[] players, GameType gameType, int[] grades)
        {
            gameType = TranslateGameType(gameType);
            var result = DetermineHighScore(songHashCode, players, gameType);

            long totalScore;
            int playerCount;
            switch (result)
            {
                case -1:
                    //No one beat the score.
                    break;
                case 4:
                    //Team score. Add invididual player scores up.
                    totalScore = (from e in players where e.IsHumanPlayer select e.Score).Sum();
                    playerCount = (from e in players where e.IsHumanPlayer select e).Count();
                    SetHighScoreEntry(CurrentSong.GetHashCode(), gameType, totalScore, grades[0], LowestDifficulty(players), playerCount + " Players");
                    break;
                case 5:
                    //Sync score. Player scores are equal so use any one of them.
                    totalScore = (from e in players where e.IsHumanPlayer select e.Score).First();
                    playerCount = (from e in players where e.IsHumanPlayer select e).Count();
                    SetHighScoreEntry(CurrentSong.GetHashCode(), gameType, totalScore, grades[0], LowestDifficulty(players), playerCount + " Players");
                    break;
                default:
                    //Individual high score.
                    var highest = players[result].Score;
                    SetHighScoreEntry(CurrentSong.GetHashCode(), gameType, highest, grades[result], players[result].PlayerOptions.PlayDifficulty, players[result].SafeName);
                    break;
            }
            return result;
        }

        public static GameType TranslateGameType(GameType gameType)
        {
            switch (gameType)
            {
                case GameType.TEAM:
                case GameType.VS_CPU:
                    return GameType.NORMAL;
                    default:
                    return gameType;
            }
        }

        private static Difficulty LowestDifficulty(Player[] players)
        {
            return (from e in players where e.Playing select e.PlayerOptions.PlayDifficulty).Min();
        }

        #region IO
        public void SaveToFile(string filename)
        {
            var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            var bf = new BinaryFormatter();
            bf.Serialize(fs, _highScoreEntries);

            fs.Close();
        }

        public static HighScoreManager LoadFromFile(string filename, LogManager log)
        {
            try
            {
                log.AddMessage("Loading high scores from: " + filename + " ...",LogLevel.INFO);
                var result = new HighScoreManager {Log = log};
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var bf = new BinaryFormatter();
                result._highScoreEntries = (List<HighScoreEntry>)bf.Deserialize(fs);
                fs.Close();
                result.Log.AddMessage("High scores loaded successfully." + result._highScoreEntries.Count + " entries.", LogLevel.INFO);
                return result;
            }
            catch (Exception ex)
            {
                log.AddMessage("Error loading high scores." + ex.Message, LogLevel.WARN);
                return new HighScoreManager();
            }
        }

        public string PrintHighScores()
        {
            string result = "";
            int missingCount = 0, scoreCount = 0;
            foreach (HighScoreEntry entry in _highScoreEntries)
            {
                result+= (entry);
                var song = GameCore.Instance.Songs.GetBySongID(entry.SongID);
                if (song == null)
                {
                    result += "| MISSING";
                    missingCount++;
                }
                else
                {
                    result += "| " + song.Title;
                }
                scoreCount++;
                result += "\n";
            }
            result += string.Format("{0} scores, {1} are orphaned.", scoreCount, missingCount);
            return result;
        }

        public void CleanHighScores(SongManager songs)
        {
            int count = 0;
            foreach (var entry in _highScoreEntries.ToArray())
            {
                if (songs.GetBySongID(entry.SongID) == null)
                {
                    _highScoreEntries.Remove(entry);
                    count++;
                }     
            }
            Log.AddMessage(string.Format("Removed {0} stale high scores.",count), LogLevel.DEBUG);
        }

        #endregion
    }
}
