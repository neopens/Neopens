using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Neopens.FrameworkLite.Logging
{
    static class ConsoleLogger
    {

        [Conditional("DEBUG"), Conditional("DEV")]
        public static void WriteLine(LogLevel level, string message)
        {
            SetConsoleColor(level);

            Console.WriteLine(message);

            Debug.WriteLine(message);

            Console.ResetColor();
        }

        [Conditional("DEBUG"), Conditional("DEV")]
        private static void SetConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.WARN:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.DEBUG:
                case LogLevel.INFO:
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
        }
    }
}
