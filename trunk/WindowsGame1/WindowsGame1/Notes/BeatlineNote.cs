namespace WGiBeat.Notes
{
    public class BeatlineNote
    {
        public int Player { get; set; }
        public double Position { get; set; }
        public int DisplayPosition { get; set; }
        //Whether this BeatlineNote has been hit already.
        public bool Hit { get; set; }

    }

    public enum BeatlineNoteJudgement
    {
        IDEAL = 0,
        COOL = 1,
        OK = 2,
        BAD = 3,
        FAIL = 4,
        MISS = 5,
        COUNT = 6
    }
}
