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
        public static bool Contains<T>(this T[] array, T item)
        {
            return Array.IndexOf(array, item) != -1;
        }
        public static int IndexOf<T> (this T[] array, T item)
        {
            return Array.IndexOf(array, item);
        }

        public static bool ContainsAny<T>(this T[] array, params T[] items)
        {
            
            foreach (T item in items)
            {
                if (array.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }
    
    }

    public class ObjectEventArgs : EventArgs
    {
        public object Object { get; set; }
    }
}
