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
            this.txtConfig.Enabled = false;
            this.txtConfig.ScrollBars = ScrollBars.Vertical;
            this.txtMessage.Enabled = false;
            this.txtMessage.ScrollBars = ScrollBars.Vertical;

            Vo = new DaemonFormVo();
        }

        private async void DaemonForm_Load(object sender, EventArgs e)
        {
            var config = await Vo.LoadConfig();
            this.txtConfig.Text = config.ToJson(true);
        }
    }
}
