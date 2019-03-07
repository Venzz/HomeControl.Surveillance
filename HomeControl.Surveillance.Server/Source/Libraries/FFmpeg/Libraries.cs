using System;

namespace FFmpeg
{
    public static class Libraries
    {
        #if DEBUG
        public const String AvCodec = "Resources\\avcodec-57.dll";
        public const String AvFormat = "Resources\\avformat-57.dll";
        public const String AvUtil = "Resources\\avutil-55.dll";
        public const String SwScale = "Resources\\swscale-4.dll";
        #else
        public const String AvCodec = "/usr/lib/arm-linux-gnueabihf/libavcodec.so.57";
        public const String AvFormat = "/usr/lib/arm-linux-gnueabihf/libavformat.so.57";
        public const String AvUtil = "/usr/lib/arm-linux-gnueabihf/libavutil.so.55";
        public const String SwScale = "/usr/lib/arm-linux-gnueabihf/libswscale.so.4";
        #endif
    }
}