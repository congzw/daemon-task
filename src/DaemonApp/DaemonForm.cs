using System;
using System.Windows.Forms;
using Common;
using DaemonApp.Libs;
using DaemonApp.ViewModel;

namespace DaemonApp
{
    public partial class DaemonForm : AsyncForm
    {
        public DaemonForm()
        {
            InitializeComponent();
            MyInitializeComponent();
        }
        protected override Control GetInvoker()
        {
            return this.txtMessage;
        }
        public override void ShowCallbackMessage(string value)
        {
            this.txtMessage.AppendText(value);
        }

        public DaemonFormVo Vo { get; set; }

        private void MyInitializeComponent()
        {
            //this.txtConfig.Enabled = false;
            //this.txtMessage.Enabled = false;
            this.txtConfig.ScrollBars = ScrollBars.Vertical;
            this.txtMessage.ScrollBars = ScrollBars.Vertical;

            Vo = new DaemonFormVo();
        }

        private async void DaemonForm_Load(object sender, EventArgs e)
        {
            await Vo.LoadConfig();
            this.txtConfig.Text = Vo.Config.ToJson(true);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var messageResult = Vo.TryStart();
            MessageBox.Show(messageResult.Message);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            var messageResult = Vo.TryStop();
            MessageBox.Show(messageResult.Message);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.txtMessage.Clear();
        }
    }
}
