using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common
{
    [TestClass]
    public class SimpleProcessRunnerSpec
    {
        [TestMethod]
        public void Start_ArgsNull_Should_Ex()
        {
            AssertHelper.ShouldThrows<ArgumentNullException>(() =>
            {
                new SimpleProcessRunner(null);
            });
        }

        [TestMethod]
        public async Task Start_NotRunning_Should_Invoke()
        {
            var mockSimpleProcess = CreateProcess();
            var runner = new SimpleProcessRunner(mockSimpleProcess);
            var tryStart = await runner.TryStart();
            tryStart.Message.Log();
            tryStart.Success.ShouldTrue();
            mockSimpleProcess.StartInvoked.ShouldTrue();
        }

        [TestMethod]
        public async Task Start_IsRunning_Should_NotInvoke()
        {
            var mockSimpleProcess = CreateProcess();
            mockSimpleProcess.MockIsRunning = true;
            var runner = new SimpleProcessRunner(mockSimpleProcess);

            var tryStart = await runner.TryStart();
            tryStart.Message.Log();
            tryStart.Success.ShouldTrue();
            mockSimpleProcess.StartInvoked.ShouldFalse();
        }

        [TestMethod]
        public async Task Stop_NotRunning_Should_NotInvoke()
        {
            var mockSimpleProcess = CreateProcess();
            var runner = new SimpleProcessRunner(mockSimpleProcess);
            var tryStart = await runner.TryStop();
            tryStart.Message.Log();
            tryStart.Success.ShouldTrue();
            mockSimpleProcess.StopInvoked.ShouldFalse();
        }

        [TestMethod]
        public async Task Stop_IsRunning_Should_Invoke()
        {
            var mockSimpleProcess = CreateProcess();
            mockSimpleProcess.MockIsRunning = true;
            var runner = new SimpleProcessRunner(mockSimpleProcess);

            var tryStart = await runner.TryStop();
            tryStart.Message.Log();
            tryStart.Success.ShouldTrue();
            mockSimpleProcess.StopInvoked.ShouldTrue();
        }

        private MockSimpleProcess CreateProcess()
        {
            return new MockSimpleProcess(new SimpleProcessInfo() { ProcessName = "Foo" });
        }
    }

    [TestClass]
    public class SimpleProcessFactorySpec
    {
        [TestMethod]
        public void Create_GetOrCreate_ArgsNull_Should_Ex()
        {
            AssertHelper.ShouldThrows<ArgumentNullException>(() =>
            {
                var simpleProcessFactory = new SimpleProcessFactory();
                simpleProcessFactory.Create(null);
            });


            AssertHelper.ShouldThrows<ArgumentNullException>(() =>
            {
                var simpleProcessFactory = new SimpleProcessFactory();
                simpleProcessFactory.GetOrCreate(null);
            });
        }

        [TestMethod]
        public void Create_GetOrCreate_ProcessNameNull_Should_Ex()
        {
            AssertHelper.ShouldThrows<ArgumentException>(() =>
            {
                var simpleProcessFactory = new SimpleProcessFactory();
                simpleProcessFactory.Create(new SimpleProcessInfo() { ProcessName = "", ExePath = "Foo.exe" });
            });


            AssertHelper.ShouldThrows<ArgumentException>(() =>
            {
                var simpleProcessFactory = new SimpleProcessFactory();
                simpleProcessFactory.GetOrCreate(new SimpleProcessInfo() { ProcessName = "", ExePath = "Foo.exe" });
            });
        }

        [TestMethod]
        public void Create_GetOrCreate_ExePathNull_Should_Ex()
        {
            AssertHelper.ShouldThrows<ArgumentException>(() =>
            {
                var simpleProcessFactory = new SimpleProcessFactory();
                simpleProcessFactory.Create(new SimpleProcessInfo() { ProcessName = "Foo", ExePath = "" });
            });

            AssertHelper.ShouldThrows<ArgumentException>(() =>
            {
                var simpleProcessFactory = new SimpleProcessFactory();
                simpleProcessFactory.GetOrCreate(new SimpleProcessInfo() { ProcessName = "Foo", ExePath = "" });
            });
        }

        [TestMethod]
        public void Create_SameProcessName_Should_Diff()
        {
            var simpleProcessFactory = new SimpleProcessFactory();
            var simpleProcess = simpleProcessFactory.Create(new SimpleProcessInfo() { ProcessName = "Foo", ExePath = "Foo.exe" });
            var simpleProcess2 = simpleProcessFactory.Create(new SimpleProcessInfo() { ProcessName = "Foo", ExePath = "Foo.exe" });
            simpleProcess2.ShouldNotSame(simpleProcess);
        }

        [TestMethod]
        public void GetOrCreate_SameProcessName_Should_Same()
        {
            var simpleProcessFactory = new SimpleProcessFactory();
            var simpleProcess = simpleProcessFactory.GetOrCreate(new SimpleProcessInfo() { ProcessName = "Foo", ExePath = "Foo.exe" });
            var simpleProcess2 = simpleProcessFactory.GetOrCreate(new SimpleProcessInfo() { ProcessName = "Foo", ExePath = "Foo.exe" });
            simpleProcess2.ShouldSame(simpleProcess);
        }
    }

    internal class MockSimpleProcess : ISimpleProcess
    {
        public MockSimpleProcess(SimpleProcessInfo info)
        {
            Info = info;
        }

        public SimpleProcessInfo Info { get; set; }

        public bool MockIsRunning { get; set; }

        public bool IsRunning()
        {
            return MockIsRunning;
        }

        public bool StartInvoked { get; set; }

        public void Start()
        {
            StartInvoked = true;
            MockIsRunning = true;
        }

        public bool StopInvoked { get; set; }


        public void Stop()
        {
            StopInvoked = true;
            MockIsRunning = false;
        }
    }
}
