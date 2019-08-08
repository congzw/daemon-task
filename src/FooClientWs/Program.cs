using System.ServiceProcess;
using FooClientWs.Services;

namespace FooClientWs
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new MockService()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
