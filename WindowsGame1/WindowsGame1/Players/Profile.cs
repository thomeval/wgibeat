using System;
using System.Runtime.Serialization;
using WGiBeat.Notes;

namespace WGiBeat.Players
{
    [Serializable]
    public class Profile : ISerializable
    {

        public string Name { get; set; }
        public long EXP { get; set; }

        public long[] JudgementCounts { get; private set; }

        public long TotalHits { get; set; }

        public PlayerOptions LastPlayerOptions { get; set; }

        public long SongsCleared { get; set; }
        public long SongsFailed { get; set; }

        public double TotalPlayTime { get; set; }

        public double AverageHitOffset { get; set; }
        public long HitOffsetCount { get; set; }

        public static readonly long[] Levels = {
                                                   0, 50, 110, 180, 260, 360, 480, 640, 820, 1000, 
                                                   1200, 1400, 1600, 1820, 2040, 2270, 2500, 2800, 3100, 3400, 
                                                   3700,4000,4400,4800,5200,5600,6000,6500,7000,7750,
                                                   8500,9250,10000, 11000,12000,13000,14000,15500,17000,18500,
                                                   20000,21500,23000, 24500,26000,28000,30000,32000,34000,36000,
                                                   38000,40000,42500,45000,47500
                                               };

        public Profile()
        {
            JudgementCounts = new long[(int)BeatlineNoteJudgement.COUNT + 2];
            LastPlayerOptions = new PlayerOptions { BeatlineSpeed = 1.0, ScrollDirectionEast = true };
        }

        public static bool ProfileOutOfDate = false;
        public Profile(SerializationInfo si, StreamingContext sc)
        {
            LastPlayerOptions = new PlayerOptions();
            Name = (string)si.GetValue("Name", typeof(string));
            EXP = (long)si.GetValue("EXP", typeof(long));
            JudgementCounts = (long[])si.GetValue("JudgementCounts", typeof(long[]));
            TotalHits = (long)si.GetValue("TotalHits", typeof(long));
            LastPlayerOptions.PlayDifficulty = (Difficulty)si.GetValue("LastDifficulty", typeof(Difficulty));
            LastPlayerOptions.BeatlineSpeed = (double)si.GetValue("LastBeatlineSpeed", typeof(double));
            SongsCleared = (long)si.GetValue("SongsCleared", typeof(long));
            SongsFailed = (long)si.GetValue("SongsFailed", typeof(long));

            TotalPlayTime = (double)si.GetValue("TotalPlayTime", typeof(double));
            AverageHitOffset = si.GetDouble("AverageHitOffset");
            HitOffsetCount = si.GetInt64("HitOffsetCount");
            LastPlayerOptions.DisableKO = si.GetBoolean("DisableKO");

            try
            {
                LastPlayerOptions.ScrollDirectionEast = si.GetBoolean("ScrollDirectionEast");
                LastPlayerOptions.ScrollDirectionWest = si.GetBoolean("ScrollDirectionWest");
                ProfileOutOfDate = false;
            }
            catch (SerializationException)
            {
                ProfileOutOfDate = true;
            }

        }

        public void GetObjectData(SerializationInfo si, StreamingContext sc)
        {
            si.AddValue("Name", Name);
            si.AddValue("EXP", EXP);
            si.AddValue("JudgementCounts", JudgementCounts);
            si.AddValue("TotalHits", TotalHits);
            si.AddValue("LastDifficulty", LastPlayerOptions.PlayDifficulty);
            si.AddValue("LastBeatlineSpeed", LastPlayerOptions.BeatlineSpeed);
            si.AddValue("SongsCleared", SongsCleared);
            si.AddValue("SongsFailed", SongsFailed);
            si.AddValue("TotalPlayTime", TotalPlayTime);
            si.AddValue("AverageHitOffset", AverageHitOffset);
            si.AddValue("HitOffsetCount", HitOffsetCount);
            si.AddValue("DisableKO", LastPlayerOptions.DisableKO);
            si.AddValue("ScrollDirectionEast", LastPlayerOptions.ScrollDirectionEast);
            si.AddValue("ScrollDirectionWest", LastPlayerOptions.ScrollDirectionWest);
        }

        public int GetLevel()
        {
            for (int x = 0; x < Levels.Length; x++)
            {
                if (EXP < Levels[x])
                {
                    return x;
                }
            }
            return Levels.Length;
        }
    }
}