using HomeControl.Surveillance.Server.ViewModel;
using System.Windows;

namespace HomeControl.Surveillance.Server.View
{
    public partial class MainWindow: Window
    {
        private MainContext Context = new MainContext();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Context;
        }
    }
}
