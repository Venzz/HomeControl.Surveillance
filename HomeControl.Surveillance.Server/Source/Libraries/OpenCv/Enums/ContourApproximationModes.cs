using System;

namespace OpenCv
{
    /// <summary>
    /// Approximation method (for all the modes, except CV_RETR_RUNS, which uses built-in approximation). 
    /// </summary>
    public enum ContourApproximationModes: Int32
    {
        /// <summary>
        /// CHAIN_APPROX_NONE - translate all the points from the chain code into points; 
        /// </summary>
        ApproxNone = 1,

        /// <summary>
        /// CHAIN_APPROX_SIMPLE - compress horizontal, vertical, and diagonal segments, that is, the function leaves only their ending points; 
        /// </summary>
        ApproxSimple = 2,

        /// <summary>
        /// CHAIN_APPROX_TC89_L1 - apply one of the flavors of Teh-Chin chain approximation algorithm. 
        /// </summary>
        ApproxTC89L1 = 3,

        /// <summary>
        /// CHAIN_APPROX_TC89_KCOS - apply one of the flavors of Teh-Chin chain approximation algorithm. 
        /// </summary>
        ApproxTC89KCOS = 4,
    }
}
