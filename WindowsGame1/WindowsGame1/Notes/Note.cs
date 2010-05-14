using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGame1.Drawing;

namespace WindowsGame1.Notes
{
    public class Note
    {

        public NoteDirection Direction;
        public bool Reverse;
        public bool Completed;

    }


    public enum NoteDirection
    {
        UP = 0,
        RIGHT = 1,
        DOWN = 2,
        LEFT = 3,
        COUNT = 4
    }
}
