using System;

namespace OpenCv
{
    /// <summary>
    /// mode of the contour retrieval algorithm
    /// </summary>
    public enum RetrievalModes: Int32
    {
        /// <summary>
        /// retrieves only the extreme outer contours. 
        /// It sets `hierarchy[i][2]=hierarchy[i][3]=-1` for all the contours.
        /// </summary>
        External = 0,
        
        /// <summary>
        /// retrieves all of the contours without establishing any hierarchical relationships.
        /// </summary>
        List = 1,

        /// <summary>
        /// retrieves all of the contours and organizes them into a two-level hierarchy. 
        /// At the top level, there are external boundaries of the components. 
        /// At the second level, there are boundaries of the holes. If there is another 
        /// contour inside a hole of a connected component, it is still put at the top level.
        /// </summary>
        CComp = 2,

        /// <summary>
        /// retrieves all of the contours and reconstructs a full hierarchy 
        /// of nested contours.
        /// </summary>
        Tree = 3,

        FloodFill = 4,
    }
}
