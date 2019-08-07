using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FooClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            var index = 1;
            await ShowRunning(index);
        }

        private int maxRunCount = 100;
        private async Task ShowRunning(int index)
        {
            if (index <= maxRunCount)
            {
                this.txtMessage.AppendText(string.Format("running {0}/{1} => {2:yyyy-MM-dd HH:mm:ss:fff}{3}", index, maxRunCount, DateTime.Now, Environment.NewLine));
                await Task.Delay(1000);
                index++;
                await ShowRunning(index);
            }
            else
            {
                this.txtMessage.AppendText("--------------------");
                this.txtMessage.AppendText(Environment.NewLine);
                this.txtMessage.AppendText(string.Format("running complete! => {0:yyyy-MM-dd HH:mm:ss:fff}", DateTime.Now));
            }
        }
    }
}
