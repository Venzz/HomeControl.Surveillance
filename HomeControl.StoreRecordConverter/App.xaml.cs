using System.IO;
using System.Reflection;
using System.Windows;

namespace HomeControl.StoreRecordConverter
{
    public partial class App: Application
    {
        protected override void OnStartup(StartupEventArgs args)
        {
            base.OnStartup(args);
            var executablePath = Assembly.GetExecutingAssembly().Location;
            var workingDirectory = Path.GetDirectoryName(executablePath);
            Directory.SetCurrentDirectory(workingDirectory);
        }
    }
}
