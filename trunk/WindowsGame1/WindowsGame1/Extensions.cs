using Microsoft.Xna.Framework;

namespace WGiBeat
{
    public static class Extensions
    {

        public static Vector2 Clone(this Vector2 vector)
        {
            var result = new Vector2();
            result.X = vector.X;
            result.Y = vector.Y;
            return result;
        }
    }
}
