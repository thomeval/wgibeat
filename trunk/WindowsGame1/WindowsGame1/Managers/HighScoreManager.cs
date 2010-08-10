﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using WGiBeat.AudioSystem;

namespace WGiBeat.Managers
{
    public class HighScoreManager
    {
        public GameSong CurrentSong { get; set; }

        private Dictionary<int, HighScoreEntry> _highScoreEntries = new Dictionary<int, HighScoreEntry>();

        public HighScoreEntry GetHighScoreEntry(int songHashCode)
        {
            if (!_highScoreEntries.ContainsKey(songHashCode))
            {
                return null;
            }
            return _highScoreEntries[songHashCode];
        }

        public void SetHighScoreEntry(int songHashCode, GameType gameType, long score, int grade, Difficulty difficulty)
        {
            if (!_highScoreEntries.ContainsKey(songHashCode))
            {
                _highScoreEntries[songHashCode] = new HighScoreEntry();
            }
            _highScoreEntries[songHashCode].Difficulties[gameType] = difficulty;
            _highScoreEntries[songHashCode].Grades[gameType] = grade;
            _highScoreEntries[songHashCode].Scores[gameType] = score;
        }


        private int DetermineHighScore(int songHashCode, Player[] players, GameType gameType)
        {
            var entry = GetHighScoreEntry(songHashCode);
            long highest;

            if ((entry == null) || (!entry.Scores.ContainsKey(gameType)))
            {
                highest = 0;
            }
            else
            {
                highest = entry.Scores[gameType];
            }

            int awardedPlayer = -1;
            switch (gameType)
            {
                case GameType.NORMAL:
                    for (int x = 0; x < 4; x++)
                    {
                        if ((players[x].Playing) && (players[x].Score > highest))
                        {

                            highest = Math.Max(highest, players[x].Score);
                            awardedPlayer = x;
                        }
                    }
                    break;
                case GameType.COOPERATIVE:
                    var currentTotal = (from e in players where e.Playing select e.Score).Sum();
                    if (currentTotal > highest)
                    {
                        awardedPlayer = 4;
                    }
                    break;
            }

            return awardedPlayer;

        }

        /// <summary>
        /// Updates the stored high score of the current song and game type, if the score achieved
        /// is higher.
        /// </summary>
        /// <param name="SongHashCode">The hashcode of the song played.</param>
        /// <param name="players">The players of the current game.</param>
        /// <param name="gameType">The GameType used in the current game.</param>
        /// <param name="grades">The GameType used in the current game.</param>
        /// <returns>-1 if no player beat the stored high score, 4 if all players as a team
        /// beat the high score, or the player index (0 to 3) if a single player beat the high score.
        public int UpdateHighScore(int SongHashCode, Player[] players, GameType gameType, int[] grades)
        {
            var result = DetermineHighScore(SongHashCode, players, gameType);

            switch (result)
            {
                case -1:
                    //No one beat the score.
                    break;
                case 4:
                    //Team score.
                    var total = (from e in players where e.Playing select e.Score).Sum();
                    SetHighScoreEntry(CurrentSong.GetHashCode(), gameType, total, grades[0], LowestDifficulty(players));
                    break;
                default:
                    //Individual high score.
                    var highest = (from e in players where e.Playing select e.Score).Max();
                    SetHighScoreEntry(CurrentSong.GetHashCode(), gameType, highest, grades[result], players[result].PlayDifficulty);
                    break;
            }
            return result;
        }

        private static Difficulty LowestDifficulty(Player[] players)
        {
            return (from e in players where e.Playing select e.PlayDifficulty).Min();
        }

        #region IO
        public void SaveToFile(string filename)
        {
            var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            var bf = new BinaryFormatter();
            bf.Serialize(fs, _highScoreEntries);

            fs.Close();
        }

        public static HighScoreManager LoadFromFile(string filename)
        {
            try
            {
                var result = new HighScoreManager();
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var bf = new BinaryFormatter();
                result._highScoreEntries = (Dictionary<int, HighScoreEntry>)bf.Deserialize(fs);
                fs.Close();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading high scores." + ex.Message);
                return new HighScoreManager();
            }
        }
        #endregion
    }
}
