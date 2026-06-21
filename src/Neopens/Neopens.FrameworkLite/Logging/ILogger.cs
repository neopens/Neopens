using System;

namespace Neopens.FrameworkLite.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// 日志可存储
        /// </summary>
        bool LogSaveable { get; set; }

        void Debug(string message);
        void Debug(string format,params object[] args);
        void Info(string message);
        void Info(string format, params object[] args);

        void Warn(string message);
        void Warn(string message,Exception ex);
        void Warn(string format, Exception ex, params object[] args);
        void Warn(string format, params object[] args);

        void Error(string message);
        void Error(string format, params object[] args);

        void Error(string message, Exception ex);
        void Error(string format, Exception ex, params object[] args);
    }

    public interface ILogger<T>: ILogger
    {       
    }
}
