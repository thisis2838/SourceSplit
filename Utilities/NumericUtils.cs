using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class NumericUtils
    {
        public static Vector3f Abs(this Vector3f a)
        {
            Vector3f b = new Vector3f();
            b.X = Math.Abs(a.X);
            b.Y = Math.Abs(a.Y);
            b.Z = Math.Abs(a.Z);
            return b;
        }

        public static bool InsideBox(
            this Vector3f a,
            float x1, float x2,
            float y1, float y2,
            float z1, float z2)
        {
            return a.X.Inside(x1, x2) && a.Y.Inside(y1, y2) && a.Z.Inside(z1, z2);
        }

        public static bool Inside(this float a, float b, float c)
        {
            return b > c ? (a > c && b > a) : (a > b && c > a);
        }

        public static T Bounded<T>(this T val, T min, T max) where T : struct, IComparable
        {
            return val.CompareTo(min) < 0 ? min : (val.CompareTo(max) > 0 ? max : val);
        }
    }
}
