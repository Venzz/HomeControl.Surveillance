using HomeControl.Surveillance.Player.ViewModel;
using System;
using System.Linq;
using Venz.UI.Xaml;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;

namespace HomeControl.Surveillance.Player.View
{
    public sealed partial class FileStoragePage: Page
    {
        private FileStorageContext Context = new FileStorageContext();

        public FileStoragePage()
        {
            InitializeComponent();
            DataContext = Context;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            await Context.InitializeAsync();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            base.OnNavigatingFrom(args);
            Context.CancelSaving();
        }

        private void OnFilesSelectionChanged(Object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs args)
        {
            Context.SetSelection(args.AddedItems);
        }

        private async void OnSaveTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args)
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
            folderPicker.FileTypeFilter.Add("*");
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
                return;

            Context.Save(folder, FilesView.SelectedItems.Cast<String>().ToList());
        }

        private void OnCancelTapped(Object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs args)
        {
            Context.CancelSaving();
        }
    }
}
