using System;
using System.Collections.Generic;
using System.Text;

namespace Neopens.FrameworkLite.Logging
{
    internal interface ILogFormatter
    {
        string Format(LogMessage message);
    }
}
