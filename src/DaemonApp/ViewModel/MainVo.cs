using System;
using System.Threading.Tasks;
using Common;

namespace DaemonApp.ViewModel
{
    public class MainVo
    {
        public SimpleProcessInfo Load()
        {
            return new SimpleProcessInfo()
            {
                ProcessName = "FooClient",
                ExePath = "FooClient.exe",
                ExeArgs = ""
            };
        }

        public MessageResult Validate(SimpleProcessInfo info)
        {
            var success = SimpleProcessInfo.Validate(info, out var message);
            return MessageResult.Create(success, message, info);
        }

        public bool IsRunning(SimpleProcessInfo info)
        {
            var simpleProcessFactory = SimpleProcessFactory.Resolve();
            var simpleProcess = simpleProcessFactory.GetOrCreate(info);
            var simpleProcessRunner = new SimpleProcessRunner(simpleProcess);
            return simpleProcessRunner.Process.IsRunning();
        }

        private FooClientDaemon _fooClientDaemon = null;

        public Task<MessageResult> TryStart(SimpleProcessInfo info)
        {
            var simpleProcessFactory = SimpleProcessFactory.Resolve();
            var simpleProcess = simpleProcessFactory.GetOrCreate(info);
            var simpleProcessRunner = new SimpleProcessRunner(simpleProcess);
            if (_fooClientDaemon == null)
            {
                var simpleDaemon = SimpleDaemon.Resolve();
                _fooClientDaemon = new FooClientDaemon(simpleDaemon, simpleProcessRunner);
            }
            else
            {
                if (!info.ProcessName.Equals(_fooClientDaemon.Runner.Process.Info.ProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    _fooClientDaemon.Daemon.Dispose();
                    _fooClientDaemon.Daemon = null;

                    var simpleDaemon = SimpleDaemon.Resolve();
                    _fooClientDaemon = new FooClientDaemon(simpleDaemon, simpleProcessRunner);
                }
            }

            return _fooClientDaemon.TryStart();
        }

        public Task<MessageResult> TryStop(SimpleProcessInfo info)
        {
            if (_fooClientDaemon == null)
            {
                return MessageResult.CreateTask(true, "尚未开始");
            }

            _fooClientDaemon.Runner.TryStop();
            return _fooClientDaemon.TryStop();
        }
    }

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
