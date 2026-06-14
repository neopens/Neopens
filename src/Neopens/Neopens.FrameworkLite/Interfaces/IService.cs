using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neopens.FrameworkLite.Interfaces
{
    public interface IService
    {
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="arg"></param>
        void Release(object arg);
    }
}
