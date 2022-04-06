using HomeControl.Surveillance;
using System;
using System.Runtime.InteropServices;

namespace OpenCv
{
    public class NativeMethods
    {
        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr core_Mat_new1();

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true, ExactSpelling = true)]
        public static extern IntPtr imgcodecs_imread(string filename, int flags);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void core_Mat_delete(IntPtr mat);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr core_Mat_new8(int rows, int cols, int type, IntPtr data, IntPtr step);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr core_InputArray_new_byMat(IntPtr mat);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void core_InputArray_delete(IntPtr ia);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr core_OutputArray_new_byMat(IntPtr mat);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void core_OutputArray_delete(IntPtr oa);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr vector_Mat_getSize(IntPtr vector);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void vector_Mat_assignToArray(IntPtr vector, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] arr);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void vector_Mat_delete(IntPtr vector);

        [DllImport(Libraries.ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern double imgproc_contourArea_InputArray(IntPtr contour, int oriented);

        [DllImport(Libraries.ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void imgproc_findContours1_OutputArray(IntPtr image, out IntPtr contours, IntPtr hierarchy, int mode, int method, Point offset);

        [DllImport(Libraries.ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void imgproc_dilate(IntPtr src, IntPtr dst, IntPtr kernel, Point anchor, int iterations, int borderType, Scalar borderValue);

        [DllImport(Libraries.ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern double imgproc_threshold(IntPtr src, IntPtr dst, double thresh, double maxval, int type);

        [DllImport(Libraries.Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void core_absdiff(IntPtr src1, IntPtr src2, IntPtr dst);

        [DllImport(Libraries.ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void imgproc_cvtColor(IntPtr src, IntPtr dst, int code, int dstCn);

        [DllImport(Libraries.ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern Moments.NativeStruct imgproc_moments(IntPtr arr, int binaryImage);
    }
}
