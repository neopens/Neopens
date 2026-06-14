using System;
using System.Collections.Concurrent;

namespace Neopens.FrameworkLite.Events
{
    internal class FrameworkEventTable:IDisposable
    {
        private bool _isDisposed  = false;

        private readonly ConcurrentDictionary<string, FrameworkEvent> _eventTable = new ConcurrentDictionary<string, FrameworkEvent>();

        ~FrameworkEventTable() 
        {
            Dispose();
        }

        public FrameworkEvent GetEvent(string eventKey) 
        {
            ThrowIfDisposed();

            return _eventTable.GetOrAdd(eventKey,x=>new FrameworkEvent());
        }

        public void ClearEvents(string eventKey) 
        {
            ThrowIfDisposed();

            if (_eventTable.TryRemove(eventKey, out var target)) 
            {
                target.Clear();
            }            
        }

        public void Clear() 
        {
            ThrowIfDisposed();

            ClearAllEvents();
        }

        private void ClearAllEvents() 
        {
            if (_eventTable.Count <= 0) return;

            foreach (var item in _eventTable)
            {
                item.Value.Clear();
            }

            _eventTable.Clear();
        }


        public void Dispose()
        {
            if(_isDisposed) return;

            ClearAllEvents();

            _isDisposed = true;
        }

        private void ThrowIfDisposed() 
        {
            if (!_isDisposed) return;

            throw new ObjectDisposedException(this.GetType().Name);
        }
    }

}
