using System.Windows.Forms;

namespace DaemonApp.Libs
{
    /// <summary>
    /// 支持AsyncFormEventBus异步更新UI的窗体基类(IAsyncMessageUi)
    /// </summary>
    public class AsyncForm : Form, IAsyncMessageUi
    {
        // 定义显示状态的委托
        private delegate void ShowStateDelegate(string value);
        private readonly ShowStateDelegate _showStateCallback = null;
        //protected Action<AsyncFormMessageEvent> callback;
        public bool WithPrefix { get; set; }


        protected AsyncForm()
        {
            if (!this.DesignMode)
            {
                //抽象类会导致Vs设计器无法显示的问题...
                _showStateCallback = ShowCallbackMessage;
                AsyncFormEventBus.Register<AsyncFormMessageEvent>(UpdateUi);
                WithPrefix = true;
            }
        }
        
        //此方法会在非UI线程中被调用
        private void UpdateUi(AsyncFormMessageEvent obj)
        {
            if (!AsyncFormEventBus.ShouldRaise())
            {
                return;
            }

            string value = string.Format("{0} \r\n", obj.Message);
            if (WithPrefix)
            {
                value = obj.DateTimeEventOccurred + ": " + value;
            }
            var invoker = GetInvoker();
            if (invoker.InvokeRequired)
            {
                invoker.Invoke(_showStateCallback, value);
            }
            else
            {
                _showStateCallback.Invoke(value);
            }
        }

        //此方法由子类继承，返回负责执行更新UI的控件
        /// <summary>
        /// 此方法由子类继承，返回负责执行更新UI的控件
        /// </summary>
        /// <returns></returns>
        protected virtual Control GetInvoker()
        {
            return null;
        }

        //此方法由子类继承，负责执行更新
        /// <summary>
        /// 此方法由子类继承，负责执行更新
        /// </summary>
        /// <param name="value"></param>
        public virtual void ShowCallbackMessage(string value)
        {
        }
    }
}
