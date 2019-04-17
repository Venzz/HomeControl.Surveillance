using System;

namespace OpenCv
{
    /// <summary>
    /// Proxy datatype for passing Mat's and vector&lt;&gt;'s as input parameters
    /// </summary>
    public class InputArray: DisposableCvObject
    {
        private object obj;

        public static implicit operator InputArray(Mat mat)
        {
            return Create(mat);
        }

        public static InputArray Create(Mat mat)
        {
            return new InputArray(mat);
        }

        internal InputArray(Mat mat)
        {
            if (mat == null)
                ptr = IntPtr.Zero;
            else
                ptr = NativeMethods.core_InputArray_new_byMat(mat.CvPtr);
            GC.KeepAlive(mat);
            obj = mat;
        }

        protected override void DisposeUnmanaged()
        {
            NativeMethods.core_InputArray_delete(ptr);
            base.DisposeUnmanaged();
        }
    }
}
