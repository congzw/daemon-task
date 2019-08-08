using System.Threading.Tasks;
using Common;

namespace DaemonApp.ViewModel
{
    public static class MyConfigFileExtensions
    {
        public static async Task<MyConfig> LoadDaemonConfig(this ISimpleConfigFile simpleConfigFile)
        {
            var config = await simpleConfigFile.ReadFile<MyConfig>(null).ConfigureAwait(false);
            if (config == null)
            {
                config = new MyConfig();
                config.ProcessInfos.Add(new SimpleProcessInfo() { ProcessName = "FooClient", ExePath = "FooClient.exe", ExeArgs = null });
                config.ServiceInfo = new WindowServiceInfo(){ServiceName = "MockService", ServicePath = "FooClientWs.exe", ServiceFriendlyName = "000-MockService" };
                await simpleConfigFile.SaveFile(config).ConfigureAwait(false);
            }
            return config;
        }
    }
}