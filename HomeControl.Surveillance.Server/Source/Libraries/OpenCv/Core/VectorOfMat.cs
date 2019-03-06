using System;

namespace OpenCv
{
    /// <summary>
    /// 
    /// </summary>
    public class VectorOfMat: DisposableCvObject
    {
        public int Size
        {
            get
            {
                var res = NativeMethods.vector_Mat_getSize(ptr).ToInt32();
                GC.KeepAlive(this);
                return res;
            }
        }

        public VectorOfMat(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        /// <summary>
        /// Releases unmanaged resources
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            NativeMethods.vector_Mat_delete(ptr);
            base.DisposeUnmanaged();
        }

        /// <summary>
        /// Converts std::vector to managed array
        /// </summary>
        /// <returns></returns>
        public Mat[] ToArray()
        {
            return ToArray<Mat>();
        }

        /// <summary>
        /// Converts std::vector to managed array
        /// </summary>
        /// <returns></returns>
        public T[] ToArray<T>() where T: Mat, new()
        {
            int size = Size;
            if (size == 0)
                return new T[0];

            T[] dst = new T[size];
            IntPtr[] dstPtr = new IntPtr[size];
            for (int i = 0; i < size; i++)
            {
                T m = new T();
                dst[i] = m;
                dstPtr[i] = m.CvPtr;
            }
            NativeMethods.vector_Mat_assignToArray(ptr, dstPtr);
            GC.KeepAlive(this);

            return dst;
        }
    }
}
