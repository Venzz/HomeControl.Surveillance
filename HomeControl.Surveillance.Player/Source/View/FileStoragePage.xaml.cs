using HomeControl.Surveillance.Player.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
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
            await Context.InitializeAsync();
        }

        private void OnFilesSelectionChanged(Object sender, SelectionChangedEventArgs args)
        {
            Context.SetSelection(args.AddedItems);
        }

        private async void OnSaveClicked(Object sender, Windows.UI.Xaml.RoutedEventArgs args)
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
            folderPicker.FileTypeFilter.Add("*");
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
                return;

            Context.Save(folder, FilesView.SelectedItems.Cast<String>().ToList());
        }

        private void OnCancelClicked(Object sender, Windows.UI.Xaml.RoutedEventArgs args)
        {
            Context.CancelSaving();
        }
    }
}
