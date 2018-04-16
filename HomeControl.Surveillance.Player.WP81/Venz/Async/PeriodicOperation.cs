using System;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Venz.Async
{
    public class PeriodicOperation
    {
        private Timer Timer;
        private TimeSpan Delay;
        private Window Window;
        private Boolean IsUiBased;

        public event EventHandler Triggered;



        private PeriodicOperation() { }

        public static PeriodicOperation CreateUiBased(TimeSpan delay)
        {
            var instance = new PeriodicOperation() { IsUiBased = true };
            instance.Window = Window.Current;
            if (instance.Window == null)
                throw new InvalidOperationException("Unable to find current Window.");
            instance.Timer = new Timer(instance.OnTriggered, null, Timeout.Infinite, (Int32)delay.TotalMilliseconds);
            instance.Delay = delay;
            return instance;
        }

        public void Start()
        {
            if (IsUiBased)
                Window.VisibilityChanged += OnWindowVisibilityChanged;
            Timer.Change(0, (Int32)Delay.TotalMilliseconds);
        }

        public void Stop()
        {
            if (IsUiBased)
                Window.VisibilityChanged -= OnWindowVisibilityChanged;
            Timer.Change(Timeout.Infinite, (Int32)Delay.TotalMilliseconds);
        }

        private async void OnTriggered(Object param)
        {
            if (IsUiBased)
                await Window.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Triggered?.Invoke(this, EventArgs.Empty)).AsTask().ConfigureAwait(false);
        }

        private void OnWindowVisibilityChanged(Object sender, VisibilityChangedEventArgs args)
        {
            Timer.Change(args.Visible ? 0 : Timeout.Infinite, (Int32)Delay.TotalMilliseconds);
        }
    }
}
