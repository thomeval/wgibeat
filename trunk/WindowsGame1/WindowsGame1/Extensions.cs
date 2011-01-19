using System;
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

        //Not a part of .NET??!
        public static bool Contains(this string[] array, string item)
        {
            return Array.IndexOf(array, item) != -1;
        }
    }
}
