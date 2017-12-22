using System;
using Windows.ApplicationModel.Activation;

namespace Venz.UI.Xaml
{
    public static class ApplicationExtensions
    {
        public static Boolean IsNewInstance(this IActivatedEventArgs args)
        {
            return (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser) || (args.PreviousExecutionState == ApplicationExecutionState.NotRunning) || (args.PreviousExecutionState == ApplicationExecutionState.Terminated);
        }
    }
}
