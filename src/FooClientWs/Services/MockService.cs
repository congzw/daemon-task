using System;
using System.ServiceProcess;
using Common;

namespace FooClientWs.Services
{
    partial class MockService : ServiceBase
    {
        public MockService()
        {
            InitializeComponent();
        }
        
        private ISimpleLog _log;

        public ISimpleLog Log
        {
            set => _log = value ?? throw new ArgumentNullException(nameof(value));
            get => _log ?? (_log = SimpleLogFactory.Resolve().CreateLogFor(this));
        }
        
        protected override void OnStart(string[] args)
        {
            Log.LogInfo(" OnStart at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }

        protected override void OnStop()
        {
            Log.LogInfo(" OnStop at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
        }
    }
}
