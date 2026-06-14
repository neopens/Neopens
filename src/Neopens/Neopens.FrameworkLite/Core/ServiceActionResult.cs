using Neopens.FrameworkLite.Interfaces;
using System;

namespace Neopens.FrameworkLite.Core
{
    internal enum ActionState
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 业务失败
        /// </summary>
        Failed = 100,
        /// <summary>
        /// 未找到
        /// </summary>
        NotFound = 101,
        /// <summary>
        /// 未授权
        /// </summary>
        NotAuthorized = 102,


        /// <summary>
        /// 未初始化
        /// </summary>
        Uninitialized = 200,
        /// <summary>
        /// 未实现
        /// </summary>
        NotImplemented = 201,
        /// <summary>
        /// 已释放
        /// </summary>
        Disposed = 202,

        /// <summary>
        /// 已被过滤
        /// </summary>
        Filtered = 300,
        /// <summary>
        /// 异常
        /// </summary>
        Exception = 301,
        /// <summary>
        /// 超时
        /// </summary>
        Timeout = 302,

        /// <summary>
        /// 未创建
        /// </summary>
        NoCreated = 400,
        /// <summary>
        /// 已存在
        /// </summary>
        AlreadyExists = 401,
    }

    internal class ServiceActionResult : ActionResult
    {
        public new ActionState State
        {
            get => (ActionState)base.State;
            set => base.State = (int)value;
        }
    }

    internal class ServiceActionResult<T> : ActionResult<T>
    {
        public new ActionState State
        {
            get => (ActionState)base.State;
            set => base.State = (int)value;
        }       
    }
}


