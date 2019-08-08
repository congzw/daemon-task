using System;
using System.Windows.Forms;
using Common;
using DaemonApp.ViewModel;

namespace DaemonApp
{
    public partial class DaemonForm : Form
    {
        public DaemonForm()
        {
            InitializeComponent();
            MyInitializeComponent();
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
