using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public interface ISimpleDaemon : IDisposable
    {
        int MaxTryFailCount { get; set; }
        Action<object> LogMessage { get; set; }
        Task<MessageResult> Start(TimeSpan loopSpan, Action loopAction, bool autoStopIfRunning = false);
        Task<MessageResult> StartTask(TimeSpan loopSpan, Func<Task> loopTask, bool autoStopIfRunning = false);
        Task<MessageResult> Stop();
    }


    public class SimpleDaemon : ISimpleDaemon
    {
        public SimpleDaemon()
        {
            LogMessage = DebugLogMessage;
            MaxTryFailCount = 3;
        }

        protected TimeSpan LoopSpan { get; set; }
        protected Action LoopAction { get; set; }
        protected Func<Task> LoopTask { get; set; }

        private CancellationTokenSource _cts = null;
        private readonly object _ctsLock = new object();

        public int MaxTryFailCount { get; set; }
        public Action<object> LogMessage { get; set; }

        public Task<MessageResult> Start(TimeSpan loopSpan, Action loopAction, bool autoStopIfRunning = false)
        {
            if (loopSpan == TimeSpan.Zero)
            {
                throw new ArgumentException(nameof(loopSpan) + " should not be zero");
            }

            if (loopAction == null)
            {
                throw new ArgumentNullException(nameof(loopAction));
            }

            lock (_ctsLock)
            {
                if (_cts != null)
                {
                    if (!autoStopIfRunning)
                    {
                        return Task.FromResult(MessageResult.Create(false, "Task is already running"));
                    }

                    LogMessage("Cancelling");
                    _cts.Cancel(false);
                    _cts.Dispose();
                    _cts = null;
                }
                _cts = new CancellationTokenSource();

                LoopSpan = loopSpan;
                LoopAction = loopAction;

                var guardTask = Task.Factory.StartNew(RunGuardLoop, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                return Task.FromResult(MessageResult.Create(true, "Task is running"));
            }
        }

        public Task<MessageResult> StartTask(TimeSpan loopSpan, Func<Task> loopTask, bool autoStopIfRunning = false)
        {
            if (loopSpan == TimeSpan.Zero)
            {
                throw new ArgumentException(nameof(loopSpan) + " should not be zero");
            }

            if (loopTask == null)
            {
                throw new ArgumentNullException(nameof(loopTask));
            }

            lock (_ctsLock)
            {
                if (_cts != null)
                {
                    if (!autoStopIfRunning)
                    {
                        return Task.FromResult(MessageResult.Create(false, "Task is already running"));
                    }

                    LogMessage("Cancelling");
                    _cts.Cancel(false);
                    _cts.Dispose();
                    _cts = null;
                }
                _cts = new CancellationTokenSource();

                LoopSpan = loopSpan;
                LoopTask = loopTask;

                var guardTask = Task.Factory.StartNew(RunGuardLoop, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                return Task.FromResult(MessageResult.Create(true, "Task is running"));
            }
        }

        public Task<MessageResult> Stop()
        {
            lock (_ctsLock)
            {
                if (_cts == null)
                {
                    return Task.FromResult(MessageResult.Create(true, "Task is not running"));
                }

                LogMessage("Cancelling");
                _cts.Cancel(false);
                _cts.Dispose();
                _cts = null;
                return Task.FromResult(MessageResult.Create(true, "Task is stopping"));
            }
        }

        private void RunGuardLoop()
        {
            int errorCount = 0;
            while (true)
            {
                lock (_ctsLock)
                {
                    if (_cts == null)
                    {
                        LogMessage("NotStarted");
                        break;
                    }
                    if (_cts.IsCancellationRequested)
                    {
                        LogMessage("Cancelled");
                        break;
                    }
                }
                
                try
                {
                    LoopAction?.Invoke();
                    LoopTask?.Invoke().Wait();
                }
                catch (Exception e)
                {
                    errorCount++;
                    if (errorCount > MaxTryFailCount)
                    {
                        LogMessage(string.Format("fail {0} more then max: {1}, exit",errorCount, MaxTryFailCount));
                        break;
                    }
                    LogMessage(string.Format("fail time: {0}/{1}, ex:{2}", errorCount, MaxTryFailCount, e.Message));
                }

                Task.Delay(LoopSpan).Wait();
            }
        }

        public void Dispose()
        {
            LogMessage("Disposing");
            lock (_ctsLock)
            {
                if (_cts == null)
                {
                    return;
                }
            }

            LogMessage("Cancelling");
            _cts.Cancel(false);
            _cts.Dispose();
            _cts = null;

            Task.Delay(LoopSpan).Wait();
        }

        private void DebugLogMessage(object message)
        {
            Debug.WriteLine("[SimpleDaemon] => " + message);
        }
        
        #region for di extensions

        public static Func<ISimpleDaemon> Resolve { get; set; } = () => new SimpleDaemon();

        #endregion
    }
}
