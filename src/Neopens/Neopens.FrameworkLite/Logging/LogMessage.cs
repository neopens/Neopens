using System;
using System.Collections.Generic;
using System.Text;

namespace Neopens.FrameworkLite.Logging
{
   internal struct LogMessage
    {
        public LogLevel Level { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }
        public LogMessage(LogLevel level, string message, DateTime timestamp)
        {
            this.Level = level;
            this.Message = message;
            this.Timestamp = timestamp;
        }    
    }
}
