using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neopens.FrameworkLite.Exceptions
{
    internal class UninitializedException:Exception
    {
        public UninitializedException(object sender):base($"{sender.GetType().Name} Uninitialized")
        {

        }
    }
}
