using Neopens.FrameworkLite.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Neopens.FrameworkLite.Logging
{
    public static class LoggerFactory
    {
        public static ILogger Default { get; set; } = GetDefaultLogger();


        private static ILogger GetDefaultLogger() 
        {
            var options = new LoggerOptions()
            {
                LogIsEnable = true,
                LogDirectory = Path.Combine(Environment.CurrentDirectory, "Logs"),
                SaveLog2File = true,
            };

            return new Logger(options);
        }

    }
}
