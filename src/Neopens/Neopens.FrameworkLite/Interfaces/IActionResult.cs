using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neopens.FrameworkLite.Interfaces
{
    public interface IActionResult
    {
        int State { get; }
        object Content { get; }
        string Message { get; }
        DateTime Time { get; }
    }

    public interface IActionResult<T> : IActionResult
    {
        new T Content { get; }
    }

}




