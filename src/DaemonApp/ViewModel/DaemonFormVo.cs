using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace DaemonApp.ViewModel
{
    public class DaemonFormVo
    {
        public string FormatProcessInfos(IList<SimpleProcessInfo> infos)
        {
            return infos.ToJson(true);
        }

        public async Task<DaemonConfig> LoadConfig()
        {
            var simpleConfigFile = SimpleConfigFactory.ResolveFile();
            var simpleConfig = await simpleConfigFile.ReadFile<DaemonConfig>(null).ConfigureAwait(false);
            if (simpleConfig == null)
            {
                simpleConfig = new DaemonConfig();
                //simpleConfig.AddOrUpdateProcessInfos(new List<SimpleProcessInfo>());
                await simpleConfigFile.SaveFile(simpleConfig).ConfigureAwait(false);
            }
            return simpleConfig;
        }

    }
}
