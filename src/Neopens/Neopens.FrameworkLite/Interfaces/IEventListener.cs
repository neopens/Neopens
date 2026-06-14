using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neopens.FrameworkLite.Interfaces
{
    public interface IEventListener
    {
        /// <summary>
        /// 当前监听的事件数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 添加监听事件
        /// </summary>
        /// <param name="action"></param>
        void AddListener(Action<object> action);
    }
}
