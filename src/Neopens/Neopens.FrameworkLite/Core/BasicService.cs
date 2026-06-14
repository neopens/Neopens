using Neopens.FrameworkLite.Exceptions;
using Neopens.FrameworkLite.Extensions;
using Neopens.FrameworkLite.Interfaces;
using System;

namespace Neopens.FrameworkLite.Core
{
    public abstract class BasicService<T> : IService where T : BasicService<T>, new()
    {
        private static readonly Lazy<T> _lazy = new Lazy<T>(() => new T());
        public static T Instance => _lazy.Value;

        public bool IsInitialized { get; private set; }

        protected BasicService() { }    

        public void Initialize(object arg)
        {
            if (IsInitialized) 
            {
                this.Warn($"{this.GetType().Name} is Initialized!!");
                return;
            }

            OnInitializing(arg);

            IsInitialized = true;

            OnInitialized(arg);

            this.Debug($"{this.GetType().Name} is Initialize Success!!");
        }

        protected virtual void OnInitializing(object arg)
        {
        }

        protected virtual void OnInitialized(object arg)
        {
        }

        public virtual void Release(object arg)
        {
            this.Debug($"{this.GetType().Name} is Released!!");
            IsInitialized = false;
        }


        protected void ThrowIfUninitialized()
        {
            if (IsInitialized) return;

            throw new UninitializedException(this);
        }
    }

}
