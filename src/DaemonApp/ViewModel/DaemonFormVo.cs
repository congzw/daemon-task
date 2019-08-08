using System.Threading.Tasks;
using Common;

namespace DaemonApp.ViewModel
{
    public class DaemonFormVo
    {
        public DaemonConfig Config { get; set; }

        public async Task LoadConfig()
        {
            var simpleConfigFile = SimpleConfigFactory.ResolveFile();
            Config = await simpleConfigFile.LoadDaemonConfig().ConfigureAwait(false);
        }

        public MessageResult TryStart()
        {
            return MessageResult.Create(true, "ToDo");
        }
        public MessageResult TryStop()
        {
            return MessageResult.Create(true, "ToDo");
        }
    }
}
