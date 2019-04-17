using System;
using System.Runtime.InteropServices;

namespace OpenCv
{
    public class NativeMethods
    {
        #if DEBUG
        public const String Core = "Resources/OpenCvLibrary.dll";
        public const String ImgProc = "Resources/OpenCvLibrary.dll";
        #else
        public const String Core = "OpenCvLibrary.so";
        public const String ImgProc = "OpenCvLibrary.so";
        #endif

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr core_Mat_new1();

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, BestFitMapping = false, ThrowOnUnmappableChar = true, ExactSpelling = true)]
        public static extern IntPtr imgcodecs_imread(string filename, int flags);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void core_Mat_delete(IntPtr mat);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr core_Mat_new8(int rows, int cols, int type, IntPtr data, IntPtr step);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr core_InputArray_new_byMat(IntPtr mat);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void core_InputArray_delete(IntPtr ia);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr core_OutputArray_new_byMat(IntPtr mat);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void core_OutputArray_delete(IntPtr oa);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr vector_Mat_getSize(IntPtr vector);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void vector_Mat_assignToArray(IntPtr vector, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] arr);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void vector_Mat_delete(IntPtr vector);

        [DllImport(ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern double imgproc_contourArea_InputArray(IntPtr contour, int oriented);

        [DllImport(ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void imgproc_findContours1_OutputArray(IntPtr image, out IntPtr contours, IntPtr hierarchy, int mode, int method, Point offset);

        [DllImport(ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void imgproc_dilate(IntPtr src, IntPtr dst, IntPtr kernel, Point anchor, int iterations, int borderType, Scalar borderValue);

        [DllImport(ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern double imgproc_threshold(IntPtr src, IntPtr dst, double thresh, double maxval, int type);

        [DllImport(Core, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void core_absdiff(IntPtr src1, IntPtr src2, IntPtr dst);

        [DllImport(ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void imgproc_cvtColor(IntPtr src, IntPtr dst, int code, int dstCn);

        [DllImport(ImgProc, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern Moments.NativeStruct imgproc_moments(IntPtr arr, int binaryImage);
    }
}
