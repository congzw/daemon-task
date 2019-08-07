using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
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

        public static MessageResult MethodResult(string method, bool success, object data = null)
        {
            var result = new MessageResult();
            result.Success = success;
            result.Data = data;
            result.Message = string.Format("{0}: {1} => {2}", method, success ? " Success" : " Fail", data);
            return result;
        }
    }
}