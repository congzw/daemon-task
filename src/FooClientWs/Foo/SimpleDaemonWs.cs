using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace FooClientWs.Foo
{
    public class SimpleLoopTaskWindowsService
    {
        private readonly SimpleLoopTask _simpleLoopTask;

        public SimpleLoopTaskWindowsService(SimpleLoopTask simpleLoopTask)
        {
            _simpleLoopTask = simpleLoopTask;
            Init(_simpleLoopTask);
        }

        private void Init(SimpleLoopTask simpleLoopTask)
        {
            //simpleLoopTask.LoopTask = () =>
            //{
            //    //todo
            //};

        }


        protected void OnStart(string[] args)
        {
            //todo
            _simpleLoopTask.Start();
        }

        protected void OnStop()
        {
            //todo
            _simpleLoopTask.Stop();
        }
    }
}
