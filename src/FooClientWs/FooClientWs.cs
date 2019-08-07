using System;
using System.ServiceProcess;
using Common;

namespace FooClientWs
{
    public partial class FooClientWs : ServiceBase
    {
        public FooClientWs()
        {
            InitializeComponent();

            var info = SimpleProcessInfo.Create("FooClient", "FooClient.exe", null);
            var simpleProcessFactory = SimpleProcessFactory.Resolve();
            var simpleProcess = simpleProcessFactory.GetOrCreate(info);
            var runner = new SimpleProcessRunner(simpleProcess);
            var simpleDaemon = SimpleDaemon.Resolve();
            Daemon = new FooClientDaemon(simpleDaemon, runner);
        }

        private ISimpleLog _log;

        public ISimpleLog Log
        {
            set => _log = value ?? throw new ArgumentNullException(nameof(value));
            get => _log ?? (_log = SimpleLogFactory.Resolve().CreateLogFor(this));
        }

        public FooClientDaemon Daemon { get; set; }

        protected override void OnStart(string[] args)
        {
            Log.LogInfo("OnStart Begin");
            Daemon.TryStart().Wait();
            Log.LogInfo("OnStart End");
        }

        protected override void OnStop()
        {
            Log.LogInfo("TryStop Begin");
            Daemon.TryStop().Wait();
            Log.LogInfo("TryStop End");
        }
    }
}
