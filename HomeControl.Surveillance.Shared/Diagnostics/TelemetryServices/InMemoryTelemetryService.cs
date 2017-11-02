using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace Venz.Telemetry
{
    public sealed class InMemoryTelemetryService: ITelemetryService
    {
        public IList<String> Items { get; } = new List<String>();
        public Object Sync { get; } = new Object();

        public event TypedEventHandler<InMemoryTelemetryService, String> ItemAdded;



        public InMemoryTelemetryService()
        {
            ItemAdded = delegate { };
        }

        public void Start() => AddItem($"{GetTimestamp()} >> Application Launched");

        public void Finish() => AddItem($"{GetTimestamp()} >> Application Exit");

        public void LogEvent(String title) => AddItem($"{GetTimestamp()} >> {title}");

        public void LogEvent(String title, String parameter, String value) => AddItem($"{GetTimestamp()} >> {title} || {parameter}: {value}");

        public void LogException(String comment, Exception exception) => AddItem($"{GetTimestamp()} >> {comment} || {exception.GetType().FullName}: {exception.Message}");

        private void AddItem(String item)
        {
            lock (Sync)
            {
                Items.Add(item);
                ItemAdded(this, item);
            }
        }

        private String GetTimestamp() => DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
    }
}
