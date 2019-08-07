using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
    public class SimpleProcessInfo
    {
        public string ProcessName { get; set; }
        public string ExePath { get; set; }
        public string ExeArgs { get; set; }

        public static bool Validate(SimpleProcessInfo info, out string message)
        {
            if (info == null)
            {
                message = "info should not be null";
                return false;
            }

            if (string.IsNullOrWhiteSpace(info.ProcessName))
            {
                message = "ProcessName should not be null or empty";
                return false;
            }

            if (string.IsNullOrWhiteSpace(info.ExePath))
            {
                message = "ExePath should not be null or empty";
                return false;
            }

            message = "OK";
            return true;
        }
    }

    public interface ISimpleProcess
    {
        SimpleProcessInfo Info { get; set; }
        bool IsRunning();
        void Start();
        void Stop();
    }

    public class SimpleProcess : ISimpleProcess
    {
        private ISimpleLog _log;

        public ISimpleLog Log
        {
            set => _log = value ?? throw new ArgumentNullException(nameof(value));
            get => _log ?? (_log = SimpleLogFactory.Resolve().CreateLogFor(this));
        }

        public SimpleProcessInfo Info { get; set; }

        public bool IsRunning()
        {
            var processes = Process.GetProcessesByName(Info.ProcessName);
            return processes.Length > 0;
        }

        public void Start()
        {
            var theProcess = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = false;
            startInfo.FileName = Info.ExePath;
            startInfo.RedirectStandardInput = true;
            //此代码意味着程序自行结束，不启用窗口，则需要自己使用Kill关闭进程（例如某些关闭命令行退出的场景）
            startInfo.CreateNoWindow = true;
            if (!string.IsNullOrWhiteSpace(Info.ExeArgs))
            {
                startInfo.Arguments = Info.ExeArgs;
            }
            theProcess.StartInfo = startInfo;
            theProcess.Start();
        }

        public void Stop()
        {
            var processes = Process.GetProcessesByName(Info.ProcessName);
            var theProcess = processes.FirstOrDefault();
            if (theProcess == null)
            {
                Log.LogInfo("find no process : " + Info.ProcessName);
                return;
            }

            Log.LogInfo("find process : " + Info.ProcessName);
            var theProcessHasExited = theProcess.HasExited;
            if (!theProcessHasExited)
            {
                Log.LogInfo("killing process: " + Info.ProcessName);
                theProcess.Kill();
                Log.LogInfo("killed process: " + Info.ProcessName);
            }
            theProcess.Dispose();
        }
    }

    #region Factory

    public interface ISimpleProcessFactory
    {
        ISimpleProcess Create(SimpleProcessInfo info);
        ISimpleProcess GetOrCreate(SimpleProcessInfo info);
    }

    public class SimpleProcessFactory : ISimpleProcessFactory
    {
        public SimpleProcessFactory()
        {
            Runners = new ConcurrentDictionary<string, ISimpleProcess>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, ISimpleProcess> Runners { get; set; }

        public ISimpleProcess Create(SimpleProcessInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var success = SimpleProcessInfo.Validate(info, out var message);
            if (!success)
            {
                throw new ArgumentException(message);
            }
            return new SimpleProcess { Info = info };
        }

        public ISimpleProcess GetOrCreate(SimpleProcessInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var success = SimpleProcessInfo.Validate(info, out var message);
            if (!success)
            {
                throw new ArgumentException(message);
            }

            var tryGetValue = Runners.TryGetValue(info.ProcessName, out var theRunner);
            if (!tryGetValue || theRunner == null)
            {
                theRunner = Create(info);
                Runners.Add(info.ProcessName, theRunner);
            }

            return theRunner;
        }

        #region for di extensions

        private static readonly Lazy<SimpleProcessFactory> Instance = new Lazy<SimpleProcessFactory>(() => new SimpleProcessFactory());
        public static Func<ISimpleProcessFactory> Resolve { get; set; } = () => Instance.Value;

        #endregion
    }


    #endregion

    #region runner

    public class SimpleProcessRunner
    {
        public SimpleProcessRunner(ISimpleProcess simpleProcess)
        {
            Process = simpleProcess ?? throw new ArgumentNullException(nameof(simpleProcess));
        }

        private ISimpleLog _log;

        public ISimpleLog Log
        {
            set => _log = value ?? throw new ArgumentNullException(nameof(value));
            get => _log ?? (_log = SimpleLogFactory.Resolve().CreateLogFor(this));
        }

        public ISimpleProcess Process { get; set; }

        public Task<MessageResult> TryStart()
        {
            return Task.Run(() =>
            {
                var isRunning = Process.IsRunning();
                Log.LogInfo("process is running? " + isRunning);
                if (isRunning)
                {
                    return MessageResult.CreateTask(true, "process is already running: " + Process.Info.ProcessName);
                }

                try
                {
                    Log.LogInfo("process starting: " + Process.Info.ProcessName);
                    Process.Start();
                    Log.LogInfo("process started: " + Process.Info.ProcessName);
                    return MessageResult.CreateTask(true, "Process start completed: " + Process.Info.ProcessName);
                }
                catch (Exception ex)
                {
                    Log.LogEx(ex, "process start ex!");
                    return MessageResult.CreateTask(false, "Process start failed: " + Process.Info.ProcessName);
                }
            });
        }

        public Task<MessageResult> TryStop()
        {
            var isRunning = Process.IsRunning();
            Log.LogInfo("process is running? " + isRunning);
            if (!isRunning)
            {
                return MessageResult.CreateTask(true, "process is not running: " + Process.Info.ProcessName);
            }
            try
            {
                Log.LogInfo("process stopping: " + Process.Info.ProcessName);
                Process.Stop();
                Log.LogInfo("process stopped: " + Process.Info.ProcessName);
                return MessageResult.CreateTask(true, "Process stop completed: " + Process.Info.ProcessName);
            }
            catch (Exception ex)
            {
                Log.LogEx(ex, "process stop ex!");
                return MessageResult.CreateTask(false, "Process stop failed: " + Process.Info.ProcessName);
            }
        }
    }

    #endregion
}
