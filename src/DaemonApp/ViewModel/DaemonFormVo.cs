using System;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.WindowsServices;

namespace DaemonApp.ViewModel
{
    public class DaemonFormVo
    {

        private ISimpleLog _log;

        public ISimpleLog Log
        {
            set => _log = value ?? throw new ArgumentNullException(nameof(value));
            get => _log ?? (_log = SimpleLogFactory.Resolve().CreateLogFor(this));
        }

        public MyConfig Config { get; set; }

        public async Task LoadConfig()
        {
            var simpleConfigFile = SimpleConfigFactory.ResolveFile();
            Config = await simpleConfigFile.LoadDaemonConfig().ConfigureAwait(false);
        }

        public MessageResult TryInstall()
        {
            return AppendLogsAndResult(true, "ToDo");
        }

        public MessageResult TryUninstall()
        {
            return AppendLogsAndResult(true, "ToDo");
        }

        public MessageResult TryStart()
        {
            var serviceInfo = Config.ServiceInfo;
            var serviceName = serviceInfo.ServiceName;
            var serviceState = GetServiceState(serviceName);
            if (serviceState == ServiceState.NotFound)
            {
                return AppendLogsAndResult(false, string.Format("{0} not installed!", serviceName));
            }
            
            if (serviceState == ServiceState.Running || serviceState == ServiceState.StartPending)
            {
                return AppendLogsAndResult(true, string.Format("{0} is already running!", serviceName));
            }

            ServiceInstaller.StartService(serviceName);

            return AppendLogsAndResult(true, "ToDo");
        }
        public MessageResult TryStop()
        {
            return AppendLogsAndResult(true, "ToDo");
        }


        private ServiceState GetServiceState(string serviceName)
        {
            var serviceStatus = ServiceInstaller.GetServiceState(serviceName);
            AppendLogs("current service state: " + serviceStatus);
            return serviceStatus;
        }

        private void AppendLogs(object message)
        {
            Log.LogInfo(message);
        }

        private MessageResult AppendLogsAndResult(bool success, string message)
        {
            AppendLogs(message);
            return MessageResult.Create(success, message);
        }

    }
}
