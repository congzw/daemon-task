using System.Threading.Tasks;
using Common;

namespace DaemonApp.Libs
{
    public class MyLogFactory : ISimpleLogFactory
    {
        private readonly ISimpleLogFactory _simpleLogFactory;

        public MyLogFactory(ISimpleLogFactory simpleLogFactory)
        {
            _simpleLogFactory = simpleLogFactory;
        }

        public ISimpleLog Create(string category)
        {
            var simpleLog = _simpleLogFactory.Create(category);
            return new MyLog(simpleLog);
        }

        public ISimpleLog GetOrCreate(string category)
        {
            var simpleLog = _simpleLogFactory.GetOrCreate(category);
            return new MyLog(simpleLog);
        }
    }

    public class MyLog : ISimpleLog
    {
        private readonly ISimpleLog _simpleLog;

        public MyLog(ISimpleLog simpleLog)
        {
            _simpleLog = simpleLog;
        }

        public SimpleLogLevel EnabledLevel
        {
            get => _simpleLog.EnabledLevel;
            set => _simpleLog.EnabledLevel = value;
        }

        public Task Log(object message, SimpleLogLevel level)
        {
            if (level >= EnabledLevel)
            {
                AsyncFormEventBus.Raise(new AsyncFormMessageEvent(message.ToString()));
            }
            //return Task.FromResult(0);
            return _simpleLog.Log(message, level);
        }
    }
}
