// ReSharper disable once CheckNamespace

using System.Threading.Tasks;

namespace Common
{
    public class MessageResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static MessageResult Create(bool success, string message, object data = null)
        {
            return new MessageResult() { Success = success, Message = message, Data = data };
        }

        public static Task<MessageResult> CreateTask(bool success, string message, object data = null)
        {
            return Task.FromResult(Create(success, message, data));
        }
    }
}