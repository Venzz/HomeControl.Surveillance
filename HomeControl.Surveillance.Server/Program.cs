using System;

namespace HomeControl.Surveillance.Server
{
    public class Program
    {
        private static App App;

        public static void Main(String[] args)
        {
            App = new App();
            App.Start();

            while (true)
                Console.ReadLine();
        }
    }
}
