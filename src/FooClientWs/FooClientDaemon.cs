using System;
using System.Threading.Tasks;
using Common;

namespace FooClientWs
{
    public class FooClientDaemon
    {
        public FooClientDaemon(ISimpleDaemon daemon, SimpleProcessRunner runner)
        {
            Daemon = daemon;
            Runner = runner;
        }

        public ISimpleDaemon Daemon { get; set; }

        public SimpleProcessRunner Runner { get; set; }

        public Task<MessageResult> TryStart()
        {
            return Daemon.StartTask(TimeSpan.FromSeconds(3), () => Runner.TryStart(true));
        }

        public async Task<MessageResult> TryStop()
        {
            var messageResult = await Daemon.Stop();
            await Runner.TryStop();
            return messageResult;
        }
    }
}
