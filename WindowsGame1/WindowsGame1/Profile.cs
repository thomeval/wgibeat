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
        }

        public void GetObjectData(SerializationInfo si, StreamingContext ctxt)
        {
            si.AddValue("Name", Name);
            si.AddValue("EXP", EXP);
            si.AddValue("JudgementCounts", JudgementCounts);
            si.AddValue("TotalHits", TotalHits);
            si.AddValue("LastDifficulty",LastDifficulty);
            si.AddValue("LastBeatlineSpeed",LastBeatlineSpeed);
        }

    }
}
