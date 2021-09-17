using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectLogger
{
    public static class ConsoleLoggerExtensions
    {

        public static void LogObject(this ILogger logger, LogLevel logLevel, string message, object obj)
        {
            StringBuilder builder = new StringBuilder("{\n", 200);
            StringBuilder builderObj = new StringBuilder("\t\t", 200);
            builderObj.Append(Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented));
            builderObj.Replace("\r\n", "\r\n\t\t");

            builder.Append('\t').Append("Object").Append(": ")
                   .AppendLine()
                   .Append(builderObj).Append('\n')
                   .Append('}');

            logger.Log(logLevel, $"{message}" + "\n" + builder);
        }
        public static ILoggerFactory AddConsoleLogger(this ILoggerFactory loggerFactory, ConsoleLoggerConfiguration config)
        {
            loggerFactory.AddProvider(new ConsoleLoggerProvider(config));
            return loggerFactory;
        }
        public static ILoggerFactory AddConsoleLogger(this ILoggerFactory loggerFactory)
        {
            var config = new ConsoleLoggerConfiguration();
            return loggerFactory.AddConsoleLogger(config);
        }
        public static ILoggerFactory AddConsoleLogger(this ILoggerFactory loggerFactory, Action<ConsoleLoggerConfiguration> configure)
        {
            var config = new ConsoleLoggerConfiguration();
            configure(config);
            return loggerFactory.AddConsoleLogger(config);
        }
    }
}
