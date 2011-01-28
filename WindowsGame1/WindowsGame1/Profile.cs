using System;
using System.Runtime.Serialization;
using WGiBeat.Notes;

namespace WGiBeat
{
    [Serializable]
    public class Profile
    {

        public string Name { get; set; }
        public long EXP { get; set; }

        public long[] JudgementCounts { get; private set; }

        public long TotalHits { get; set; }

        public Difficulty LastDifficulty { get; set; }
        public double LastBeatlineSpeed { get; set; }
        public bool DisableKO { get; set; }

        public long SongsCleared { get; set; }
        public long SongsFailed { get; set; }

        public double TotalPlayTime { get; set; }

        public double AverageHitOffset { get; set; }
        public long HitOffsetCount { get; set; }

        public Profile()
        {
            JudgementCounts = new long[(int) BeatlineNoteJudgement.COUNT + 2];
        }

        public Profile(SerializationInfo si, StreamingContext sc)
        {
            Name = (string) si.GetValue("Name", typeof (string));
            EXP = (long) si.GetValue("EXP", typeof (long));
            JudgementCounts = (long[]) si.GetValue("JudgementCounts", typeof (long[]));
            TotalHits = (long) si.GetValue("TotalHits", typeof (long));
            LastDifficulty = (Difficulty) si.GetValue("LastDifficulty", typeof (Difficulty));
            LastBeatlineSpeed = (double) si.GetValue("LastBeatlineSpeed", typeof (double));
            SongsCleared = (long) si.GetValue("SongsCleared", typeof (long));
            SongsFailed = (long) si.GetValue("SongsFailed", typeof (long));
            try
            {
                TotalPlayTime = (double)si.GetValue("TotalPlayTime", typeof(double));
                AverageHitOffset = si.GetDouble("AverageHitOffset");
                HitOffsetCount = si.GetInt64("HitOffsetCount");
                DisableKO = si.GetBoolean("DisableKO");
            }
            catch (Exception)
            {

                throw;  
            }
 
        }

        public void GetObjectData(SerializationInfo si, StreamingContext sc)
        {
            si.AddValue("Name", Name);
            si.AddValue("EXP", EXP);
            si.AddValue("JudgementCounts", JudgementCounts);
            si.AddValue("TotalHits", TotalHits);
            si.AddValue("LastDifficulty",LastDifficulty);
            si.AddValue("LastBeatlineSpeed",LastBeatlineSpeed);
            si.AddValue("SongsCleared",SongsCleared);
            si.AddValue("SongsFailed",SongsFailed);
            si.AddValue("TotalPlayTime",TotalPlayTime);
            si.AddValue("AverageHitOffset",AverageHitOffset);
            si.AddValue("HitOffsetCount",HitOffsetCount);
            si.AddValue("DisableKO",DisableKO);
        }

    }
}
