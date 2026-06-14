using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neopens.FrameworkLite.Exceptions
{
    internal class MethodNotFoundException:Exception
    {
        public MethodNotFoundException(string methodName):base($"{methodName} not found!!")
        {

        }
    }
}
