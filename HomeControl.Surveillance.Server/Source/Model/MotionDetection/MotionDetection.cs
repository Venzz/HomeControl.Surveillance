using FFmpeg;
using HomeControl.Surveillance.Server.Data;
using OpenCv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Model
{
    public class MotionDetection
    {
        private const AvPixelFormat PictureBufferPixelFormat = AvPixelFormat.AV_PIX_FMT_RGB24;
        private const UInt16 PictureBufferWidth = 320;
        private const UInt16 PictureBufferHeight = 180;

        private List<IMediaData> CapturedMedia = new List<IMediaData>();
        private DateTime BasePictureUpdatedDate;
        private Mat BasePicture = new Mat();
        private ContourAnalyzer ContourAnalyzer;

        public event TypedEventHandler<MotionDetection, Object> Detected = delegate { };
        public event TypedEventHandler<MotionDetection, (String CustomText, String Parameter)> LogReceived = delegate { };



        static MotionDetection()
        {
            LibAvCodec.avcodec_register_all();
            LibAvUtil.av_log_set_level(-8);
        }

        public MotionDetection()
        {
            var contourExclusions = new List<(Windows.Foundation.Point TopLeft, Windows.Foundation.Point BottomRight)>
            {
                (new Windows.Foundation.Point(240, 7), new Windows.Foundation.Point(320, 18)), // Time
                (new Windows.Foundation.Point(27, 169), new Windows.Foundation.Point(59, 180)), // Title
            };
            ContourAnalyzer = new ContourAnalyzer(contourExclusions);
            ContourAnalyzer.MotionStarted += MotionStarted;
            ContourAnalyzer.MotionFinished += MotionFinished;
        }

        public void Process(IMediaData mediaData)
        {
            if (mediaData.MediaDataType == MediaDataType.AudioFrame)
                return;

            lock (this)
            {
                CapturedMedia.Add(mediaData);
                Monitor.Pulse(this);
            }
        }

        public async void Start() => await Task.Run(() =>
        {
            var videoCodecPointer = LibAvCodec.avcodec_find_decoder(AvCodecId.AV_CODEC_ID_H264);
            if (videoCodecPointer == IntPtr.Zero)
            {
                OnFailure("The video codec is not supported.");
                return;
            }
            var codecContext = LibAvCodec.avcodec_alloc_context3(videoCodecPointer);
            if (LibAvCodec.avcodec_open2(codecContext, videoCodecPointer, IntPtr.Zero) < 0)
            {
                OnFailure($"The codec {AvCodecId.AV_CODEC_ID_H264} could not be opened.");
                return;
            }

            var picturePointer = LibAvUtil.av_frame_alloc();
            var pictureSize = (UInt32)LibAvCodec.avpicture_get_size(PictureBufferPixelFormat, PictureBufferWidth, PictureBufferWidth);
            var pictureBufferPointer = LibAvUtil.av_malloc(new UIntPtr(pictureSize * sizeof(Byte)));
            LibAvCodec.avpicture_fill(picturePointer, pictureBufferPointer, PictureBufferPixelFormat, PictureBufferWidth, PictureBufferWidth);
            var picture = Marshal.PtrToStructure<AvFrame>(picturePointer);

            while (true)
            {
                var mediaData = (Byte[])null;
                lock (this)
                {
                    if (CapturedMedia.Count == 0)
                        Monitor.Wait(this);

                    mediaData = CapturedMedia[0].Data;
                    CapturedMedia.RemoveAt(0);
                }

                var packetPointer = Marshal.AllocHGlobal(Marshal.SizeOf<AvPacket>());
                var packetData = Marshal.AllocHGlobal(mediaData.Length);
                Marshal.Copy(mediaData, 0, packetData, mediaData.Length);
                Marshal.StructureToPtr(new AvPacket() { data = packetData, size = mediaData.Length }, packetPointer, false);

                var frameFinished = 0;
                var framePointer = LibAvUtil.av_frame_alloc();
                LibAvCodec.avcodec_decode_video2(codecContext, framePointer, ref frameFinished, packetPointer);
                if (frameFinished != 0)
                {
                    var frame = Marshal.PtrToStructure<AvFrame>(framePointer);
                    var scaleContextPointer = LibSwScale.sws_getContext(frame.width, frame.height, (AvPixelFormat)frame.format, PictureBufferWidth, PictureBufferHeight, PictureBufferPixelFormat, ScalingFlags.SWS_BILINEAR, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    LibSwScale.sws_scale(scaleContextPointer, frame.data, frame.linesize, 0, frame.height, picture.data, picture.linesize);
                    LibSwScale.sws_freeContext(scaleContextPointer);
                    picture = Marshal.PtrToStructure<AvFrame>(picturePointer);

                    var now = DateTime.Now;
                    if (now - BasePictureUpdatedDate > TimeSpan.FromMilliseconds(1000))
                    {
                        lock (this)
                            CapturedMedia.Clear();

                        BasePictureUpdatedDate = now;
                        Cv2.CvtColor(new Mat(PictureBufferHeight, PictureBufferWidth, MatType.CV_8UC3, picture.data[0]), BasePicture, ColorConversionCodes.RGB2GRAY);
                    }
                    else
                    {
                        var currentPicture = new Mat();
                        var pictureDifference = new Mat();
                        var pictureThreshold = new Mat();
                        var pictureDilated = new Mat();
                        var hierarchy = new Mat();

                        Cv2.CvtColor(new Mat(PictureBufferHeight, PictureBufferWidth, MatType.CV_8UC3, picture.data[0]), currentPicture, ColorConversionCodes.RGB2GRAY);
                        Cv2.Absdiff(BasePicture, currentPicture, pictureDifference);
                        Cv2.Threshold(pictureDifference, pictureThreshold, 75, 255, ThresholdTypes.Binary);
                        Cv2.Dilate(pictureThreshold, pictureDilated, new Mat(), null, 2);
                        Cv2.FindContours(pictureDilated, out var contours, hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                        ContourAnalyzer.Process(contours);
                    }
                }

                Marshal.FreeHGlobal(packetData);
                LibAvCodec.av_free_packet(packetPointer);
            }
        });

        private void MotionStarted(ContourAnalyzer sender, IEnumerable<Windows.Foundation.Point> countourCenters)
        {
            Detected(this, null);
            LogReceived(this, ("Motion Started.", String.Join(", ", countourCenters.Select(a => $"{a.X:0}x{a.Y:0}"))));
        }

        private void MotionFinished(ContourAnalyzer sender, Object args)
        {
            LogReceived(this, ("Motion Finished.", null));
        }

        private void OnFailure(String message)
        {
            LogReceived(this, (message, null));
        }
    }
}
