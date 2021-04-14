using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControl.StoreRecordConverter.Controller
{
    public class ConverterController
    {
        private TaskScheduler UiTaskScheduler;

        public ObservableCollection<StoreRecordFile> Files { get; } = new ObservableCollection<StoreRecordFile>();



        public ConverterController()
        {
            UiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }


        public void Add(List<String> filePaths)
        {
            foreach (var filePath in filePaths)
                Files.Add(new StoreRecordFile(filePath));
        }

        public Task ConvertAsync(StoreRecordFile storeRecordFile) => Task.Run(async () =>
        {
            var tempFile = new FileInfo("temp.h264");
            tempFile.Delete();

            using (var fileStream = new FileStream(storeRecordFile.FilePath, FileMode.Open, FileAccess.Read))
            using (var fileReader = new BinaryReader(fileStream))
            {
                var mediaDescriptors = StoredRecordFile.ReadMediaDescriptors(fileStream);
                if (mediaDescriptors.Count == 0)
                    return;

                using (var tempFileWriter = new BinaryWriter(tempFile.OpenWrite()))
                {
                    var processedCount = 0.0;
                    var progressCheckpointCount = mediaDescriptors.Count / 10;
                    foreach (var mediaDescriptor in mediaDescriptors)
                    {
                        fileStream.Seek(mediaDescriptor.Offset, SeekOrigin.Begin);
                        var size = fileReader.ReadInt32();
                        var data = fileReader.ReadBytes(size);
                        tempFileWriter.Write(data);
                        processedCount++;

                        if (processedCount % progressCheckpointCount == 0)
                            await Task.Factory.StartNew(() => storeRecordFile.AddProgress(0.9m / 10), new CancellationToken(false), TaskCreationOptions.None, UiTaskScheduler);
                    }
                }

                var mp4File = $"{storeRecordFile.FileLocation}\\{storeRecordFile.FileName}.mp4";
                var process = new Process();
                process.StartInfo.FileName = $"FFmpeg\\ffmpeg.exe";
                process.StartInfo.Arguments = $"-y -i \"{tempFile.FullName}\" -c:v copy -f mp4 \"{mp4File}\"";
                process.Start();
                process.WaitForExit();
                process.Close();
                tempFile.Delete();
                await Task.Factory.StartNew(() => storeRecordFile.AddProgress(0.1m), new CancellationToken(), TaskCreationOptions.None, UiTaskScheduler);
            }
        });



        public class StoreRecordFile: INotifyPropertyChanged
        {
            public String FilePath { get; }
            public String FileName { get; }
            public String FileLocation { get; }
            public Decimal Progress { get; set; }

            public event PropertyChangedEventHandler PropertyChanged = delegate { };



            public StoreRecordFile(String filePath)
            {
                FilePath = filePath;
                FileName = Path.GetFileNameWithoutExtension(filePath);
                FileLocation = Path.GetDirectoryName(filePath);
            }

            public void AddProgress(Decimal value)
            {
                Progress += value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }
    }
}
