using System;

namespace HomeControl.Surveillance
{
    public static class Libraries
    {
        #if RASPBERRY
        public const String AvCodec = "/usr/lib/arm-linux-gnueabihf/libavcodec.so.57";
        public const String AvUtil = "/usr/lib/arm-linux-gnueabihf/libavutil.so.55";
        public const String SwScale = "/usr/lib/arm-linux-gnueabihf/libswscale.so.4";
        public const String Core = "OpenCvLibrary.so";
        public const String ImgProc = "OpenCvLibrary.so";
        #else
        public const String AvCodec = "Resources\\avcodec-571.dll";
        public const String AvUtil = "Resources\\avutil-55.dll";
        public const String SwScale = "Resources\\swscale-4.dll";
        public const String Core = "Resources/OpenCvLibrary.dll";
        public const String ImgProc = "Resources/OpenCvLibrary.dll";
        #endif
    }
}