using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public class SimpleLoopTask : IDisposable
    {
        private CancellationTokenSource _cts = null;
        private readonly object _ctsLock = new object();

        public SimpleLoopTask()
        {
            MaxTryFailCount = 3;
            LoopSpan = TimeSpan.FromSeconds(3);
        }

        public TimeSpan LoopSpan { get; set; }
        public Action LoopAction { get; set; }
        public Action AfterExitLoopAction { get; set; }
        //public Func<Task> LoopTask { get; set; }
        //public Func<Task> AfterExitLoopTask { get; set; }
        public int MaxTryFailCount { get; set; }

        private ISimpleLog _log;

        public ISimpleLog Log
        {
            set => _log = value ?? throw new ArgumentNullException(nameof(value));
            get => _log ?? (_log = SimpleLogFactory.Resolve().CreateLogFor(this));
        }

        public Task<MessageResult> Start(bool autoStopIfRunning = false)
        {
            lock (_ctsLock)
            {
                if (_cts != null)
                {
                    if (!autoStopIfRunning)
                    {
                        return Task.FromResult(MessageResult.Create(false, "Task is already running"));
                    }
                    RunCancelLogic();
                }
                _cts = new CancellationTokenSource();
                var guardTask = Task.Factory.StartNew(RunGuardLoop, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                return Task.FromResult(MessageResult.Create(true, "Task is running"));
            }
        }

        public Task<MessageResult> Stop()
        {
            lock (_ctsLock)
            {
                var result = RunCancelLogic();
                return Task.FromResult(result);
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
                        Log.LogInfo("NotStarted");
                        break;
                    }
                    if (_cts.IsCancellationRequested)
                    {
                        Log.LogInfo("Cancelled");
                        break;
                    }
                }

                try
                {
                    LoopAction?.Invoke();
                    //LoopTask?.Invoke().Wait();
                }
                catch (Exception e)
                {
                    errorCount++;
                    if (errorCount > MaxTryFailCount)
                    {
                        Log.LogInfo(string.Format("fail {0} more then max: {1}, exit", errorCount, MaxTryFailCount));
                        break;
                    }
                    Log.LogInfo(string.Format("fail time: {0}/{1}, ex:{2}", errorCount, MaxTryFailCount, e.Message));
                }
                Task.Delay(LoopSpan).Wait();
            }
        }

        private MessageResult RunCancelLogic()
        {
            if (_cts == null)
            {
                return MessageResult.Create(true, "Task is not running");
            }

            Log.LogInfo("Cancelling");
            _cts.Cancel(false);
            _cts.Dispose();
            _cts = null;
            //AfterExitLoopTask?.Invoke().Wait();
            AfterExitLoopAction?.Invoke();
            return MessageResult.Create(true, "Task is stopping");
        }

        public void Dispose()
        {
            Log.LogInfo("Disposing");
            RunCancelLogic();
        }
    }
}