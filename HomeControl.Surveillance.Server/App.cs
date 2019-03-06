﻿using HomeControl.Surveillance.Server.Model;
using System;
using System.Threading.Tasks;
using Venz.Telemetry;

namespace HomeControl.Surveillance.Server
{
    public class App
    {
        public static Diagnostics Diagnostics { get; } = new Diagnostics("App");
        public static ApplicationModel Model { get; } = new ApplicationModel();

        public App()
        {
            Diagnostics.Debug.Add(new DebugTelemetryService());
            Diagnostics.Debug.Add(new ConsoleTelemetryService());
            Diagnostics.Debug.Add(new FileTelemetryService());
        }

        public async void Start() => await Task.Run(() =>
        {
            Model.Initialize();
        });

        public sealed class ConsoleTelemetryService: ITelemetryService
        {
            public void Start() => Console.WriteLine($"{GetTimestamp()} >> Application Launched");
            public void Finish() => Console.WriteLine($"{GetTimestamp()} >> Application Exit");
            public void LogEvent(String title) => Console.WriteLine($"{GetTimestamp()} >> {title}");
            public void LogEvent(String title, String parameter, String value) => Console.WriteLine($"{GetTimestamp()} >> {title} || {parameter}: {value}");
            public void LogException(String comment, Exception exception) => Console.WriteLine($"{GetTimestamp()} >> {comment} || {exception.GetType().FullName}: {exception.Message}");

            private String GetTimestamp() => DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
        }
    }
}