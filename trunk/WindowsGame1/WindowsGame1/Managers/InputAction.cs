using System;

namespace WGiBeat.Managers
{
    [Serializable]
    public class InputAction
    {
        public int Player { get; set; }
        public string Action { get; set; }

        public bool Equals(InputAction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Player == Player && Equals(other.Action, Action);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(InputAction)) return false;
            return Equals((InputAction)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Player * 397) ^ (Action != null ? Action.GetHashCode() : 0);
            }
        }

    }
}
