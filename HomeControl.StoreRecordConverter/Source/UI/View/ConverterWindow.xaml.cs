using HomeControl.StoreRecordConverter.Controller;
using System;
using System.Linq;
using System.Windows;

namespace HomeControl.StoreRecordConverter.View
{
    public partial class ConverterWindow: Window
    {
        private ConverterController Context = new ConverterController();

        public ConverterWindow()
        {
            InitializeComponent();
            DataContext = Context;
        }

        private async void OnDrop(Object sender, DragEventArgs args)
        {
            if (!args.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (String[])args.Data.GetData(DataFormats.FileDrop);
            Context.Add(files.ToList());
            foreach (var file in Context.Files)
                await Context.ConvertAsync(file);
        }
    }
}
