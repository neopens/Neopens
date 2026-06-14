using Neopens.FrameworkLite.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Neopens.FrameworkLite.Extensions
{
    internal static class LoggerExtension
    {
        private readonly static Logger _logger = new Logger();

        public static void Debug(this object sender, string message)
        {   
            _logger.Debug(string.Format($"{GetSenderString(sender)} {message}"));
        }

        public static void Debug(this object sender, string format, params object[] args)=>Debug(sender,string.Format(format,args));
     
        public static void Info(this object sender, string message)
        {
            _logger.Info(string.Format($"{GetSenderString(sender)} {message}"));
        }

        public static void Info(this object sender, string format, params object[] args) => Info(sender, string.Format(format, args));

        public static void Warn(this object sender, string message)
        {
            _logger.Warn(string.Format($"{GetSenderString(sender)} {message}"));
        }
        public static void Warn(this object sender, string format, params object[] args) => Warn(sender, string.Format(format, args));

        public static void Warn(this object sender, string message,Exception ex)
        {
            _logger.Warn(string.Format($"{GetSenderString(sender)} {message}"),ex);
        }

        public static void Error(this object sender, string message)
        {
            _logger.Error(string.Format($"{GetSenderString(sender)} {message}"));
        }

        public static void Error(this object sender, string message, Exception ex)
        {
            _logger.Error(string.Format($"{GetSenderString(sender)} {message}"), ex);
        }

        public static void Error(this object sender, string format, params object[] args) => Error(sender, string.Format(format, args));

        private static string GetSenderString(object sender)
        {
            return string.Empty;

            if (sender is null) return string.Empty;
     
            return $"<{sender.GetType().Name}>";
        }
    }
}
