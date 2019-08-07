using System;
using System.Collections.Generic;

namespace DaemonApp.Libs
{
    #region Fx

    public static class AsyncFormEventBus
    {
        //[ThreadStatic]
        private static List<Delegate> actions;

        public static void Register<T>(Action<T> callback) where T : IAsyncFormEvent
        {
            if (actions == null)
            {
                actions = new List<Delegate>();
            }
            actions.Add(callback);
        }

        public static void ClearCallbacks()
        {
            actions = null;
        }

        public static void Raise<T>(T args) where T : IAsyncFormEvent
        {
            if (!ShouldRaise())
            {
                return;
            }

            if (actions != null)
            {
                foreach (var action in actions)
                {
                    if (action is Action<T>)
                    {
                        ((Action<T>)action)(args);
                    }
                }
            }
        }

        public static Func<bool> ShouldRaise = () => true;
    }

    public interface IAsyncFormEvent
    {
        /// <summary>
        /// �¼�������ʱ��
        /// </summary>
        DateTime DateTimeEventOccurred { get; }

        //�������������ĸ������
        ///// <summary>
        ///// �Ƿ�֧��ȡ��
        ///// </summary>
        //bool SupportCancel { get; }
    }

    public abstract class BaseAsyncFormEvent : IAsyncFormEvent
    {
        public DateTime DateTimeEventOccurred { get; private set; }

        protected BaseAsyncFormEvent()
        {
            DateTimeEventOccurred = DateTime.Now;
        }
    }

    #endregion

    #region using
    
    public interface IAsyncMessageUi
    {
        void ShowCallbackMessage(string value);
    }
    public class AsyncFormMessageEvent : BaseAsyncFormEvent
    {
        public AsyncFormMessageEvent(string message)
        {
            Message = message;
        }
        public string Message { get; set; }
    }

    #endregion
}