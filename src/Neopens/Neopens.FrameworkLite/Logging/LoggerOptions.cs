using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Neopens.FrameworkLite.Logging
{
    internal class LoggerOptions
    {
        public static readonly LoggerOptions Default = new LoggerOptions();
    

        private const int MIN_FLUSH_INTERVAL = 1000;

        /// <summary>
        /// 单个日志文件最大默认大小 10mb
        /// </summary>
        private const int DEFAULT_PER_FILE_MAX_SIZE = 10 * 1024 * 1024;

        /// <summary>
        /// 单个日志文件最大默认大小 最低下线 1mb
        /// </summary>
        private const int PER_FILE_MAX_SIZE_LOW = 1 * 1024 * 1024;

        private const string DEFAULT_FILE_NAME_FORMAT = "log_{0:yyyy-MM-dd_HH}";

        /// <summary>
        /// 最大缓存条目
        /// </summary>
        public int MaxCacheSize { get; set; } = 20;

        /// <summary>
        /// 单个日志文件最大大小，单位：字节 默认 10MB
        /// </summary>
        public int PerFileMaxSize { get; set; } = DEFAULT_PER_FILE_MAX_SIZE;

        /// <summary>
        /// 日志文件格式
        /// </summary>
        public string FileNameFormat { get; set; } = DEFAULT_FILE_NAME_FORMAT;

        /// <summary>
        /// 日志后缀
        /// </summary>
        public string FileExtension { get; set; } = ".log";
        public string LogDirectory { get; set; } = Path.Combine(Environment.CurrentDirectory, "Logs", "{0:yyyy-MM-dd}");

        private int flushInterval = 3000;

        /// <summary>
        /// 日志输入间隔 默认 3s，最小 1s
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

        public static LoggerOptions ReadOptionsFromXml(string xmlFilePath) 
        {
            LoggerOptions options = new LoggerOptions()
            {
                LogIsEnable = false,
                SaveLog2File = false,
                LevelRules = "WARN | ERROR"
            };

            if (!File.Exists(xmlFilePath))
            {
                Debug.WriteLine($"XML config file not found: {xmlFilePath}");

                return options;
            }

            XDocument xDoc = null;
            try
            {
                xDoc = XDocument.Load(xmlFilePath);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to load XML config: {e.Message}");
                return options;
            }

            var logElement = xDoc?.Root?.Element("Log");

            if (logElement is null) return options;

            string isEnableStr = logElement.Attribute("isEnable")?.Value?.ToLower()??"false";

            if (bool.TryParse(isEnableStr, out var isEnable)) 
            {
                options.LogIsEnable = isEnable;
                options.SaveLog2File = isEnable;
            }

            string fileMaxSizeStr = logElement.Attribute("perFileMaxSize")?.Value?.ToLower() ?? string.Empty;

            if (int.TryParse(fileMaxSizeStr, out var fileMaxSize)) 
            {
                options.PerFileMaxSize = fileMaxSize > PER_FILE_MAX_SIZE_LOW ? fileMaxSize: PER_FILE_MAX_SIZE_LOW;
            }

            var layoutElement = logElement.Element("Layout");

            if (layoutElement is null) return options;

            options.LevelRules = layoutElement.Attribute("level")?.Value??"*";

            string fileFormat = layoutElement.Attribute("name")?.Value ?? "";

            var dirPath = Path.GetDirectoryName(fileFormat);

            options.LogDirectory = string.IsNullOrEmpty(dirPath)? Path.Combine(Environment.CurrentDirectory, "Logs", "{0:yyyy-MM-dd}"): dirPath;

            var fileName = Path.GetFileNameWithoutExtension(fileFormat);
            var extension = Path.GetExtension(fileFormat);
            options.FileNameFormat = string.IsNullOrEmpty(fileName)? DEFAULT_FILE_NAME_FORMAT: fileName;
            options.FileExtension = extension;

            return options;
        }
    }
}
