using System;
using System.Windows.Forms;
using Common;
using DaemonApp.Libs;
using DaemonApp.ViewModel;

namespace DaemonApp
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var simpleLogFactory = SimpleLogFactory.Resolve();
            var myLogFactory = new MyLogFactory(simpleLogFactory);
            SimpleLogFactory.Resolve = () => myLogFactory;
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var isDaemonForm = IsDaemonForm();
            if (isDaemonForm)
            {
                Application.Run(new DaemonForm());
            }
            else
            {
                Application.Run(new MainForm());
            }
        }

        private static bool IsDaemonForm()
        {
            var simpleConfigFile = SimpleConfigFactory.ResolveFile();
            var config = simpleConfigFile.LoadDaemonConfig().Result;
            return config.IsEntryForm();
        }
    }
}
