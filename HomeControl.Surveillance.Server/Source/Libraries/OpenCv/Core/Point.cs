using System;
using System.Runtime.InteropServices;

namespace OpenCv
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        /// <summary>
        /// 
        /// </summary>
        public int X;

        /// <summary>
        /// 
        /// </summary>
        public int Y;

        /// <summary>
        /// 
        /// </summary>
        public const int SizeOf = sizeof (int) + sizeof (int);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point(double x, double y)
        {
            X = (int) x;
            Y = (int) y;
        }
    }
}