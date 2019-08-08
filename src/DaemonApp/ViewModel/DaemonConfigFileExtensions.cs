using System.Threading.Tasks;
using Common;

namespace DaemonApp.ViewModel
{
    public static class DaemonConfigFileExtensions
    {
        public static async Task<DaemonConfig> LoadDaemonConfig(this ISimpleConfigFile simpleConfigFile)
        {
            var config = await simpleConfigFile.ReadFile<DaemonConfig>(null).ConfigureAwait(false);
            if (config == null)
            {
                config = new DaemonConfig();
                await simpleConfigFile.SaveFile(config).ConfigureAwait(false);
            }
            return config;
        }
    }
}