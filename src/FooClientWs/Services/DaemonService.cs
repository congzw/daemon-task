using System;
using System.ServiceProcess;
using System.Threading;
using Common;

namespace FooClientWs.Services
{
    public partial class DaemonService : ServiceBase
    {
        public DaemonService()
        {
            InitializeComponent();

            //var clientProcess = SimpleProcess.GetOrCreate(new SimpleProcessInfo()
            //{
            //    ProcessName = "FooClient",
            //    ExePath = "FooClient.exe",
            //    ExeArgs = null
            //});


            var jaegerProcess = SimpleProcess.GetOrCreate(new SimpleProcessInfo()
            {
                ProcessName = "jaeger-all-in-one",
                ExePath = "jaeger-all-in-one.exe",
                ExeArgs = "--collector.zipkin.http-port=9411"
            });

            var runner = new SimpleProcessRunner(jaegerProcess);
            LoopTask = new SimpleLoopTask();
            Init(LoopTask, runner);
        }

        private void Init(SimpleLoopTask loopTask, SimpleProcessRunner runner)
        {
            loopTask.LoopSpan = TimeSpan.FromSeconds(3);
            loopTask.LoopAction = () =>
            {
                runner.TryStart();
            };
            loopTask.AfterExitLoopAction = () =>
            {
                runner.TryStop();
            };
        }

        private ISimpleLog _log;

        public ISimpleLog Log
        {
            set => _log = value ?? throw new ArgumentNullException(nameof(value));
            get => _log ?? (_log = SimpleLogFactory.Resolve().CreateLogFor(this));
        }

        public SimpleLoopTask LoopTask { get; set; }

        protected override void OnStart(string[] args)
        {
            Log.LogInfo(string.Format("OnStart begin {0:yyyy-MM-dd HH:mm:ss:fff} in thread {1}", DateTime.Now, Thread.CurrentThread.ManagedThreadId));
            LoopTask.Start();
            Log.LogInfo(string.Format("OnStart end {0:yyyy-MM-dd HH:mm:ss:fff} in thread {1}", DateTime.Now, Thread.CurrentThread.ManagedThreadId));
        }

        protected override void OnStop()
        {
            Log.LogInfo(string.Format("OnStop begin {0:yyyy-MM-dd HH:mm:ss:fff} in thread {1}", DateTime.Now, Thread.CurrentThread.ManagedThreadId));
            LoopTask.Stop();
            Log.LogInfo(string.Format("OnStop end {0:yyyy-MM-dd HH:mm:ss:fff} in thread {1}", DateTime.Now, Thread.CurrentThread.ManagedThreadId));
        }
    }
}
