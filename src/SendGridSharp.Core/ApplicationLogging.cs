using Microsoft.Extensions.Logging;

namespace SendGridSharp.Core
{
    internal static class ApplicationLogging
    {
        private static bool _initialized;

        private static ILoggerFactory _loggerFactory;
        public static ILoggerFactory LoggerFactory => LoggerFactoryInternal();

        private static ILoggerFactory LoggerFactoryInternal()
        {
            if (_initialized)
                return _loggerFactory;

            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddConsole(LogLevel.Debug);
            _loggerFactory.AddDebug();
            _initialized = true;

            return _loggerFactory;
        }

        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}
