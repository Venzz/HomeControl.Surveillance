using HomeControl.Surveillance.Player.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venz.Data;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;

namespace HomeControl.Surveillance.Player.ViewModel
{
    public class CameraContext: ObservableObject, IDisposable
    {
        public CameraController CameraController { get; private set; }
        public CameraStream CameraStream { get; private set; }
        public Boolean IsTilePinned { get; private set; }
        public IconElement TileIcon => new SymbolIcon(IsTilePinned ? Symbol.UnPin : Symbol.Pin);
        public IEnumerable<StoredRecord> StoredRecords { get; private set; }
        public StoredRecordStream StoredRecordStream { get; private set; }



        public CameraContext() { }

        public void Initialize(CameraController cameraController, CoreDispatcher dispatcher)
        {
            OverrideDispatcher(dispatcher);
            CameraController = cameraController;
            CameraStream = new CameraStream(cameraController);
            IsTilePinned = SecondaryTile.Exists(CameraController.Id);
            OnPropertyChanged(nameof(CameraController), nameof(IsTilePinned));
        }

        public Task InitializeAsync() => Task.Run(async () =>
        {
            var storedRecordsMetadata = await CameraController.GetStoredRecordsMetadataAsync().ConfigureAwait(false);
            var storedRecords = new List<StoredRecord>();
            storedRecords.Add(new StoredRecord(null));
            storedRecords.AddRange(storedRecordsMetadata.Select(a => new StoredRecord(a)));

            StoredRecords = storedRecords;
            OnPropertyChanged(nameof(StoredRecords));
        });

        public async void StartZoomingIn() => await Task.Run(() => CameraController.StartZoomingInAsync());

        public async void StartZoomingOut() => await Task.Run(() => CameraController.StartZoomingOutAsync());

        public async void StopZooming() => await Task.Run(() => CameraController.StopZoomingAsync());

        public async Task ManageTileAsync()
        {
            if (IsTilePinned)
            {
                var cameraTile = new SecondaryTile(CameraController.Id);
                IsTilePinned = !await cameraTile.RequestDeleteAsync();
            }
            else
            {
                var cameraTile = new SecondaryTile(CameraController.Id, CameraController.Title, CameraController.Id, new Uri("ms-appx:///Resources/Package/Square_150x150.png"), TileSize.Default);
                IsTilePinned = await cameraTile.RequestCreateAsync();
            }
            OnPropertyChanged(nameof(TileIcon));
        }

        public Task InitializeStoredRecordStreamAsync(StoredRecord storedRecord) => Task.Run(async () =>
        {
            var mediaDataDescriptors = await CameraController.GetMediaDataDescriptorsAsync(storedRecord.Model.Id).ConfigureAwait(false);
            StoredRecordStream = new StoredRecordStream(CameraController, storedRecord.Model.Id, mediaDataDescriptors);
        });

        public void Dispose()
        {
            CameraStream.Dispose();
            StoredRecordStream?.Dispose();
        }



        public class StoredRecord
        {
            public Model.StoredRecord Model { get; }
            public String Title { get; }

            public StoredRecord(Model.StoredRecord model)
            {
                Model = model;
                Title = (model == null) ? "Live" : model.Id;
            }
        }
    }
}
