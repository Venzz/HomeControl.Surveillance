using System;
using System.IO;

namespace OpenCv
{
    /// <summary>
    /// OpenCV C++ n-dimensional dense array class (cv::Mat)
    /// </summary>
    public class Mat: DisposableCvObject
    {
        /// <summary>
        /// Creates empty Mat
        /// </summary>
        public Mat()
        {
            ptr = NativeMethods.core_Mat_new1();
        }

        /// <summary>
        /// constructor for matrix headers pointing to user-allocated data
        /// </summary>
        /// <param name="rows">Number of rows in a 2D array.</param>
        /// <param name="cols">Number of columns in a 2D array.</param>
        /// <param name="type">Array type. Use MatType.CV_8UC1, ..., CV_64FC4 to create 1-4 channel matrices, 
        /// or MatType. CV_8UC(n), ..., CV_64FC(n) to create multi-channel matrices.</param>
        /// <param name="data">Pointer to the user data. Matrix constructors that take data and step parameters do not allocate matrix data. 
        /// Instead, they just initialize the matrix header that points to the specified data, which means that no data is copied. 
        /// This operation is very efficient and can be used to process external data using OpenCV functions. 
        /// The external data is not automatically deallocated, so you should take care of it.</param>
        /// <param name="step">Number of bytes each matrix row occupies. The value should include the padding bytes at the end of each row, if any.
        /// If the parameter is missing (set to AUTO_STEP ), no padding is assumed and the actual step is calculated as cols*elemSize() .</param>
        public Mat(int rows, int cols, MatType type, IntPtr data, long step = 0)
        {
            ptr = NativeMethods.core_Mat_new8(rows, cols, type, data, new IntPtr(step));
        }

        /// <summary>
        /// Loads an image from a file. (cv::imread)
        /// </summary>
        /// <param name="fileName">Name of file to be loaded.</param>
        /// <param name="flags">Specifies color type of the loaded image</param>
        public Mat(string fileName, ImreadModes flags = ImreadModes.Color)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (!File.Exists(fileName))
                throw new FileNotFoundException("", fileName);

            ptr = NativeMethods.imgcodecs_imread(fileName, (int)flags);
        }

        /// <summary>
        /// Releases the resources
        /// </summary>
        public void Release()
        {
            Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        /// Releases unmanaged resources
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            if (ptr != IntPtr.Zero)
                NativeMethods.core_Mat_delete(ptr);
            base.DisposeUnmanaged();
        }
    }
}
