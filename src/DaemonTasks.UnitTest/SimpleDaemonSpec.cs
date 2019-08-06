using System;
using System.Security.Permissions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common
{
    [TestClass]
    public class SimpleDaemonSpec
    {
        [TestMethod]
        public void Start_ArgsNull_Should_Ex()
        {
            using (var simpleDaemon = new SimpleDaemon())
            {
                var config = TimeSpan.FromMilliseconds(50);
                var mockTask = MockTask.Create();

                AssertHelper.ShouldAsyncThrows<ArgumentException>(() => simpleDaemon.Start(TimeSpan.Zero, mockTask.MockAction, false));
                AssertHelper.ShouldAsyncThrows<ArgumentNullException>(() => simpleDaemon.Start(config, null, false));
            }
        }

        [TestMethod]
        public async Task Start_SecondTime_Should_Fail()
        {
            using (var simpleDaemon = new SimpleDaemon())
            {
                var config = TimeSpan.FromMilliseconds(50);
                var mockTask = MockTask.Create();

                var startResult = await simpleDaemon.Start(config, mockTask.MockAction, false).ConfigureAwait(false);
                startResult.Success.ShouldTrue();
                startResult.Message.Log();

                var startResult2 = await simpleDaemon.Start(config, mockTask.MockAction, false).ConfigureAwait(false);
                startResult2.Success.ShouldFalse();
                startResult2.Message.Log();
            }
        }

        [TestMethod]
        public async Task Stop_NotRunning_Should_Ignore()
        {
            using (var simpleDaemon = new SimpleDaemon())
            {
                var startResult = await simpleDaemon.Stop().ConfigureAwait(false);
                startResult.Success.ShouldTrue();
                startResult.Message.Log();
            }
        }

        [TestMethod]
        public async Task Stop_Running_Should_Success()
        {
            using (var simpleDaemon = new SimpleDaemon())
            {
                var config = TimeSpan.FromMilliseconds(50);
                var mockTask = MockTask.Create();

                var startResult = await simpleDaemon.Start(config, mockTask.MockAction, false).ConfigureAwait(false);
                startResult.Success.ShouldTrue();
                startResult.Message.Log();

                var stopResult = await simpleDaemon.Stop().ConfigureAwait(false);
                stopResult.Success.ShouldTrue();
                stopResult.Message.Log();
            }
        }

        [TestMethod]
        public async Task Stop_SecondTime_Should_Ignore()
        {
            using (var simpleDaemon = new SimpleDaemon())
            {
                var config = TimeSpan.FromMilliseconds(50);
                var mockTask = MockTask.Create();

                var startResult = await simpleDaemon.Start(config, mockTask.MockAction, false).ConfigureAwait(false);
                startResult.Success.ShouldTrue();
                startResult.Message.Log();

                var stopResult = await simpleDaemon.Stop().ConfigureAwait(false);
                stopResult.Success.ShouldTrue();
                stopResult.Message.Log();

                var stopResult2 = await simpleDaemon.Stop().ConfigureAwait(false);
                stopResult2.Success.ShouldTrue();
                stopResult2.Message.Log();
            }
        }
        
        [TestMethod]
        public async Task Start_NotStop_Should_Running()
        {
            using (var simpleDaemon = new SimpleDaemon())
            {
                var config = TimeSpan.FromMilliseconds(10);

                var mockTask = MockTask.Create();
                var startResult = await simpleDaemon.Start(config, mockTask.MockAction, false).ConfigureAwait(false);

                startResult.Success.ShouldTrue();
                startResult.Message.Log();

                var lastInvokeCount = 0;
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(20).ConfigureAwait(false);
                    var isRunning = mockTask.InvokeCount >= lastInvokeCount;
                    string.Format("Running Invoked: {0}", mockTask.InvokeCount).Log();
                    isRunning.ShouldTrue();
                    lastInvokeCount = mockTask.InvokeCount;
                }

                var stopResult = await simpleDaemon.Stop();
                stopResult.Success.ShouldTrue();
                stopResult.Message.Log();
            }
        }

        [TestMethod]
        public async Task LogMessage_Replace_Should_Ok()
        {
            using (var simpleDaemon = new SimpleDaemon())
            {
                var mockLog = new MockLog();
                simpleDaemon.LogMessage = msg => mockLog.Log(msg);

                var config = TimeSpan.FromMilliseconds(50);
                var mockTask = MockTask.Create();

                var startResult = await simpleDaemon.Start(config, mockTask.MockAction, false).ConfigureAwait(false);
                startResult.Success.ShouldTrue();
                startResult.Message.Log();

                var startResult2 = await simpleDaemon.Start(config, mockTask.MockAction, false).ConfigureAwait(false);
                startResult2.Success.ShouldFalse();
                startResult2.Message.Log();
            }
        }
    }

    public class MockTask
    {
        public int InvokeCount { get; set; }

        public Action MockAction { get; set; }

        public static MockTask Create()
        {
            var mockTask = new MockTask();
            mockTask.MockAction = () =>
            {
                mockTask.InvokeCount++;
                AssertHelper.WriteLine("task running at: " + DateTime.Now.ToString("yyyyMMdd HH:mm:ss:fff"));
            };
            return mockTask;
        }
    }

    public class MockLog
    {
        public bool LogInvoked { get; set; }

        public void Log(object message)
        {
            LogInvoked = true;
        }
    }
}
