using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.ServerStarter
{
    public class Program
    {
        private const String ServerApplication = "HomeControl/HomeControl.Surveillance.Server.dll";

        public static void Main(String[] args)
        {
            var process = Process.Start("dotnet", ServerApplication);
            while (true)
            {
                while (!process.HasExited)
                    Task.Delay(1000).Wait();

                process = Process.Start("dotnet", ServerApplication);
            }
        }
    }
}
