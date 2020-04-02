using System;
using System.Threading;

namespace Caching
{
    public abstract class TimedThread : IDisposable
    {
        private int _sleepTime;
        private Timer _sleepTimer;
        private readonly object _selfRunningLock;
        private bool _disposed;
        private bool _stopRequested;
        private bool _initialized;
        private ManualResetEvent _manualResetEvent;

        protected TimedThread()
        {
            _selfRunningLock = new object();
            _manualResetEvent = new ManualResetEvent(false);
            _initialized = false;
        }

        protected abstract bool DoWork();

        protected void InitializeBase(int interval)
        {
            if (interval <= 0)
                throw _exception_NotValidInterval;
            else
            {
                _sleepTime = interval;
                _initialized = true;
            }
        }

        public void Start()
        {
            if (_initialized)
            {
                _sleepTimer = new Timer(CallBack, null, 0, Timeout.Infinite);
            }
            else
            {
                throw _exception_NotInitialized;
            }
        }

        private void CallBack(object o)
        {
            DoCallBackLogic();
        }

        private void DoCallBackLogic()
        {
            if (_stopRequested)
            {
                SetStopped();
            }
            else
            {
                PerformDoWork();
            }
        }

        private void PerformDoWork()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            try
            {
                _sleepTimer.Change(-1, -1);
            }
            catch { }
            bool moreWorkToDo;
            lock (_selfRunningLock)
            {
                moreWorkToDo = DoWork();
            }

            SetNextScheduledCallBack(moreWorkToDo);
        }

        private void SetNextScheduledCallBack(bool moreWorkToDo)
        {
            if (!_stopRequested)
            {
                if (!_disposed)
                {
                    _sleepTimer.Change(moreWorkToDo ? 0 : _sleepTime, Timeout.Infinite);
                }
            }
            else
            {
                SetStopped();
            }
        }

        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            lock (_selfRunningLock)
            {
                SetStopped();
            }
            _stopRequested = true;
        }

        private void SetStopped()
        {
            if (!_disposed)
            {
                _sleepTimer.Dispose();
                _manualResetEvent.Set();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_sleepTimer != null)
                    { _sleepTimer.Dispose(); }
                    _manualResetEvent.Dispose();
                    _disposed = true;
                }
            }
        }

        private Exception _exception_NotInitialized = new Exception("Database cannot be started.");
        private Exception _exception_NotValidInterval = new Exception("Interval is not valid.");
    }
}
