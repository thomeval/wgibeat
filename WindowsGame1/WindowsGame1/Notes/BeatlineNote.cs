using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame1.Notes
{
    public class BeatlineNote
    {
        public int Player { get; set; }
        public double Position { get; set; }
        public int DisplayPosition { get; set; }
        //Whether this BeatlineNote has been hit already.
        public bool Hit { get; set; }

        //The opacity of this BeatlineNote.
        public double DisplayUntil { get; set; }

        //TODO: Refactor or remove.
        public byte DisplayOpacity(double currentTime)
        {
            if (!Hit)
            {
                //Opacity is determined by how far over the impact line the BeatlineNote is.
                return 255;
            }
            else
            {
                //Opacity is determined by the amount of time until the DisplayUntil.
            }
            return 255;
        }
    }

    public enum BeatlineNoteJudgement
    {
        IDEAL,
        COOL,
        OK,
        BAD,
        FAIL
    }
}
