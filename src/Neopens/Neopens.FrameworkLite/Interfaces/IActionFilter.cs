using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Neopens.FrameworkLite.Interfaces
{
    public interface IActionFilter
    {
        object Executing(MethodInfo method);
        object Executed(MethodInfo method);
    }
}
