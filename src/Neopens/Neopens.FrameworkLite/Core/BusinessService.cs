using Neopens.FrameworkLite.Attributes;
using Neopens.FrameworkLite.Events;
using Neopens.FrameworkLite.Exceptions;
using Neopens.FrameworkLite.Extensions;
using Neopens.FrameworkLite.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Neopens.FrameworkLite.Core
{
    public abstract class BusinessService : IService
    {
        private readonly object _eventTableLocker = new object();

        private const BindingFlags MethodBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        /// <summary>
        /// 函数缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, MethodInfo> _methodCache = new ConcurrentDictionary<string, MethodInfo>();


        private FrameworkEventTable _eventTable = null;

        public BusinessService()
        {
            SetEventTable(new FrameworkEventTable());
        }

        internal void Create(params object[] args) 
        {
            try
            {
                OnCreating(args);

                OnCreated(args);

                InvokeEvent("OnCreated", this);

                this.Debug($"the {this.GetType().Name} created");
            }
            catch (Exception ex)
            {
                this.Error($"{GetType().Name} Create Exception", ex);
                throw;
            }      
        }

        protected virtual void OnCreating(object[] args)
        {
        }

        protected virtual void OnCreated(object[] args)
        {
        }

        internal void SetEventTable(FrameworkEventTable table) 
        {
            lock (_eventTableLocker) 
            {
                ReleaseEventTable();

                _eventTable = table ?? new FrameworkEventTable();
            }
        }

        internal object ExecuteMethod(string methodName, params object[] args)
        {
            MethodInfo method = FindMethod(methodName, MethodBindingFlags, args);

            return ExecuteMethod(method, args);
        }

        private MethodInfo FindMethod(string methodName, BindingFlags bindingFlags, params object[] args) 
        {
            Type[] paramTypes = args?.Select(a => a?.GetType() ?? typeof(object)).ToArray() ?? Type.EmptyTypes;

            var methodKey = BuildCacheKey(GetType(),methodName, paramTypes);

            return _methodCache.GetOrAdd(methodKey, key => GetMethod(methodName, bindingFlags, paramTypes)); 
        }

        private MethodInfo GetMethod(string methodName, BindingFlags bindingFlags, Type[] paramTypes)
        {
            MethodInfo method = this.GetType().GetMethod(methodName, bindingFlags, null, paramTypes, null);

            if (method is null)
            {
                throw new MethodNotFoundException($"{this.GetType().FullName} {methodName}({string.Join(",", paramTypes.Select(x => x.Name))})");
            }
            return method;
        }

        private object ExecuteMethod(MethodInfo method,params object[] args)
        {
            var filters = method.GetCustomAttributes<ActionFilterAttribute>();

            foreach (var item in filters)
            {
                if (!item.CanExecute()) 
                {
                    this.Warn($"the {item.GetType().FullName} filter blocked the method execution");
                    throw new MethodFilteredException(item);
                }
            }

            try
            {
                return method.Invoke(this, args);
            }
            catch (TargetInvocationException ex)
            {
                this.Error($"the {method.Name} invoke exception", ex.InnerException ?? ex);
                throw ex.InnerException ?? ex;
            }
        }

        internal IEventListener GetEvent(string name) 
        {
            lock (_eventTableLocker) 
            {
                return _eventTable.GetEvent(name);
            }
        }

        protected void InvokeEvent(string eventName, object arg) 
        {
            FrameworkEvent targetEvent = null;

            lock (_eventTableLocker) 
            {
                targetEvent = _eventTable.GetEvent(eventName);
            }

            targetEvent?.Invoke(arg);
        }

        public void Release(object arg)
        {
            try
            {
                OnReleasing(arg);

                OnReleased(arg);

                InvokeEvent("OnReleased", this);

                this.Debug($"the {this.GetType().Name} released");
            }
            catch (Exception ex)
            {
                this.Error($"{GetType().Name} Release Exception", ex);
            }
            finally 
            {
                ReleaseEventTable();
            }
       
        }

        private void ReleaseEventTable() 
        {
            if (_eventTable is null) return;

            FrameworkEventTable table = null;

            lock (_eventTableLocker) 
            {
                table = _eventTable;
                _eventTable = null;
            }

            table?.Dispose(); 
        }

        protected virtual void OnReleased(object arg)
        {
        }

        protected virtual void OnReleasing(object arg)
        {
        }

        private static string BuildCacheKey(Type targetType, string methodName, Type[] paramTypes)
        {
            if (paramTypes is null || paramTypes.Length <= 0)
            {
                return $"{targetType.FullName}.{methodName}()";
            }

            var @params = new string[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                var paramType = paramTypes[i];

                @params[i] = $"{paramType.FullName?? paramType.Name} @{paramType.Name}";
            }
            string paramStrs = string.Join(",", @params);
            return $"{targetType.FullName}.{methodName}({paramStrs})";
        }
    }
}
