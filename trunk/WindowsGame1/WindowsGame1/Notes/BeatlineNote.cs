﻿namespace WGiBeat.Notes
{
    /// <summary>
    /// A BeatlineNote represents a single note on a Beatline that must be hit in time with the beat of a song,
    /// after all arrows in the current NoteBar have been hit.
    /// </summary>
    public class BeatlineNote
    {
        public int Player { get; set; }
        public double Position { get; set; }
        public int DisplayPosition { get; set; }
        //Whether this BeatlineNote has been hit already.
        public bool Hit { get; set; }

    }

    /// <summary>
    /// Represents how accurately a BeatlineNote was hit. Use COUNT to get the number of judgements available.
    /// </summary>
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