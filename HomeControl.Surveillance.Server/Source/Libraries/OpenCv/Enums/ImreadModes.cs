﻿using System;

namespace OpenCv
{
    /// <summary>
    /// Specifies colorness and Depth of the loaded image
    /// </summary>
    [Flags]
    public enum ImreadModes: Int32
    {
        /// <summary>
        /// If set, return the loaded image as is (with alpha channel, otherwise it gets cropped).
        /// </summary>
        Unchanged = -1,

        /// <summary>
        /// If set, always convert image to the single channel grayscale image.
        /// </summary>
        GrayScale = 0,

        /// <summary>
        /// If set, always convert image to the 3 channel BGR color image.
        /// </summary>
        Color = 1,

        /// <summary>
        /// If set, return 16-bit/32-bit image when the input has the corresponding depth, otherwise convert it to 8-bit.
        /// </summary>
        AnyDepth = 2,

        /// <summary>
        /// If set, the image is read in any possible color format.
        /// </summary>
        AnyColor = 4,

        /// <summary>
        /// If set, use the gdal driver for loading the image.
        /// </summary>
        LoadGdal = 8,
    };
}
