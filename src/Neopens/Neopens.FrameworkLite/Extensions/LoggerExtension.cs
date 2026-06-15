using Neopens.FrameworkLite.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Neopens.FrameworkLite.Extensions
{
    internal static class LoggerExtension
    {
        public static void Debug(this object sender, string message)
        {
            LoggerFactory.Default.Debug(string.Format($"{GetSenderString(sender)} {message}"));
        }

        public static void Debug(this object sender, string format, params object[] args)=>Debug(sender,string.Format(format,args));
     
        public static void Info(this object sender, string message)
        {
            LoggerFactory.Default.Info(string.Format($"{GetSenderString(sender)} {message}"));
        }

        public static void Info(this object sender, string format, params object[] args) => Info(sender, string.Format(format, args));

        public static void Warn(this object sender, string message)
        {
            LoggerFactory.Default.Warn(string.Format($"{GetSenderString(sender)} {message}"));
        }
        public static void Warn(this object sender, string format, params object[] args) => Warn(sender, string.Format(format, args));

        public static void Warn(this object sender, string message,Exception ex)
        {
            LoggerFactory.Default.Warn(string.Format($"{GetSenderString(sender)} {message}"),ex);
        }

        public static void Error(this object sender, string message)
        {
            LoggerFactory.Default.Error(string.Format($"{GetSenderString(sender)} {message}"));
        }

        public static void Error(this object sender, string message, Exception ex)
        {
            LoggerFactory.Default.Error(string.Format($"{GetSenderString(sender)} {message}"), ex);
        }

        public static void Error(this object sender, string format, params object[] args) => Error(sender, string.Format(format, args));

        private static string GetSenderString(object sender)
        {
            return string.Empty;
        }
    }
}
