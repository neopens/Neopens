using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Neopens.FrameworkLite.Logging
{
    public static class LoggerFactory
    {
        public static ILogger Default { get; set; } = GetDefaultLogger();


        private static ILogger GetDefaultLogger() => new Logger(GetDefaultOptions());

        private static LoggerOptions GetDefaultOptions() 
        {
            var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location ?? string.Empty;

            if (string.IsNullOrEmpty(entryAssemblyPath))
            {
               return new LoggerOptions()
                {
                    LogIsEnable = false,
                    SaveLog2File = false,
                    LevelRules = "WARN | ERROR"
                };
            }
            string xmlFilePath = $"{entryAssemblyPath}.config";

            return LoggerOptions.ReadOptionsFromXml(xmlFilePath);
        }

    }
}
