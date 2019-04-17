using System;

namespace OpenCv
{
    public static class Cv2
    {
        /// <summary>
        /// Converts image from one color space to another
        /// </summary>
        /// <param name="src">The source image, 8-bit unsigned, 16-bit unsigned or single-precision floating-point</param>
        /// <param name="dst">The destination image; will have the same size and the same depth as src</param>
        /// <param name="code">The color space conversion code</param>
        /// <param name="dstCn">The number of channels in the destination image; if the parameter is 0, the number of the channels will be derived automatically from src and the code</param>
        public static void CvtColor(InputArray src, OutputArray dst, ColorConversionCodes code, int dstCn = 0)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            src.ThrowIfDisposed();
            dst.ThrowIfNotReady();

            NativeMethods.imgproc_cvtColor(src.CvPtr, dst.CvPtr, (int)code, dstCn);
            GC.KeepAlive(src);
            GC.KeepAlive(dst);
            dst.Fix();
        }

        /// <summary>
        /// computes element-wise absolute difference of two arrays (dst = abs(src1 - src2))
        /// </summary>
        /// <param name="src1"></param>
        /// <param name="src2"></param>
        /// <param name="dst"></param>
        public static void Absdiff(InputArray src1, InputArray src2, OutputArray dst)
        {
            if (src1 == null)
                throw new ArgumentNullException(nameof(src1));
            if (src2 == null)
                throw new ArgumentNullException(nameof(src2));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            src1.ThrowIfDisposed();
            src2.ThrowIfDisposed();
            dst.ThrowIfNotReady();
            NativeMethods.core_absdiff(src1.CvPtr, src2.CvPtr, dst.CvPtr);
            GC.KeepAlive(src1);
            GC.KeepAlive(src2);
            GC.KeepAlive(dst);
            dst.Fix();
        }

        /// <summary>
        /// Applies a fixed-level threshold to each array element.
        /// </summary>
        /// <param name="src">input array (single-channel, 8-bit or 32-bit floating point).</param>
        /// <param name="dst">output array of the same size and type as src.</param>
        /// <param name="thresh">threshold value.</param>
        /// <param name="maxval">maximum value to use with the THRESH_BINARY and THRESH_BINARY_INV thresholding types.</param>
        /// <param name="type">thresholding type (see the details below).</param>
        /// <returns>the computed threshold value when type == OTSU</returns>
        public static double Threshold(InputArray src, OutputArray dst, double thresh, double maxval, ThresholdTypes type)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            src.ThrowIfDisposed();
            dst.ThrowIfNotReady();
            double ret = NativeMethods.imgproc_threshold(src.CvPtr, dst.CvPtr, thresh, maxval, (int)type);
            GC.KeepAlive(src);
            GC.KeepAlive(dst);
            dst.Fix();
            return ret;
        }

        /// <summary>
        /// Dilates an image by using a specific structuring element.
        /// </summary>
        /// <param name="src">The source image</param>
        /// <param name="dst">The destination image. It will have the same size and the same type as src</param>
        /// <param name="element">The structuring element used for dilation. If element=new Mat() , a 3x3 rectangular structuring element is used</param>
        /// <param name="anchor">Position of the anchor within the element. The default value (-1, -1) means that the anchor is at the element center</param>
        /// <param name="iterations">The number of times dilation is applied. [By default this is 1]</param>
        /// <param name="borderType">The pixel extrapolation method. [By default this is BorderType.Constant]</param>
        /// <param name="borderValue">The border value in case of a constant border. The default value has a special meaning. [By default this is CvCpp.MorphologyDefaultBorderValue()]</param>
        public static void Dilate(InputArray src, OutputArray dst, InputArray element, Point? anchor = null, int iterations = 1, BorderTypes borderType = BorderTypes.Constant, Scalar? borderValue = null)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            src.ThrowIfDisposed();
            dst.ThrowIfNotReady();

            Point anchor0 = anchor.GetValueOrDefault(new Point(-1, -1));
            Scalar borderValue0 = borderValue.GetValueOrDefault(Scalar.All(Double.MaxValue));
            IntPtr elementPtr = element?.CvPtr ?? IntPtr.Zero;
            NativeMethods.imgproc_dilate(src.CvPtr, dst.CvPtr, elementPtr, anchor0, iterations, (int)borderType, borderValue0);
            GC.KeepAlive(src);
            GC.KeepAlive(dst);
            GC.KeepAlive(element);
            dst.Fix();
        }

        /// <summary>
        /// Finds contours in a binary image.
        /// </summary>
        /// <param name="image">Source, an 8-bit single-channel image. Non-zero pixels are treated as 1’s. 
        /// Zero pixels remain 0’s, so the image is treated as binary.
        /// The function modifies the image while extracting the contours.</param> 
        /// <param name="contours">Detected contours. Each contour is stored as a vector of points.</param>
        /// <param name="hierarchy">Optional output vector, containing information about the image topology. 
        /// It has as many elements as the number of contours. For each i-th contour contours[i], 
        /// the members of the elements hierarchy[i] are set to 0-based indices in contours of the next 
        /// and previous contours at the same hierarchical level, the first child contour and the parent contour, respectively. 
        /// If for the contour i there are no next, previous, parent, or nested contours, the corresponding elements of hierarchy[i] will be negative.</param>
        /// <param name="mode">Contour retrieval mode</param>
        /// <param name="method">Contour approximation method</param>
        /// <param name="offset"> Optional offset by which every contour point is shifted. 
        /// This is useful if the contours are extracted from the image ROI and then they should be analyzed in the whole image context.</param>
        public static void FindContours(InputOutputArray image, out Mat[] contours, OutputArray hierarchy, RetrievalModes mode, ContourApproximationModes method, Point? offset = null)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));
            image.ThrowIfNotReady();
            hierarchy.ThrowIfNotReady();

            Point offset0 = offset.GetValueOrDefault(new Point());
            IntPtr contoursPtr;
            NativeMethods.imgproc_findContours1_OutputArray(image.CvPtr, out contoursPtr, hierarchy.CvPtr, (int)mode, (int)method, offset0);

            using (var contoursVec = new VectorOfMat(contoursPtr))
            {
                contours = contoursVec.ToArray();
            }
            image.Fix();
            hierarchy.Fix();
            GC.KeepAlive(image);
            GC.KeepAlive(hierarchy);
        }

        /// <summary>
        /// Calculates the contour area
        /// </summary>
        /// <param name="contour">The contour vertices, represented by CV_32SC2 or CV_32FC2 matrix</param>
        /// <param name="oriented"></param>
        /// <returns></returns>
        public static double ContourArea(InputArray contour, bool oriented = false)
        {
            if (contour == null)
                throw new ArgumentNullException(nameof(contour));
            contour.ThrowIfDisposed();
            var ret = NativeMethods.imgproc_contourArea_InputArray(contour.CvPtr, oriented ? 1 : 0);
            GC.KeepAlive(contour);
            return ret;
        }
        
        /// <summary>
        /// Calculates all of the moments 
        /// up to the third order of a polygon or rasterized shape.
        /// </summary>
        /// <param name="array">A raster image (single-channel, 8-bit or floating-point 
        /// 2D array) or an array ( 1xN or Nx1 ) of 2D points ( Point or Point2f )</param>
        /// <param name="binaryImage">If it is true, then all the non-zero image pixels are treated as 1’s</param>
        /// <returns></returns>
        public static Moments Moments(InputArray array, bool binaryImage = false)
        {
            return new Moments(array, binaryImage);
        }
    }
}
