using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Venz.Data;
using Windows.Storage;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class FileStorageContext: ObservableObject
    {
        public IEnumerable<String> Files { get; private set; }
        public Boolean IsSelected { get; private set; }
        public FileSavingProcess SavingProcess { get; }



        public FileStorageContext() { SavingProcess = new FileSavingProcess(); }

        public Task InitializeAsync() => Task.Run(async () =>
        {
            Files = await App.Model.ProviderController.GetFileListAsync().ConfigureAwait(false);
            OnPropertyChanged(nameof(Files));
        });

        public void SetSelection(IList<Object> items)
        {
            IsSelected = items.Count > 0;
            OnPropertyChanged(nameof(IsSelected));
        }

        public async void Save(StorageFolder folder, IReadOnlyCollection<String> items) => await Task.Run(() => SavingProcess.SaveAsync(folder, items));

        public void CancelSaving() => SavingProcess.Cancel();



        public class FileSavingProcess: ObservableObject
        {
            private Boolean IsCancelled;

            public FileSavingProgress Status { get; private set; }
            public Boolean InProgress { get; private set; }



            public FileSavingProcess() { }

            public async Task SaveAsync(StorageFolder folder, IReadOnlyCollection<String> items)
            {
                IsCancelled = false;
                InProgress = true;
                Status = new FileSavingProgress(items.First(), 0, 0);
                OnPropertyChanged(nameof(InProgress), nameof(Status));

                var currentItemIndex = 0;
                foreach (var item in items)
                {
                    var currentFileOffset = 0U;
                    var fileName = item.ToString();
                    var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName).AsTask().ConfigureAwait(false);
                    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false))
                    {
                        Status = new FileSavingProgress(fileName, currentFileOffset, (Double)currentItemIndex / items.Count);
                        OnPropertyChanged(nameof(Status));

                        while (true)
                        {
                            if (IsCancelled)
                            {
                                InProgress = false;
                                OnPropertyChanged(nameof(InProgress));
                                return;
                            }

                            var fileData = await App.Model.ProviderController.GetFileDataAsync(fileName, currentFileOffset, 10 * 1000 * 1000).ConfigureAwait(false);
                            if (IsCancelled)
                            {
                                InProgress = false;
                                OnPropertyChanged(nameof(InProgress));
                                return;
                            }
                            if (fileData.Length == 0)
                            {
                                currentItemIndex++;
                                break;
                            }

                            await fileStream.WriteAsync(fileData.AsBuffer()).AsTask().ConfigureAwait(false);
                            currentFileOffset += (UInt32)fileData.Length;
                            Status = new FileSavingProgress(fileName, currentFileOffset, (Double)currentItemIndex / items.Count);
                            OnPropertyChanged(nameof(Status));
                        }
                    }
                }

                InProgress = false;
                OnPropertyChanged(nameof(InProgress));
            }

            public void Cancel()
            {
                IsCancelled = true;
            }
        }

        public class FileSavingProgress
        {
            public String Text { get; private set; }
            public Double Progress { get; private set; }

            public FileSavingProgress(String fileName, UInt32 downloadedSize, Double progress)
            {
                var sizeInGigabytes = (Double)downloadedSize / 1000 / 1000 / 1000;
                if (sizeInGigabytes >= 1)
                    Text = $"{fileName} - {sizeInGigabytes:0.###} Gb";
                else if (sizeInGigabytes * 1000 >= 1)
                    Text = $"{fileName} - {(sizeInGigabytes * 1000):0.###} Mb";
                else if (sizeInGigabytes * 1000 * 1000 >= 1)
                    Text = $"{fileName} - {(sizeInGigabytes * 1000 * 1000):0.###} Kb";
                else
                    Text = $"{fileName} - {downloadedSize} b";
                Progress = progress;
            }
        }
    }
}
