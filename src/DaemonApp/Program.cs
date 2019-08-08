using System;
using System.Security.Principal;
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

            var isAdministrator = IsAdministrator();
            if (!isAdministrator)
            {
                var message = string.Format("{0}{1}{2}", "服务的安装、卸载需要管理员身份！", Environment.NewLine,
                    "请尝试使用右键，然后以管理员身份运行此程序！");
                MessageBox.Show(message);
                return;
            }

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

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool IsDaemonForm()
        {
            var simpleConfigFile = SimpleConfigFactory.ResolveFile();
            var config = simpleConfigFile.LoadDaemonConfig().Result;
            return config.IsEntryForm();
        }
    }
}
