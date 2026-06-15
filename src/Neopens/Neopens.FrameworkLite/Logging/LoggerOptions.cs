using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Neopens.FrameworkLite.Logging
{
    internal class LoggerOptions
    {
        public static readonly LoggerOptions Default = new LoggerOptions();
    

        private const int MIN_FLUSH_INTERVAL = 33;

        /// <summary>
        /// 最大缓存条目
        /// </summary>
        public int MaxCacheSize { get; set; } = 1024;

        /// <summary>
        /// 单个日志文件最大大小，单位：字节 默认 10MB
        /// </summary>
        public int FileSize { get; set; } = 10 * 1024 * 1024;

        /// <summary>
        /// 日志文件格式
        /// </summary>
        public string FileNameFormat { get; set; } = "log_{0:yyyy-MM-dd_HH}";

        /// <summary>
        /// 日志后缀
        /// </summary>
        public string FileExtension { get; set; } = ".log";
        public string LogDirectory { get; set; } = Path.Combine(Environment.CurrentDirectory, "Logs", "{0:yyyy-MM-dd}");

        private int flushInterval = 333;

        /// <summary>
        /// 日志输入间隔 默认 333ms，最小 33ms
        /// </summary>
        public int FlushInterval
        {
            get => flushInterval;
            set => flushInterval = value < MIN_FLUSH_INTERVAL ? MIN_FLUSH_INTERVAL : value;
        }

        public string LevelRules { get; set; } = "*";// "WARN | ERROR";//"*";

        internal bool SaveLog2File { get; set; } = false;

        internal bool LogIsEnable { get; set; } = false;

        internal LogLevel[] GetLevels()
        {
            var ruleStr = LevelRules?.ToUpper()?.Replace(" ", "");

            if (string.IsNullOrEmpty(ruleStr)) return Array.Empty<LogLevel>();

            if (ruleStr.Equals("*") || ruleStr.Equals("ALL"))
            {
                return new LogLevel[] { LogLevel.DEBUG, LogLevel.INFO, LogLevel.WARN, LogLevel.ERROR };
            }

            var levels = new List<LogLevel>();

            var rules = ruleStr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rule in rules)
            {
                if (Enum.TryParse<LogLevel>(rule, true, out var level))
                {
                    levels.Add(level);
                }
            }

            return levels.ToArray();
        }
    }
}
