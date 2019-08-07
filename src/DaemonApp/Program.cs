using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using DaemonApp.Libs;

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
            Application.Run(new MainForm());
        }
    }
}
