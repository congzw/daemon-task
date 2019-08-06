using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public interface ISimpleDaemon
    {
        Action<object> LogMessage { get; set; }
        Task<MessageResult> Start(TimeSpan loopSpan, Action loopAction, bool autoStopIfRunning = false);
        Task<MessageResult> Stop();
    }

    public class SimpleDaemon : ISimpleDaemon, IDisposable
    {
        public SimpleDaemon()
        {
            LogMessage = DebugLogMessage;
        }

        protected TimeSpan LoopSpan { get; set; }
        protected Action LoopAction { get; set; }
        
        private CancellationTokenSource _cts = null;
        private readonly object _ctsLock = new object();

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

                LoopAction();
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
