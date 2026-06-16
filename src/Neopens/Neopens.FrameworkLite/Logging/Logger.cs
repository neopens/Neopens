using Neopens.FrameworkLite.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Neopens.FrameworkLite.Logging
{
    internal class Logger : ILogger, IDisposable, ILogFormatter
    {
        public bool IsDebugEnabled { get; protected set; }
        public bool IsInfoEnabled { get; protected set; }
        public bool IsWarnEnabled { get; protected set; }
        public bool IsErrorEnabled { get; protected set; }
        public bool LogSaveable { get; set; }

        private readonly object _streamLocker = new object();

        private readonly StringBuilder _logBuilder = new StringBuilder(8192);

        private readonly ConcurrentQueue<LogMessage> messageQueue = new ConcurrentQueue<LogMessage>();

        private readonly LoggerOptions _options;

        private readonly List<LogMessage> _batchBuffer;

        private Timer _flushTimer;

        private FileStream _currentLogStream;

        //0 表示 不在刷入磁盘 1 表示正在刷入磁盘
        private int isFlushingFlag = 0;  

        /// <summary>
        /// 上一次刷入时间
        /// </summary>
        private DateTime _lastFlushTime = DateTime.UtcNow;

        public Logger() : this(LoggerOptions.Default) { }

        internal Logger(LoggerOptions options)
        {
            _options = options ?? LoggerOptions.Default;

            _batchBuffer = new List<LogMessage>(_options.MaxCacheSize);

            LogSaveable = _options.SaveLog2File;

            SetLogLevelEnabled(_options.GetLevels());

            _flushTimer = new Timer(FlushTimerCallback, null, 3000, options.FlushInterval);
        }

        ~Logger() { Dispose(); }

        public void Debug(string message)
        {
            if (!IsDebugEnabled) return;
            PrintLog(LogLevel.DEBUG, message);
        }

        public void Debug(string format, params object[] args) => Debug(string.Format(format, args));

        public void Info(string message)
        {
            if (!IsInfoEnabled) return;
            PrintLog(LogLevel.INFO, message);
        }

        public void Info(string format, params object[] args) => Info(string.Format(format, args));

        public void Warn(string message)
        {
            if (!IsWarnEnabled) return;

            PrintLog(LogLevel.WARN, message);
        }

        public void Warn(string message, Exception ex)
        {
            string formattedMessage = $"{message}{FormatExceptionContent(ex)}";

            Warn(formattedMessage);
        }

        public void Warn(string format, params object[] args) => Warn(string.Format(format, args));

        public void Warn(string format, Exception ex, params object[] args) => Warn(string.Format(format, args), ex);

        public void Error(string message)
        {
            if (!IsErrorEnabled) return;
            PrintLog(LogLevel.ERROR, message);
        }

        public void Error(string message, Exception ex)
        {
            string formattedMessage = $"{message}{FormatExceptionContent(ex)}";

            Error(formattedMessage);
        }

        public void Error(string format, params object[] args) => Error(string.Format(format, args));

        public void Error(string format, Exception ex, params object[] args) => Error(string.Format(format, args), ex);

        public void Dispose()
        {
            _flushTimer.Change(Timeout.Infinite, Timeout.Infinite);

            FlushQueueLogs(true);

            CloseCurrentFileStream();

            _flushTimer.Dispose();

            GC.SuppressFinalize(this);
        }

        private void SetLogLevelEnabled(LogLevel[] logLevels)
        {
            IsDebugEnabled = false;
            IsInfoEnabled = false;
            IsWarnEnabled = false;
            IsErrorEnabled = false;

            if (logLevels == null || logLevels.Length == 0) return;

            foreach (var level in logLevels)
            {
                SetLogLevelEnabled(level, true);
            }
        }

        private void SetLogLevelEnabled(LogLevel logLevel, bool enabled)
        {
            switch (logLevel)
            {
                case LogLevel.DEBUG:
                    IsDebugEnabled = enabled;
                    break;
                case LogLevel.INFO:
                    IsInfoEnabled = enabled;
                    break;
                case LogLevel.WARN:
                    IsWarnEnabled = enabled;
                    break;
                case LogLevel.ERROR:
                    IsErrorEnabled = enabled;
                    break;
            }
        }


        private void FlushTimerCallback(object state)
        {
            if (Interlocked.Exchange(ref isFlushingFlag, 1) == 1) return;

            try
            {
                FlushQueueLogs();
            }
            finally
            {
                Interlocked.Exchange(ref isFlushingFlag, 0);

            }
        }

       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isFlush">true:强制刷入，不管到没到时间</param>
        private void FlushQueueLogs(bool isFlush = false)
        {
            if (!isFlush) 
            {
                var intervalMs = (DateTime.UtcNow - _lastFlushTime).TotalMilliseconds;

                if (intervalMs < _options.FlushInterval) return;
            }           

            _batchBuffer.Clear();

            while (messageQueue.TryDequeue(out LogMessage logMessage))
            {
                _batchBuffer.Add(logMessage);
            }

            FlushLogs(_batchBuffer);

            _lastFlushTime = DateTime.UtcNow;
        }

        private void FlushLogs(List<LogMessage> batchBuffer)
        {
            if (!LogSaveable) return;

            if (batchBuffer.Count <= 0) return;

            _logBuilder.Clear();

            foreach (var item in batchBuffer)
            {
                _logBuilder.AppendLine(Format(item));
            }

            var logBytes = Encoding.UTF8.GetBytes(_logBuilder.ToString());

            lock (_streamLocker)
            {
                _currentLogStream = GetCurrentFileStream();
                _currentLogStream.Write(logBytes, 0, logBytes.Length);
                _currentLogStream.Flush();
            }
        }

        /// <summary>
        /// 获取当前日志存储路径
        /// </summary>
        /// <returns></returns>
        private string GetCurrentLogDirectory()
        {
            string logdirPath = string.Empty;

            try
            {
                logdirPath = string.Format(_options.LogDirectory, DateTime.UtcNow.ToLocalTime());
            }
            catch (Exception)
            {
                logdirPath = _options.LogDirectory;
            }

            if (!Directory.Exists(logdirPath))
                Directory.CreateDirectory(logdirPath);

            return logdirPath;
        }

        /// <summary>
        /// 获取当前日志文件
        /// </summary>
        /// <returns></returns>
        private string GetCurrentLogFilePath()
        {
            var logDirectory = GetCurrentLogDirectory();
            string fileName = $"{string.Format(_options.FileNameFormat, DateTime.UtcNow.ToLocalTime())}{_options.FileExtension}";
            return Path.Combine(logDirectory, fileName);
        }

        /// <summary>
        /// 获取当前文件流，如果文件大小超过限制则备份当前文件并创建新文件
        /// </summary>
        /// <returns></returns>
        private FileStream GetCurrentFileStream()
        {
            string filePath = GetCurrentLogFilePath();

            lock (_streamLocker)
            {
                if (_currentLogStream != null)
                {
                    var currentFilePath = _currentLogStream.Name;

                    bool isSameFile = string.Equals(currentFilePath, filePath);

                    if (isSameFile && _currentLogStream.Length < _options.PerFileMaxSize)
                    {
                        return _currentLogStream;
                    }
                    CloseCurrentFileStream();
                }

                FileInfo fileInfo = GetFileInfo(filePath);

                _currentLogStream = new FileStream(fileInfo.FullName,
                                                    FileMode.Append,
                                                    FileAccess.Write,
                                                    FileShare.Read,
                                                    65536);

            }
            return _currentLogStream;
        }

        private void CloseCurrentFileStream()
        {
            if (_currentLogStream is null) return;

            lock (_streamLocker)
            {
                _currentLogStream.Flush(true);
                _currentLogStream.Close();
                _currentLogStream = null;
            }
        }

        private FileInfo GetFileInfo(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            SplitLog(fileInfo, _options.PerFileMaxSize);

            return fileInfo;
        }

        private void SplitLog(FileInfo fileInfo, int backupSize)
        {
            if (!fileInfo.Exists) return;

            if (fileInfo.Length < backupSize) return;

            var logDirectory = fileInfo.DirectoryName;

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.Name);

            var fileExt = fileInfo.Extension;

            var fileName = Path.GetFileName(fileInfo.Name);

            int count = Directory.GetFiles(logDirectory)?.Count(x => Path.GetFileNameWithoutExtension(x).StartsWith(fileNameWithoutExt)) ?? 0;

            string newFileName = $"{fileNameWithoutExt}{fileExt}{count + 1}";

            string newFilePath = Path.Combine(logDirectory, newFileName);
            try
            {
                fileInfo.MoveTo(newFilePath);
            }
            catch (Exception ex)
            {
                ConsoleLogger.WriteLine(LogLevel.ERROR, $"Split Log Exception {ex.Message} {ex.StackTrace}");
            }
        }

        private void PrintLog(LogLevel level, string message)
        {
            if (!_options.LogIsEnable) return;

            var logMsg = new LogMessage(level, message, DateTime.UtcNow);

            ConsoleLogger.WriteLine(level, Format(logMsg));

            messageQueue.Enqueue(logMsg);

            if (messageQueue.Count > _options.MaxCacheSize)
            {
                FlushQueueLogs();
            }
        }

        private static string FormatExceptionContent(Exception ex)
        {
            if (ex == null) return string.Empty;

            var sb = new StringBuilder();
            FormatExceptionContentRecursive(sb, ex);
            return sb.ToString();
        }

        private static void FormatExceptionContentRecursive(StringBuilder sb, Exception ex)
        {
            sb.AppendLine($"      Exception: {ex.Message}");
            sb.AppendLine($"      StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                sb.AppendLine("      --- Inner Exception ---");
                FormatExceptionContentRecursive(sb, ex.InnerException);
            }
        }

        public virtual string Format(LogMessage message)
        {
            return $"{message.Timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} [{message.Level}] {message.Message}";
        }
    }

    internal class Logger<T> : Logger, ILogger<T>
    {
        private readonly string typeName;

        public Logger():base()
        {
            typeName = typeof(T).Name;
        }
        public Logger(LoggerOptions options) :base(options)
        {
            typeName = typeof(T).Name;
        }

        public override string Format(LogMessage message)
        {
            return $"{message.Timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff} [{message.Level}] < {typeName} > {message.Message}";
        }
    }
}
