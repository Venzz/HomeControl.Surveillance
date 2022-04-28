using HomeControl.Surveillance.Server.Model;
using HomeControl.Surveillance.Server.Services.OrientProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Venz.Telemetry;

namespace HomeControl.Surveillance.Server
{
    public class App
    {
        public static Diagnostics Diagnostics { get; } = new Diagnostics("Server");
        public static ApplicationModel Model { get; } = new ApplicationModel();

        public List<Int32> interframe = new List<Int32>();
        public List<Int32> predictionFrame = new List<Int32>();
        public List<Int32> audioFrame = new List<Int32>();
        //следующим кодим открытие файлов и так, чтобы не запускалась камера
        public App()
        {
            Diagnostics.Console.Add(new ConsoleTelemetryService());
            #if !DEBUG
            Diagnostics.File.Add(new FileTelemetryService());
#endif

#if DEBUG && !RASPBERRY
            var stream = new MemoryStream();
            using (var fileStream = new FileStream("I:\\2022-04-25_18.test", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                fileStream.CopyTo(stream);

            stream.Seek(0, SeekOrigin.Begin);
            var gg = new List<List<Byte>>();

            var signalCount = 8;
            using (var binaryReader = new BinaryReader(stream))
            {
                while (stream.Position != stream.Length)
                {
                    if (signalCount == 8)
                    {
                        var ls = gg.LastOrDefault();
                        if (ls != null)
                            ls.RemoveRange(ls.Count - 8, 8);


                        gg.Add(new List<byte>());
                        signalCount = 0;
                    }


                    var byt1 = binaryReader.ReadByte();
                    if (byt1 == 0xBB)
                        signalCount++;
                    else
                        signalCount = 0;
                    gg.Last().Add(byt1);
                }
            }

            var packages = new Dictionary<UInt32, List<String>>();
            for (var i = 0u; i < 12000; i++)
                packages.Add(i, new List<String>());


            var result = gg.Select(a => a.ToArray()).ToList();




            var session = new SessionProperties();

            for (var l = 0; l < result.Count; l++)
            {

                //if (l == 5910)
                //    System.Diagnostics.Debugger.Break();

                session.DataQueue.Enqueue(result[l]);

                //if (!dataQueue.Peek(4).SequenceEqual(new Byte[] { 0xFF, 0x01, 00, 00 }))
                //    System.Diagnostics.Debugger.Break();

                while (session.DataQueue.Length >= 20)
                {
                    var peekedData1 = session.DataQueue.Peek(20);
                    var dataSize1 = peekedData1[16] + peekedData1[17] * 256;
                    if (session.DataQueue.Length < dataSize1 + 20)
                        break;

                    var dataz = session.DataQueue.Dequeue(dataSize1 + 20);
                    var message = Message.Create(dataz);


                   // надо понять, а какой пакет-то отсутствут? чтобы точно знать, из-за меня он отсутствует или его реально не было, потому 
                    //например, получанная куча данных содержит 2 пакта сразу. А что если это мой кэш в 4096 байт повлиял на это?
                    //    может кеш увеличить?


  
                    packages[message.SequenceNumber].Add("1");




                    switch (message)
                    {
                        case AuthorizationResponseMessage authorizationResponse:
                            break;
                        case OpMonitorClaimResponseMessage claimResponse:
                            break;
                        case MediaDataResponseMessage mediaDataResponse:
                            var incomingStartsWithPackage = StartsWithFrame(mediaDataResponse.Data);
                            if (session.MediaDataQueue.Length > 0 && incomingStartsWithPackage)
                            {
                                // get package size, fill with zeros, create.
                                // corrupted data


                                ExtractFrames(session.MediaDataQueue, true);

                                session.MediaDataQueue.Clear();
                                session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
                            }
                            else if (session.MediaDataQueue.Length > 0 && !incomingStartsWithPackage)
                            {
                                var startsWithPackage = StartsWithFrame(session.MediaDataQueue.Peek(4));
                                if (startsWithPackage)
                                {
                                    session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
                                }
                                else
                                {
                                    // get package size, fill with zeros, create.
                                    // corrupted data
                                    ExtractFrames(session.MediaDataQueue, true);

                                    session.MediaDataQueue.Clear();
                                    break;
                                }
                            }
                            else if (session.MediaDataQueue.Length == 0 && incomingStartsWithPackage)
                            {
                                session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
                            }
                            else if (session.MediaDataQueue.Length == 0 && !incomingStartsWithPackage)
                            {
                                for (var i = 4; i < mediaDataResponse.Data.Length - 8; i++)
                                    if (mediaDataResponse.Data[i] == 0x00 && mediaDataResponse.Data[i + 1] == 0x00 && mediaDataResponse.Data[i + 2] == 0x01)
                                        if (mediaDataResponse.Data[i + 3] == 0xFA || mediaDataResponse.Data[i + 3] == 0xFC || mediaDataResponse.Data[i + 3] == 0xFD)
                                        {
                                            session.MediaDataQueue.Enqueue(mediaDataResponse.Data, i, mediaDataResponse.Data.Length - i);
                                            break;
                                        }
                                break;
                            }


                            ExtractFrames(session.MediaDataQueue, false);









                            ///пробовать следующую логику
                            //если приходит пакет, который начинается нормально (а что если он начинается не нормальн, но внутри есть нормальный?)
                            //а media queue ждет нехватающих байт, то событие corrupted data и мы составляем пакет из того, что есть (или заполняем нулями?)

                            //81920 - количество данных реальных
                            //82421 - количество данных ожидаемых

                            /*if ((session.LastSequenceNumber != 0) && (mediaDataResponse.SequenceNumber - session.LastSequenceNumber != 1))
                            {
                                session.SequenceNumberLost = true;
                                    session.MediaDataQueue.Clear();
                                //Log(this, ($"{nameof(OrientProtocolCameraConnection)}", $"Package Lost: {session.LastSequenceNumber + 1}"));
                            }
                            if (session.SequenceNumberLost)
                            {
                                var packageFound = false;
                                for (var i = 0; i < mediaDataResponse.Data.Length - 8; i++)
                                {
                                    if (mediaDataResponse.Data[i] == 0x00 && mediaDataResponse.Data[i + 1] == 0x00 && mediaDataResponse.Data[i + 2] == 0x01)
                                    {
                                        if (mediaDataResponse.Data[i + 3] == 0xFA || mediaDataResponse.Data[i + 3] == 0xFC || mediaDataResponse.Data[i + 3] == 0xFD)
                                        {
                                            session.MediaDataQueue.Enqueue(mediaDataResponse.Data, i, mediaDataResponse.Data.Length - i);
                                            session.SequenceNumberLost = false;
                                            packageFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (!packageFound)
                                {
                                    session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
                                }
                            }
                            else
                            {
                                session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
                            }*/

                            /*session.LastSequenceNumber = mediaDataResponse.SequenceNumber;
                            while (session.MediaDataQueue.Length >= 16)
                            {
                                var peekedData = session.MediaDataQueue.Peek(16);
                                var operationCode = peekedData[2] * 256 + peekedData[3];
                                var dataSize = 0;
                                var packageSize = 0;
                                switch (operationCode)
                                {
                                    case (UInt16)Message.Operation.AudioFrame:
                                        dataSize = BitConverter.ToInt16(peekedData, 6);
                                        packageSize = dataSize + 8;
                                        break;
                                    case (UInt16)Message.Operation.PredictionFrame:
                                        dataSize = BitConverter.ToInt32(peekedData, 4);
                                        packageSize = dataSize + 8;
                                        break;
                                    case (UInt16)Message.Operation.InterFrame:
                                        dataSize = BitConverter.ToInt32(peekedData, 12);
                                        packageSize = dataSize + 16;
                                        break;
                                    default:
                                        session.MediaDataQueue.Clear();
                                        break;
                                }

                                if (session.MediaDataQueue.Length < packageSize || packageSize == 0)
                                    break;

                                var now = DateTime.UtcNow;
                                switch (operationCode)
                                {
                                    case (UInt16)Message.Operation.AudioFrame:
                                        var duration = TimeSpan.FromMilliseconds(1000.0 / 50);
                                        session.MediaDataQueue.Dequeue(8);
                                        session.MediaDataQueue.Dequeue(dataSize);
                                        audioFrame.Add(dataSize);
                                        //MediaReceived(this, new AudioMediaData(mediaQueue.Dequeue(dataSize), now, duration));
                                        break;
                                    case (UInt16)Message.Operation.PredictionFrame:
                                        duration = TimeSpan.FromMilliseconds(1000.0 / 12.5);
                                        session.MediaDataQueue.Dequeue(8);
                                        session.MediaDataQueue.Dequeue(dataSize);
                                        predictionFrame.Add(dataSize);
                                        //MediaReceived(this, new PredictionFrameMediaData(mediaQueue.Dequeue(dataSize), now, duration));
                                        break;
                                    case (UInt16)Message.Operation.InterFrame:
                                        System.Diagnostics.Debug.WriteLine("" + session.MediaDataQueue.Peek(16).Skip(4).Take(7).ToArray().ToTestView());
                                        duration = TimeSpan.FromMilliseconds(1000.0 / 12.5);
                                        session.MediaDataQueue.Dequeue(16);
                                        session.MediaDataQueue.Dequeue(dataSize);
                                        interframe.Add(dataSize);
                                        //MediaReceived(this, new InterFrameMediaData(mediaQueue.Dequeue(dataSize), now, duration));
                                        break;
                                    default:
                                        var queuedData = session.MediaDataQueue.Peek(session.MediaDataQueue.Length);
                                        //DetailedLog(this, ($"{nameof(OrientProtocolCameraConnection)}", $"MediaDataCode = {operationCode}", $"Header = {mediaDataResponse.Header}\n{queuedData.ToHexView()}"));
                                        session.MediaDataQueue.Clear();
                                        break;
                                }
                            }*/
                            break;
                        case UnknownResponseMessage unknownResponse:
                            break;
                    }
                }
            }

            var size1 = audioFrame.Max();
            var size2 = interframe.Max();
            var size3 = predictionFrame.Max();
            System.Diagnostics.Debug.WriteLine($"audioFrame: {audioFrame.Count}, interframe: {interframe.Count}, predictionFrame: {predictionFrame.Count}");



            /*using (var fileStream = new FileStream($"I:\\from_raspberry.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var writer = new BinaryWriter(fileStream))
            {
                for (var i = 0; i < result.Count; i++)
                {
                    writer.Write(result[i].ToArray().ToHexView());
                    writer.Write("\n");
                }
            }*/
#endif
        }


        private class SessionProperties
        {
            public UInt32 Id { get; set; }
            public DataQueue DataQueue { get; }
            public DataQueue MediaDataQueue { get; }
            public UInt32 LastSequenceNumber { get; set; }
            public Boolean SequenceNumberLost { get; set; }

            public SessionProperties()
            {
                Id = 0;
                DataQueue = new DataQueue();
                MediaDataQueue = new DataQueue();
                LastSequenceNumber = 0;
            }
        }

        private Boolean StartsWithFrame(Byte[] data)
        {
            if (data.Length >= 4)
            {
                if (data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x01)
                {
                    if (data[3] == 0xFA || data[3] == 0xFC || data[3] == 0xFD)
                        return true;
                }
            }
            return false;
        }

        private void ExtractFrames(DataQueue mediaDataQueue, Boolean fillWithZeros)
        {
            while (mediaDataQueue.Length >= 16)
            {
                var peekedData = mediaDataQueue.Peek(16);
                var operationCode = peekedData[2] * 256 + peekedData[3];
                var dataSize = 0;
                var packageSize = 0;
                switch (operationCode)
                {
                    case (UInt16)Message.Operation.AudioFrame:
                        dataSize = BitConverter.ToInt16(peekedData, 6);
                        packageSize = dataSize + 8;
                        break;
                    case (UInt16)Message.Operation.PredictionFrame:
                        dataSize = BitConverter.ToInt32(peekedData, 4);
                        packageSize = dataSize + 8;
                        break;
                    case (UInt16)Message.Operation.InterFrame:
                        dataSize = BitConverter.ToInt32(peekedData, 12);
                        packageSize = dataSize + 16;
                        break;
                    default:
                        mediaDataQueue.Clear();
                        break;
                }

                if ((mediaDataQueue.Length < packageSize || packageSize == 0) && !fillWithZeros)
                    break;

                var now = DateTime.UtcNow;
                switch (operationCode)
                {
                    case (UInt16)Message.Operation.AudioFrame:
                        var duration = TimeSpan.FromMilliseconds(1000.0 / 50);
                        mediaDataQueue.Dequeue(8);
                        if (fillWithZeros && mediaDataQueue.Length < dataSize)
                        {
                            var remaining = dataSize - mediaDataQueue.Length;
                            mediaDataQueue.Dequeue(mediaDataQueue.Length);
   
                        }
                        else
                        {
                            mediaDataQueue.Dequeue(dataSize);
                        }
                        audioFrame.Add(dataSize);
                        //MediaReceived(this, new AudioMediaData(mediaQueue.Dequeue(dataSize), now, duration));
                        break;
                    case (UInt16)Message.Operation.PredictionFrame:
                        duration = TimeSpan.FromMilliseconds(1000.0 / 12.5);
                        mediaDataQueue.Dequeue(8);
                        if (fillWithZeros && mediaDataQueue.Length < dataSize)
                        {
                            var remaining = dataSize - mediaDataQueue.Length;
                            mediaDataQueue.Dequeue(mediaDataQueue.Length);

                        }
                        else
                        {
                            mediaDataQueue.Dequeue(dataSize);
                        }

                        predictionFrame.Add(dataSize);
                        //MediaReceived(this, new PredictionFrameMediaData(mediaQueue.Dequeue(dataSize), now, duration));
                        break;
                    case (UInt16)Message.Operation.InterFrame:
                        duration = TimeSpan.FromMilliseconds(1000.0 / 12.5);
                        mediaDataQueue.Dequeue(16);
                        if (fillWithZeros && mediaDataQueue.Length < dataSize)
                        {
                            var remaining = dataSize - mediaDataQueue.Length;
                            mediaDataQueue.Dequeue(mediaDataQueue.Length);

                        }
                        else
                        {
                            mediaDataQueue.Dequeue(dataSize);
                        }
                        interframe.Add(dataSize);
                        //MediaReceived(this, new InterFrameMediaData(mediaQueue.Dequeue(dataSize), now, duration));
                        break;
                    default:
                        var queuedData = mediaDataQueue.Peek(mediaDataQueue.Length);
                        //DetailedLog(this, ($"{nameof(OrientProtocolCameraConnection)}", $"MediaDataCode = {operationCode}", $"Header = {mediaDataResponse.Header}\n{queuedData.ToHexView()}"));
                        mediaDataQueue.Clear();
                        break;
                }
            }
        }

        private void EnqueueData(SessionProperties session, MediaDataResponseMessage mediaDataResponse)
        {
            if ((session.LastSequenceNumber != 0) && (mediaDataResponse.SequenceNumber - session.LastSequenceNumber != 1))
            {
                if (session.MediaDataQueue.Length > 0)
                    ;//Log(this, ($"{nameof(OrientProtocolCameraConnection)}", $"Package Lost: {session.LastSequenceNumber + 1}"));

                session.SequenceNumberLost = true;
                session.MediaDataQueue.Clear();
            }
            if (session.SequenceNumberLost)
            {
                var packageFound = false;
                for (var i = 0; i < mediaDataResponse.Data.Length - 16; i++)
                {
                    if (mediaDataResponse.Data[i] == 0x00 && mediaDataResponse.Data[i + 1] == 0x00 && mediaDataResponse.Data[i + 2] == 0x01)
                    {
                        var audioFrameFound = mediaDataResponse.Data[i + 3] == 0xFA && mediaDataResponse.Data[i + 6] == 0xA0 && mediaDataResponse.Data[i + 7] == 0x00; // 00 00 01 FA 0E 02 A0 00 D5 D5 D5 D5 55 D5 55 D5 55 D5 D5 55 55 55 55 D5 55 D5 D5 55 55 D5
                        var interFrameFound = mediaDataResponse.Data[i + 3] == 0xFC && mediaDataResponse.Data[i + 14] <= 0x0A && mediaDataResponse.Data[i + 15] == 0x00; // 00 00 01 FC 02 0D F0 87 9F 25 26 59 DD 04 01 00 00 00 00 01 67 42 00 2A 95 A8 1E 00 89 F9
                        var predictionFrameFound = mediaDataResponse.Data[i + 3] == 0xFD && mediaDataResponse.Data[i + 6] <= 0x02 && mediaDataResponse.Data[i + 7] == 0x00; // 00 00 01 FD BF 3E 00 00 00 00 00 01 61 E0 20 47 CD 09 FA B2 FA F1 32 0A DF E1 11 6C 9F ED
                        if (audioFrameFound || interFrameFound || predictionFrameFound)
                        {
                            session.MediaDataQueue.Enqueue(mediaDataResponse.Data, i, mediaDataResponse.Data.Length - i);
                            session.SequenceNumberLost = false;
                            packageFound = true;
                            break;
                        }
                    }
                }
                if (!packageFound)
                {
                    session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
                }
            }
            else
            {
                session.MediaDataQueue.Enqueue(mediaDataResponse.Data);
            }

            session.LastSequenceNumber = mediaDataResponse.SequenceNumber;
        }

        public async void Start() => await Task.Run(async () =>
        {
            await Model.InitializeAsync().ConfigureAwait(false);
        });

        public sealed class ConsoleTelemetryService: ITelemetryService
        {
            public void Start() => Console.WriteLine($"{GetTimestamp()} >> Application Launched");
            public void Finish() => Console.WriteLine($"{GetTimestamp()} >> Application Exit");
            public void LogEvent(String title) => Console.WriteLine($"{GetTimestamp()} >> {title}");
            public void LogEvent(String title, String parameter, String value) => Console.WriteLine($"{GetTimestamp()} >> {title} || {parameter}: {value}");
            public void LogException(String comment, Exception exception) => Console.WriteLine($"{GetTimestamp()} >> {comment} || {exception.GetType().FullName}: {exception.Message}");

            private String GetTimestamp() => DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
        }
    }
}
