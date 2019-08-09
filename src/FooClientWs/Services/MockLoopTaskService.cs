using System;
using System.ServiceProcess;
using System.Threading;
using Common;

namespace FooClientWs.Services
{
    partial class MockLoopTaskService : ServiceBase
    {
        public MockLoopTaskService()
        {
            InitializeComponent();

            this.ServiceName = nameof(MockLoopTaskService);
            LoopTask = new SimpleLoopTask();
            Init(LoopTask);
        }

        private void Init(SimpleLoopTask loopTask)
        {
            loopTask.LoopSpan = TimeSpan.FromSeconds(3);
            loopTask.LoopAction = () =>
            {
                LoopTask.Log.LogInfo(string.Format("demo long running task is running at {0} in thread {1}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), Thread.CurrentThread.ManagedThreadId));
            };
            loopTask.AfterExitLoopAction = () =>
            {
                LoopTask.Log.LogInfo(string.Format(">>> demo long running task is stopping at {0} in thread {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), Thread.CurrentThread.ManagedThreadId));
            };
        }

        public SimpleLoopTask LoopTask { get; set; }

        protected override void OnStart(string[] args)
        {
            LoopTask.Start();
        }

        protected override void OnStop()
        {
            LoopTask.Stop();
        }
    }
}
