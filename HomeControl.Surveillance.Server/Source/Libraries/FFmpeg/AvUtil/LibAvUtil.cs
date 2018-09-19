using System;
using System.Runtime.InteropServices;

namespace FFmpeg
{
    public static class LibAvUtil
    {
        /// <summary>
        /// Allocate an AVFrame and set its fields to default values. The resulting struct must be freed using av_frame_free(). Note, this only allocates the
        /// AVFrame itself, not the data buffers. Those must be allocated through other means, e.g. with av_frame_get_buffer() or manually.
        /// </summary>
        /// <returns>Returns a pointer to an AVFrame filled with default values or <c>IntPtr.Zero</c> on failure.</returns>
        [DllImport(Libraries.AvUtil)]
        public static extern IntPtr av_frame_alloc();

        /// <summary>
        /// Frees a memory block which has been allocated with av_malloc(z)() or av_realloc(). Note, it is recommended that you use av_freep() instead.
        /// </summary>
        /// <param name="ptr">The pointer to the memory block which should be freed. Note, <c>IntPtr.Zero</c> is explicitly allowed.</param>
        [DllImport(Libraries.AvUtil)]
        public static extern void av_free(IntPtr ptr);

        /// <summary>
        /// Allocate a block of size bytes with alignment suitable for all memory accesses (including vectors if available on the CPU).
        /// </summary>
        /// <param name="size">The size in bytes for the memory block to be allocated.</param>
        /// <returns>Returns a pointer to the allocated block, <c>IntPtr.Zero</c> if the block cannot be allocated.</returns>
        [DllImport(Libraries.AvUtil)]
        public static extern IntPtr av_malloc(UIntPtr size);

        [DllImport(Libraries.AvUtil)]
        public static extern void av_log_set_level(int logLevel);

        [DllImport(Libraries.AvUtil)]
        public static extern int av_strerror(int errnum, IntPtr errbuf, int errbuf_size);
    }
}