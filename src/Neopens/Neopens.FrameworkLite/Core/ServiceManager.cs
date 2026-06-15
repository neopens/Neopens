using Neopens.FrameworkLite.Events;
using Neopens.FrameworkLite.Exceptions;
using Neopens.FrameworkLite.Extensions;
using Neopens.FrameworkLite.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace Neopens.FrameworkLite.Core
{
    /// <summary>
    /// 业务服务管理器
    /// </summary>
    public sealed class ServiceManager:BasicService<ServiceManager>
    {
        /// <summary>
        /// 已加载的业务模块
        /// </summary>
        private ConcurrentDictionary<string, BusinessService> _loadedServices = new ConcurrentDictionary<string, BusinessService>();

        /// <summary>
        /// 待监听事件
        /// </summary>
        private ConcurrentDictionary<string, FrameworkEventTable> _pendingListenerEvents = new ConcurrentDictionary<string, FrameworkEventTable>();

        /// <summary>
        /// 待发送消息
        /// </summary>
        private ConcurrentDictionary<string, ConcurrentStack<MessageParams>> _pendingSendMsg = new ConcurrentDictionary<string, ConcurrentStack<MessageParams>>();


        public IEventListener GetEvent(string serviceName,string eventName)
        {
            ThrowIfUninitialized();

            if (TryGetService(serviceName,out var service))
            { 
                return service.GetEvent(eventName);
            }

            var target = _pendingListenerEvents.GetOrAdd(serviceName,x=> new FrameworkEventTable());

            return target.GetEvent(eventName);           
        }

        public async Task<IActionResult> SendMessageAsync(string serviceName, string methodName, params object[] args)
        {
            var result = new ServiceActionResult<object>() { State = ActionState.NotCreated };

            try
            {
                ThrowIfUninitialized();
            }
            catch (Exception ex)
            {
                result.State = ActionState.Uninitialized;
                result.Message = ex.Message;
                return result;
            }

            if (!TryGetService(serviceName, out var service))
            {
                SendMessageIfServiceNotLoad(serviceName, methodName, args);

                result.State = ActionState.NotCreated;

                result.Message = $"{serviceName} not loaded, message queued";

                return result;
            }
           
            return await InvokeMethodAsync<object>(service, methodName, args);
        }

        public async Task<IActionResult<TContent>> SendMessageAsync<TContent>(string serviceName, string methodName, params object[] args)
        {
            var result = new ServiceActionResult<TContent>() { State = ActionState.NotCreated };

            try
            {
                ThrowIfUninitialized();
            }
            catch (Exception ex)
            {
                result.State = ActionState.Uninitialized;
                result.Message = ex.Message;
                return result;
            }

            if (!TryGetService(serviceName, out var service))
            {
                SendMessageIfServiceNotLoad(serviceName, methodName, args);

                result.State = ActionState.NotCreated;

                result.Message = $"{serviceName} not loaded, message queued";

                return result;
            }

            return await InvokeMethodAsync<TContent>(service, methodName, args); ;
        }

        public IActionResult SendMessage(string serviceName, string methodName, params object[] args)
        {
            var result = new ServiceActionResult<object>() { State = ActionState.NotCreated };

            try
            {
                ThrowIfUninitialized();
            }
            catch (Exception ex)
            {
                result.State = ActionState.Uninitialized;
                result.Message = ex.Message;
                return result;
            }

            if (!TryGetService(serviceName, out var service))
            {
                SendMessageIfServiceNotLoad(serviceName, methodName, args);

                result.State = ActionState.NotCreated;

                result.Message = $"{serviceName} not loaded, message queued";

                return result;
            }

            var task = InvokeMethodAsync<object>(service, methodName, args);
            if (task.Wait(10000)) return task.Result;

            return new ServiceActionResult<object>
            {
                State = ActionState.Timeout,
                Message = $"Timeout after {10000}ms"
            };
            /*
            return InvokeMethodAsync<object>(service, methodName, args)
                   .ConfigureAwait(false)
                   .GetAwaiter()
                   .GetResult();
            */
        }

        public IActionResult<T> SendMessage<T>(string serviceName, string methodName, params object[] args)
        {
            var result =  SendMessage(serviceName, methodName, args);

            ActionState state = (ActionState)result.State;

            T content = result.Content is null?default(T):(T)result.Content;

            return new ServiceActionResult<T>()
            {
                State = state,
                Message = result.Message,
                Content = content,
             };
        }

        public void Register(string assemblyFile, string typeName, string serviceName = "", params object[] args)
        {
            string name = string.IsNullOrEmpty(serviceName) ? typeName : serviceName;

            if (ServiceIsLoaded(name)) return;

            var assembly = Assembly.LoadFrom(assemblyFile);

           var type = assembly.GetType(typeName) ?? assembly.GetTypes().FirstOrDefault(x => x.Name == typeName);

            if (type is null) 
            {
                this.Warn($"未找到类型{typeName}，无法注册服务{name}");
                return;
            }

            var objHandle = Activator.CreateInstance(type);

            BusinessService business = objHandle as BusinessService;

            if (business is null)
            {
               this.Warn($"the {name}({assemblyFile}/{typeName}) not found!!!");

                return;
            }

            CreateService(name, business, args);
        }

        /// <summary>
        /// 注册业务服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        public void Register<TService>(string serviceName = "", params object[] args) where TService : BusinessService, new()
        {
            ThrowIfUninitialized();

            Type type = typeof(TService);

            string name = string.IsNullOrEmpty(serviceName) ? type.Name : serviceName;

            if (ServiceIsLoaded(name)) return;

            BusinessService business = Activator.CreateInstance(type, true) as BusinessService;

            if (business is null)
            {
                this.Warn($"the {name}({type.FullName}) not found!!!");

                return;
            }

            CreateService(name, business, args);
        }

        public sealed override void Release(object arg)
        {
            if(!IsInitialized) return;

            try
            {
                ReleaseServices(arg);

                ClearPendingSendMessages();

                ClearPendingListenerEvents();
            }
            catch (Exception ex)
            {
                this.Error($"{nameof(ServiceManager)} Released Exception !!",ex);
            }
            finally 
            {
                base.Release(arg);
            }       
        }

 
        private bool TryGetService(string serviceName, out BusinessService service) 
        {
           return _loadedServices.TryGetValue(serviceName, out service);
        }

        private void CreateService(string serviceName,BusinessService business,params object[] args)
        {    
            if (_pendingListenerEvents.TryRemove(serviceName, out var eventTable)) 
            {
                business.SetEventTable(eventTable);
            }

            business.Create(args);

            if (_pendingSendMsg.TryRemove(serviceName, out var msgStack))            
            {
                while (msgStack.TryPop(out var msg)) 
                {
                    try
                    {
                        business.ExecuteMethod(msg.method, msg.args);
                    }
                    catch (Exception ex)
                    {
                        this.Warn($"{serviceName}.{msg.method} Execute Exception !!", ex);
                    }
                }
            }
   
            _loadedServices.TryAdd(serviceName, business);
        }

        private bool ServiceIsLoaded(string serviceName)
        {
            return _loadedServices.ContainsKey(serviceName);
        }

        private async Task<ServiceActionResult<TContent>> InvokeMethodAsync<TContent>(BusinessService service, string methodName, params object [] args)
        {
            ServiceActionResult<TContent> result = new ServiceActionResult<TContent>();

            try
            {
                var executeResult = service?.ExecuteMethod(methodName, args);

                result.State = ActionState.Success;              

                if (executeResult is Task<TContent> t1)
                {
                    result.Content = await t1.ConfigureAwait(false);
                }
                else if (executeResult is Task t2)
                {
                    await t2.ConfigureAwait(false);
                    result.Message = "return type no found";
                }
                else
                {
                    try
                    {
                        result.Content = (TContent)executeResult;
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"return type mismatch {ex.Message}";
                    }
                }
            }
            catch (MethodFilteredException filteredEx)
            {
                result.State = ActionState.Filtered;
                result.Message = filteredEx.Message;
            }
            catch (MethodNotFoundException notFoundEx)
            {
                result.State = ActionState.NotFound;
                result.Message = notFoundEx.Message;
            }
            catch (Exception ex)
            {
                result.State = ActionState.Exception;
                result.Message = ex.Message;
            }

            return result;
        }

        private void SendMessageIfServiceNotLoad(string serviceName, string methodName, params object [] args)
        {
           var msgList =  _pendingSendMsg.GetOrAdd(serviceName, x=>new ConcurrentStack<MessageParams>());

            msgList.Push(new MessageParams(methodName, args));
        }

        private void ReleaseServices(object arg) 
        {
            if (_loadedServices is null) return;

            var services = _loadedServices.Values.ToArray();

            foreach (var service in services)
            {
                service.Release(arg);
            }

            _loadedServices.Clear();
        }

        private void ClearPendingSendMessages()
        {
            if (_pendingSendMsg is null || _pendingSendMsg.Count <= 0) return;
            _pendingSendMsg.Clear();
        }

        private void ClearPendingListenerEvents()
        {
            if (_pendingListenerEvents is null || _pendingListenerEvents.Count <= 0) return;
            _pendingListenerEvents.Clear();
        }


        struct MessageParams 
        {
           public string method;
           public object[] args;
            public MessageParams(string method, object[] args)
            {
                this.method = method;
                this.args = args;
            }
        }
    }

}
