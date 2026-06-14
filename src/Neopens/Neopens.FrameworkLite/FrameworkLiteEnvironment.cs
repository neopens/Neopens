using Neopens.FrameworkLite.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Neopens.FrameworkLite
{
    public static class FrameworkLiteEnvironment
    {
        /// <summary>
        /// 释放启用日志
        /// </summary>
        public static bool LogEnabled { get; set; } = false;

        /// <summary>
        /// 日志文件路径
        /// </summary>
        public static string LogPath { get; set; } = Path.Combine(Environment.CurrentDirectory,"Logs");

        /// <summary>
        /// 是否存储日志到文件
        /// </summary>
        public static bool SaveLogFile { get; set; } = false;   
       
    }
}
