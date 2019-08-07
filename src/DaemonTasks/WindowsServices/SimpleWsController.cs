using System;

namespace Common.WindowsServices
{
    public interface ISimpleWsController
    {
        string ServiceName { get; set; }
        string DisplayName { get; set; }
        string ExePath { get; set; }

        string GetServiceState();
        MessageResult TryInstall();
        MessageResult TryUninstall();
        MessageResult TryStart();
        MessageResult TryStop();
    }

    public class SimpleWsController : ISimpleWsController
    {
        public SimpleWsController(string exePath, string serviceName, string displayName = null)
        {
            if (exePath == null)
            {
                throw new ArgumentNullException(nameof(exePath));
            }

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = serviceName;
            }

            ServiceName = serviceName;
            DisplayName = displayName;
            ExePath = exePath;
        }

        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string ExePath { get; set; }

        public string GetServiceState()
        {
            var serviceStatus = ServiceInstaller.GetServiceState(ServiceName);
            return serviceStatus.ToString();
        }

        public MessageResult TryInstall()
        {
            var failResult = MessageResult.MethodResult(nameof(TryInstall), false);
            var successResult = MessageResult.MethodResult(nameof(TryInstall), true);

            try
            {
                var installed = ServiceInstaller.ServiceIsInstalled(ServiceName);
                if (installed)
                {
                    var message = string.Format("{0} already installed!", ServiceName);
                    successResult.Message = message;
                    return successResult;
                }

                ServiceInstaller.Install(ServiceName, DisplayName, ExePath);
                return successResult;
            }
            catch (Exception e)
            {
                failResult.Message = failResult.Message + " => " + e.Message;
                return failResult;
            }
        }

        public MessageResult TryUninstall()
        {
            var failResult = MessageResult.MethodResult(nameof(TryUninstall), false);
            var successResult = MessageResult.MethodResult(nameof(TryUninstall), true);

            try
            {
                var installed = ServiceInstaller.ServiceIsInstalled(ServiceName);
                if (!installed)
                {
                    var message = string.Format("{0} already uninstalled!", ServiceName);
                    successResult.Message = message;
                    return successResult;
                }

                ServiceInstaller.Uninstall(ServiceName);
                return successResult;
            }
            catch (Exception e)
            {
                failResult.Message = failResult.Message + " => " + e.Message;
                return failResult;
            }
        }

        public MessageResult TryStart()
        {
            var failResult = MessageResult.MethodResult(nameof(TryStart), false);
            var successResult = MessageResult.MethodResult(nameof(TryStart), true);

            try
            {
                var serviceState = ServiceInstaller.GetServiceState(ServiceName);
                if (serviceState == ServiceState.NotFound)
                {
                    failResult.Message = string.Format("{0} not installed!", ServiceName);
                    return failResult;
                }

                if (serviceState == ServiceState.Running || serviceState == ServiceState.StartPending)
                {
                    successResult.Message = string.Format("{0} is already running!", ServiceName);
                    return successResult;
                }

                ServiceInstaller.StartService(ServiceName);
                return successResult;
            }
            catch (Exception e)
            {
                failResult.Message = failResult.Message + " => " + e.Message;
                return failResult;
            }
        }

        public MessageResult TryStop()
        {
            var failResult = MessageResult.MethodResult(nameof(TryStart), false);
            var successResult = MessageResult.MethodResult(nameof(TryStart), true);

            try
            {
                var serviceState = ServiceInstaller.GetServiceState(ServiceName);
                if (serviceState == ServiceState.NotFound)
                {
                    successResult.Message = string.Format("{0} not installed!", ServiceName);
                    return successResult;
                }

                if (serviceState == ServiceState.Stopped || serviceState == ServiceState.StopPending)
                {
                    successResult.Message = string.Format("{0} is already stopped!", ServiceName);
                    return successResult;
                }

                ServiceInstaller.StopService(ServiceName);
                return successResult;
            }
            catch (Exception e)
            {
                failResult.Message = failResult.Message + " => " + e.Message;
                return failResult;
            }
        }

        #region for easy use

        public static SimpleWsController Create(string exePath, string serviceName, string displayName = null)
        {
            return new SimpleWsController(exePath, serviceName, displayName);
        }

        #endregion
    }
}
