using Neopens.FrameworkLite.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neopens.FrameworkLite.Events
{
    internal class FrameworkEvent: IEventListener
    {
        private readonly object handlerLocker = new object();

        private Action<object> _handlers;

        public int Count
        {
            get
            {
                int count = 0;

                lock (handlerLocker)
                {
                    count = _handlers?.GetInvocationList()?.Length ?? 0;
                }
                return count;
            }
        }

        public FrameworkEvent()
        {
            Clear();           
        }

        public void AddListener(Action<object> action)
        {
            lock (handlerLocker) 
            {
                _handlers += action;               
            }            
        }

        public void Remove(Action<object> action)
        {
            lock (handlerLocker) 
            {
                _handlers -= action;               
            }
        }

        public void Clear() 
        {
            lock (handlerLocker) 
            {
                _handlers = new Action<object>(o => { });
            }            
        }

        public void Invoke(object arg)
        {
            Action<object> handlers = null;

            lock (handlerLocker)
            {
                handlers = _handlers;
            }

            handlers?.Invoke(arg);
        }
    }
}
