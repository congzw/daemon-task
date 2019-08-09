using System.Collections.Generic;

namespace Common
{
    public class SimpleProcessDaemon
    {
        public SimpleProcessDaemon(IList<SimpleProcessRunner> runners)
        {
            Runners = runners;
        }

        public IList<SimpleProcessRunner> Runners { get; set; }

        public void TryStart()
        {
            foreach (var runner in Runners)
            {
                runner.TryStart();
            }
        }

        public void TryStop()
        {
            foreach (var runner in Runners)
            {
                runner.TryStop();
            }
        }
    }
}
