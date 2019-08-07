using System.Windows.Forms;
using Common;
using DaemonApp.Libs;
using DaemonApp.ViewModel;

namespace DaemonApp
{
    public partial class MainForm : AsyncForm
    {
        public MainForm()
        {
            InitializeComponent();
            Vo = new MainVo();
        }

        protected override Control GetInvoker()
        {
            return this.txtMessage;
        }
        public override void ShowCallbackMessage(string value)
        {
            this.txtMessage.AppendText(value);
        }
        public MainVo Vo { get; set; }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            var info = Vo.Load();
            SetUi(info);

            var vr = Vo.Validate(info);
            var isRunning = false;
            if (vr.Success)
            {
                isRunning = Vo.IsRunning(info);
            }
            this.btnProcess.Text = isRunning ? "StopIt" : "StartIt";
        }

        private async void btnProcess_Click(object sender, System.EventArgs e)
        {
            var info = ReadUi();
            var vr = Vo.Validate(info);
            if (!vr.Success)
            {
                MessageBox.Show(vr.Message);
                return;
            }

            var isRunning = Vo.IsRunning(info);
            if (isRunning)
            {
                var stopResult = await Vo.TryStop(info);
                MessageBox.Show(stopResult.Message);
            }
            else
            {
                var startResult = await Vo.TryStart(info);
                MessageBox.Show(startResult.Message);
            }

            isRunning = Vo.IsRunning(info);
            this.btnProcess.Text = isRunning ? "StopIt" : "StartIt";
        }

        private void SetUi(SimpleProcessInfo info)
        {
            this.txtProcessName.Text = info.ProcessName;
            this.txtExePath.Text = info.ExePath;
            this.txtExeArgs.Text = info.ExeArgs;
        }

        private SimpleProcessInfo ReadUi()
        {
            var processName = this.txtProcessName.Text.Trim();
            var exePath = this.txtExePath.Text.Trim();
            var exeArgs = this.txtExeArgs.Text.Trim();
            return SimpleProcessInfo.Create(processName, exePath, exeArgs);
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            this.txtMessage.Clear();
        }
    }
}
