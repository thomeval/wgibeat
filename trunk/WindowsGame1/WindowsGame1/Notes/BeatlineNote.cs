namespace WGiBeat.Notes
{
    /// <summary>
    /// A BeatlineNote represents a single note on a Beatline that must be hit in time with the beat of a song,
    /// after all arrows in the current NoteBar have been hit.
    /// </summary>
    public class BeatlineNote
    {
        public bool CanBeHit {get { return NoteType == BeatlineNoteType.Normal || NoteType == BeatlineNoteType.Super; }}
        public double Opacity { get; set; }

        public double Position { get; set; }
        public int DisplayPosition { get; set; }
        //Whether this BeatlineNote has been hit already.
        public bool Hit { get; set; }
        public BeatlineNoteType NoteType {get; set;}
    }

    /// <summary>
    /// Represents how accurately a BeatlineNote was hit. Use COUNT to get the number of judgements available.
    /// </summary>
    public enum BeatlineNoteJudgement
    {
        Ideal = 0,
        Cool = 1,
        OK = 2,
        Bad = 3,
        Fail = 4,
        Miss = 5,
        Count = 6
    }

    /// <summary>
    /// Represents the type of Beatline note. Not all Beatline notes are designed to be hit - others are used as markers
    /// for significant events in the song, such as BPM changes, Stops, or the end of the song.
    /// </summary>
    public enum BeatlineNoteType
    {
        Normal = 0,
        EndOfSong = 1,
        BPMIncrease = 2,
        BPMDecrease = 3,
        Stop = 4,
        Super = 5
    }
}
