using System;
using System.IO;
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

        public MessageResult TryGetStatus()
        {
            var serviceInfo = Config.ServiceInfo;
            var serviceName = serviceInfo.ServiceName;
            var serviceState = GetServiceState(serviceName);
            if (serviceState == ServiceState.NotFound)
            {
                return AppendLogsAndResult(false, string.Format("{0} not installed!", serviceName));
            }
            return AppendLogsAndResult(true, string.Format("{0} state: {1}", serviceName, serviceState));
        }

        public MessageResult TryInstall()
        {
            var serviceInfo = Config.ServiceInfo;
            var serviceName = serviceInfo.ServiceName;
            var serviceFriendlyName = serviceInfo.ServiceFriendlyName;
            var servicePath = serviceInfo.ServicePath;
            var exePath = Path.GetFullPath(servicePath);
            if (!File.Exists(exePath))
            {
                return AppendLogsAndResult(false, string.Format("{0} is not found!", exePath));
            }

            var serviceState = GetServiceState(serviceName);
            if (serviceState != ServiceState.NotFound)
            {
                return AppendLogsAndResult(true, string.Format("{0} is already installed!", exePath));
            }

            //ServiceInstaller.Install(serviceName, serviceFriendlyName, servicePath);
            ServiceInstaller.InstallAndStart(serviceName, serviceFriendlyName, servicePath);

            GetServiceState(serviceName);
            return AppendLogsAndResult(true, string.Format("{0} install completed!", serviceName));
        }

        public MessageResult TryUninstall()
        {
            var serviceInfo = Config.ServiceInfo;
            var serviceName = serviceInfo.ServiceName;
            var serviceState = GetServiceState(serviceName);
            if (serviceState == ServiceState.NotFound)
            {
                return AppendLogsAndResult(true, string.Format("{0} not installed!", serviceName));
            }

            ServiceInstaller.Uninstall(serviceName);
            GetServiceState(serviceName);
            return AppendLogsAndResult(true, string.Format("{0} uninstall completed!", serviceName));
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
            return AppendLogsAndResult(true, string.Format("{0} start completed!", serviceName));
        }

        public MessageResult TryStop()
        {
            var serviceInfo = Config.ServiceInfo;
            var serviceName = serviceInfo.ServiceName;
            var serviceState = GetServiceState(serviceName);
            if (serviceState == ServiceState.NotFound)
            {
                return AppendLogsAndResult(true, string.Format("{0} not installed!", serviceName));
            }

            if (serviceState == ServiceState.Stopped || serviceState == ServiceState.StopPending)
            {
                return AppendLogsAndResult(true, string.Format("{0} is stopping!", serviceName));
            }

            ServiceInstaller.StopService(serviceName);
            GetServiceState(serviceName);
            return AppendLogsAndResult(true, string.Format("{0} stop completed!", serviceName));
        }
        
        private ServiceState GetServiceState(string serviceName)
        {
            var serviceStatus = ServiceInstaller.GetServiceState(serviceName);
            AppendLogs(string.Format("{0} current state: {1}", serviceName, serviceStatus));
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
