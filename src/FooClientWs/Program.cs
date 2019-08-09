using System.ServiceProcess;
using FooClientWs.Services;

namespace FooClientWs
{
    static class Program
    {
        static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new DaemonService()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
