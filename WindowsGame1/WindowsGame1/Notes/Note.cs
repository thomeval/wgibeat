using WGiBeat.Managers;

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

        public static NoteDirection ActionToDirection(InputAction inputAction)
        {
            switch (inputAction.Action)
            {
                case "LEFT":
                    return NoteDirection.LEFT;
                case "RIGHT":
                    return NoteDirection.RIGHT;
                case "UP":
                    return NoteDirection.UP;
                case "DOWN":
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
