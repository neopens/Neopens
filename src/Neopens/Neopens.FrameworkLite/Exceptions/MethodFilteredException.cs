using Neopens.FrameworkLite.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neopens.FrameworkLite.Exceptions
{
    internal class MethodFilteredException:Exception
    {
        public MethodFilteredException(ActionFilterAttribute filter):base(filter.Message)
        {

        }
    }
}
