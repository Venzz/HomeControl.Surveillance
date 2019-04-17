using System;

namespace OpenCv
{
    /// <summary>
    /// Proxy datatype for passing Mat's and List&lt;&gt;'s as output parameters
    /// </summary>
    public class OutputArray: DisposableCvObject
    {
        private readonly object obj;

        public static implicit operator OutputArray(Mat mat)
        {
            return new OutputArray(mat);
        }

        internal OutputArray(Mat mat)
        {
            if (mat == null)
                throw new ArgumentNullException(nameof(mat));
            ptr = NativeMethods.core_OutputArray_new_byMat(mat.CvPtr);
            GC.KeepAlive(mat);
            obj = mat;
        }

        /// <summary>
        /// Releases unmanaged resources
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            NativeMethods.core_OutputArray_delete(ptr);
            base.DisposeUnmanaged();
        }



        public bool IsMat()
        {
            return obj is Mat;
        }

        public bool IsReady()
        {
            return
                ptr != IntPtr.Zero &&
                !IsDisposed &&
                obj != null &&
                IsMat();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void ThrowIfNotReady()
        {
            if (!IsReady())
                throw new ArgumentException("Invalid OutputArray");
        }



        public void Fix()
        {
            AssignResult();
            Dispose();
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual void AssignResult()
        {
            if (!IsReady())
                throw new NotSupportedException();

            // OutputArrayの実体が cv::Mat のとき
            if (IsMat())
            {
                // 実は、何もしなくても結果入ってるっぽい？
                /*
                Mat mat = GetMat();
                // OutputArrayからMatオブジェクトを取得
                IntPtr outMat = NativeMethods.core_OutputArray_getMat(ptr);
                // ポインタをセット
                //NativeMethods.core_Mat_assignment_FromMat(mat.CvPtr, outMat);
                NativeMethods.core_Mat_assignTo(outMat, mat.CvPtr);
                // OutputArrayから取り出したMatをdelete
                NativeMethods.core_Mat_delete(outMat);
                */
            }
            else
            {
                throw new NotSupportedException("Not supported OutputArray-compatible type");
            }
        }
    }
}
