using HomeControl.Surveillance.Data.Storage;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace HomeControl.StoreRecordConverter
{
    public partial class ConverterWindow: Window
    {
        public ConverterWindow()
        {
            InitializeComponent();
            var executablePath = Assembly.GetExecutingAssembly().Location;
            var workingDirectory = Path.GetDirectoryName(executablePath);
            Directory.SetCurrentDirectory(workingDirectory);
        }

        private async void OnDrop(Object sender, DragEventArgs args)
        {
            if (!args.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (String[])args.Data.GetData(DataFormats.FileDrop);
            await Task.Run(() =>
            {
                foreach (var file in files)
                {
                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    using (var fileReader = new BinaryReader(fileStream))
                    {
                        var mediaDescriptors = StoredRecordFile.ReadMediaDescriptors(fileStream);
                        if (mediaDescriptors.Count == 0)
                            continue;

                        var tempFile = new FileInfo("temp.h264");
                        using (var tempFileWriter = new BinaryWriter(tempFile.OpenWrite()))
                        {
                            foreach (var mediaDescriptor in mediaDescriptors)
                            {
                                fileStream.Seek(mediaDescriptor.Offset, SeekOrigin.Begin);
                                var size = fileReader.ReadInt32();
                                var data = fileReader.ReadBytes(size);
                                tempFileWriter.Write(data);
                            }
                        }

                        var mp4File = $"{Path.GetDirectoryName(file)}\\{Path.GetFileNameWithoutExtension(file)}.mp4";
                        var process = new Process();
                        process.StartInfo.FileName = $"FFmpeg\\ffmpeg.exe";
                        process.StartInfo.Arguments = $"-y -i \"{tempFile.FullName}\" -c:v copy -f mp4 \"{mp4File}\"";
                        process.Start();
                        process.WaitForExit();
                        process.Close();

                        tempFile.Delete();
                    }
                }
            });
        }
    }
}
