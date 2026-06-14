using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neopens.FrameworkLite.Attributes
{
    public abstract class ActionFilterAttribute:Attribute
    {
        public string Message { get;protected set; }

        public virtual bool CanExecute() 
        {
            return true;
        }
    }
}
