using Neopens.FrameworkLite.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neopens.FrameworkLite.Interfaces
{
    internal interface ILogFormatter
    {
        string Format(LogMessage message);
    }
}
