using Microsoft.Extensions.Logging;

namespace shopFlow.Utils
{

    public static class LoggerUtils
    {
        private static readonly ILoggerFactory LoggerFactory = InitLoggerFactory();

        private static ILoggerFactory InitLoggerFactory()
        {
            return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder
                    .AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss.FFFFFF ";
                    })
                    .AddDebug(); // Add debug logging
            });
        }

        public static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        public static ILogger CreateLogger(string categoryName)
        {
            return LoggerFactory.CreateLogger(categoryName);
        }
    }
}