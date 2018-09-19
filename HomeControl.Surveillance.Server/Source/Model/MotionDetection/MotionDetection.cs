using FFmpeg;
using HomeControl.Surveillance.Data;
using OpenCvSharp;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Model
{
    public class MotionDetection
    {
        private DataQueue Data = new DataQueue();
        private Boolean IsInitialized;
        private DateTime? DetectedDate;
        private DateTime BasePictureUpdatedDate;
        private Mat BasePicture = new Mat();

        public event TypedEventHandler<MotionDetection, Object> Detected = delegate { };
        public event TypedEventHandler<MotionDetection, (String CustomText, String Parameter)> LogReceived = delegate { };



        static MotionDetection()
        {
            LibAvFormat.av_register_all();
            LibAvUtil.av_log_set_level(-8);
        }

        public void Process(Byte[] data)
        {
            lock (this)
            {
                Data.Enqueue(data);
                Monitor.Pulse(this);
            }
        }

        public async void Start() => await Task.Run(async () =>
        {
            LibAvFormat.ReadPacket read = (opaque, buf, buf_size) =>
            {
                if (Data.Length > 0)
                {
                    var data = Data.Dequeue(Data.Length > buf_size ? buf_size : Data.Length);
                    Marshal.Copy(data, 0, buf, data.Length);
                    return data.Length;
                }
                else
                {
                    return 0;
                }
            };

            var dataReadingBuffer = LibAvUtil.av_malloc(new UIntPtr(10000U * sizeof(Byte)));
            var ioContextPointer = LibAvFormat.avio_alloc_context(dataReadingBuffer, 10000, 0, IntPtr.Zero, Marshal.GetFunctionPointerForDelegate(read), IntPtr.Zero, IntPtr.Zero);
            var contextPointer = LibAvFormat.avformat_alloc_context();
            var avFormatContext = Marshal.PtrToStructure<AvFormatContext>(contextPointer);
            avFormatContext.pb = ioContextPointer;
            Marshal.StructureToPtr(avFormatContext, contextPointer, false);

            var videoStreamId = -1;
            var videoStream = new AvStream();
            var videoCodecContext = new AvCodecContext();
            var picturePointer = LibAvUtil.av_frame_alloc();
            var picture = new AvFrame();
            var framePointer = LibAvUtil.av_frame_alloc();

            while (true)
            {
                var tooLowData = false;
                lock (this)
                {
                    tooLowData = Data.Length < 1 * 1000 * 1000;
                    if (tooLowData)
                        Monitor.Wait(this);
                }
                if (tooLowData)
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    continue;
                }

                lock (this)
                {
                    if (!IsInitialized)
                    {
                        var inputFormat = LibAvFormat.av_find_input_format("h264");
                        if (LibAvFormat.avformat_open_input(out contextPointer, "", inputFormat, IntPtr.Zero) < 0)
                        {
                            OnFailure("avformat_open_input failed.");
                            return;
                        }

                        var context = Marshal.PtrToStructure<AvFormatContext>(contextPointer);
                        if (LibAvFormat.avformat_find_stream_info(contextPointer, IntPtr.Zero) < 0)
                        {
                            OnFailure("An error occurred while retrieving the stream information of the video.");
                            return;
                        }

                        for (var i = 0; i < context.nb_streams; i++)
                        {
                            var stream = Marshal.PtrToStructure<AvStream>(Marshal.PtrToStructure<IntPtr>(IntPtr.Add(context.streams, i * IntPtr.Size)));
                            var codecContext = Marshal.PtrToStructure<AvCodecContext>(stream.codec);
                            if (codecContext.codec_type == AvMediaType.AVMEDIA_TYPE_VIDEO)
                            {
                                videoStreamId = i;
                                videoStream = stream;
                                videoCodecContext = codecContext;
                                break;
                            }
                        }
                        if (videoStreamId == -1)
                        {
                            OnFailure("No video stream found.");
                            return;
                        }

                        var videoCodecPointer = LibAvCodec.avcodec_find_decoder(videoCodecContext.codec_id);
                        if (videoCodecPointer == IntPtr.Zero)
                        {
                            OnFailure("The video codec is not supported.");
                            return;
                        }

                        var videoCodec = Marshal.PtrToStructure<AvCodec>(videoCodecPointer);
                        if (LibAvCodec.avcodec_open2(videoStream.codec, videoCodecPointer, IntPtr.Zero) < 0)
                        {
                            OnFailure($"The codec {videoCodec.long_name} could not be opened.");
                            return;
                        }
     
                        var pictureSize = (UInt32)LibAvCodec.avpicture_get_size(AvPixelFormat.AV_PIX_FMT_RGB24, videoCodecContext.width, videoCodecContext.height);
                        var pictureBufferPointer = LibAvUtil.av_malloc(new UIntPtr(pictureSize * sizeof(Byte)));
                        LibAvCodec.avpicture_fill(picturePointer, pictureBufferPointer, AvPixelFormat.AV_PIX_FMT_RGB24, videoCodecContext.width, videoCodecContext.height);
                        picture = Marshal.PtrToStructure<AvFrame>(picturePointer);

                        IsInitialized = true;
                    }
                    else
                    {
                        var packetPointer = Marshal.AllocHGlobal(Marshal.SizeOf<AvPacket>());
                        while (LibAvFormat.av_read_frame(contextPointer, packetPointer) >= 0)
                        {
                            var packet = Marshal.PtrToStructure<AvPacket>(packetPointer);
                            if (packet.stream_index == videoStreamId)
                            {
                                var frameFinished = 0;
                                LibAvCodec.avcodec_decode_video2(videoStream.codec, framePointer, ref frameFinished, packetPointer);
                                if (frameFinished != 0)
                                {
                                    var frame = Marshal.PtrToStructure<AvFrame>(framePointer);
                                    var scaleContextPointer = LibSwScale.sws_getContext(videoCodecContext.width, videoCodecContext.height, videoCodecContext.pix_fmt, videoCodecContext.width, videoCodecContext.height, AvPixelFormat.AV_PIX_FMT_RGB24, ScalingFlags.SWS_BILINEAR, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                                    LibSwScale.sws_scale(scaleContextPointer, frame.data, frame.linesize, 0, videoCodecContext.height, picture.data, picture.linesize);
                                    LibSwScale.sws_freeContext(scaleContextPointer);
                                    picture = Marshal.PtrToStructure<AvFrame>(picturePointer);

                                    var now = DateTime.Now;
                                    if (now - BasePictureUpdatedDate > TimeSpan.FromSeconds(1))
                                    {
                                        BasePictureUpdatedDate = now;
                                        Cv2.CvtColor(new Mat(frame.height, frame.width, MatType.CV_8UC3, picture.data[0]), BasePicture, ColorConversionCodes.RGB2GRAY);
                                    }
                                    else
                                    {
                                        var currentPicture = new Mat();
                                        var pictureDifference = new Mat();
                                        var pictureThreshold = new Mat();
                                        var pictureDilated = new Mat();
                                        var hierarchy = new Mat();

                                        Cv2.CvtColor(new Mat(frame.height, frame.width, MatType.CV_8UC3, picture.data[0]), currentPicture, ColorConversionCodes.RGB2GRAY);
                                        Cv2.Absdiff(BasePicture, currentPicture, pictureDifference);
                                        Cv2.Threshold(pictureDifference, pictureThreshold, 25, 255, ThresholdTypes.Binary);
                                        Cv2.Dilate(pictureThreshold, pictureDilated, new Mat(), null, 2);
                                        Cv2.FindContours(pictureDilated, out var contours, hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                                        foreach (var contour in contours)
                                        {
                                            var contourArea = Cv2.ContourArea(contour);
                                            if (contourArea > 500)
                                            {
                                                if (!DetectedDate.HasValue)
                                                {
                                                    Detected(this, null);
                                                }

                                                DetectedDate = now;
                                            }
                                        }
                                        if (DetectedDate.HasValue && (now - DetectedDate.Value > TimeSpan.FromSeconds(5)))
                                        {
                                            DetectedDate = null;
                                        }
                                    }
                                }
                            }
                            LibAvCodec.av_free_packet(packetPointer);
                        }
                    }
                }
            }
        });

        private void OnFailure(String message)
        {
            LogReceived(this, (message, null));
        }
    }
}
