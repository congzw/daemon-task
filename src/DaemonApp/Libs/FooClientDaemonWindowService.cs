using System;
using System.ServiceProcess;
using Common;

namespace DaemonApp.Libs
{
    partial class FooClientDaemonWindowService : ServiceBase
    {
        public FooClientDaemonWindowService()
        {
            InitializeComponent();
            MyInitializeComponent();
        }

        private void MyInitializeComponent()
        {
            this.ServiceName = "FooClientDaemon";
            var simpleProcessFactory = SimpleProcessFactory.Resolve();
            var fooClientInfo = new SimpleProcessInfo()
                {ProcessName = "FooClient", ExePath = "FooClient.exe", ExeArgs = ""};
            simpleProcessFactory.GetOrCreate(fooClientInfo);

            Daemon = SimpleDaemon.Resolve();
        }

        private ISimpleLog _log;

        public ISimpleLog Log
        {
            set => _log = value ?? throw new ArgumentNullException(nameof(value));
            get => _log ?? (_log = SimpleLogFactory.Resolve().CreateLogFor(this));
        }


        public ISimpleDaemon Daemon { get; set; }

        protected override void OnStart(string[] args)
        {
            Log.LogInfo("OnStart".AppendDate());

            //Action action = () =>
            //{
            //};

            //new SimpleProcessRunner()

            //Daemon.Start(TimeSpan.FromSeconds(1), action);

        }

        protected override void OnStop()
        {
            Log.LogInfo("OnStop".AppendDate());
        }
    }

    public class FooClientDaemon
    {

    }
}
