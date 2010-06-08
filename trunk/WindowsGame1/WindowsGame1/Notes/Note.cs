using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Notes
{
    /// <summary>
    /// This represents a single note arrow that a player must press. An arrow can be pointing either left, right, up or down.
    /// It can also be reversed (which will cause it to be drawn differently).
    /// </summary>
    public class Note
    {

        public NoteDirection Direction;
        public bool Reverse;
        public bool Completed;

        public static NoteDirection ActionToDirection(Action action)
        {
            switch (action)
            {
                case Action.P1_LEFT:
                case Action.P2_LEFT:
                case Action.P3_LEFT:
                case Action.P4_LEFT:
                    return NoteDirection.LEFT;
                case Action.P1_RIGHT:
                case Action.P2_RIGHT:
                case Action.P3_RIGHT:
                case Action.P4_RIGHT:
                    return NoteDirection.RIGHT;
                case Action.P1_UP:
                case Action.P2_UP:
                case Action.P3_UP:
                case Action.P4_UP:
                    return NoteDirection.UP;
                case Action.P1_DOWN:
                case Action.P2_DOWN:
                case Action.P3_DOWN:
                case Action.P4_DOWN:
                    return NoteDirection.DOWN;
                default:
                    return NoteDirection.COUNT;
            }
        }
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
