using System;

namespace Venz.Telemetry
{
    internal static class ExceptionExtensions
    {
        public static String AsDebugMessage(this Exception exception)
        {
            if (exception == null)
                return "";

            var message = $"{exception.GetType().FullName}: {exception.Message}\n{exception.StackTrace}\n";
            message += CreateInnerExceptionMessage(exception.InnerException);
            return message;
        }

        private static String CreateInnerExceptionMessage(Exception exception)
        {
            if (exception == null)
                return "";

            var message = $"Caused by {exception.GetType().FullName}: {exception.Message}\n{exception.StackTrace}\n";
            message += CreateInnerExceptionMessage(exception.InnerException);
            return message;
        }
    }
}
