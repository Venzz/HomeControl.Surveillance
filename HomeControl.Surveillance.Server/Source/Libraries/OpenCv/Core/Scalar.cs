using System;
using System.Runtime.InteropServices;

namespace OpenCv
{
    /// <summary>
    /// Template class for a 4-element vector derived from Vec.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Scalar
    {
        /// <summary>
        /// 
        /// </summary>
        public double Val0;

        /// <summary>
        /// 
        /// </summary>
        public double Val1;

        /// <summary>
        /// 
        /// </summary>
        public double Val2;

        /// <summary>
        /// 
        /// </summary>
        public double Val3;

        public Scalar(double v0, double v1, double v2, double v3)
        {
            Val0 = v0;
            Val1 = v1;
            Val2 = v2;
            Val3 = v3;
        }

        public static Scalar All(double v)
        {
            return new Scalar(v, v, v, v);
        }
    }
}
