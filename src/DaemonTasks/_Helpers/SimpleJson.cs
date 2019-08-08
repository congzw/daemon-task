using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Common
{
    public interface ISimpleJson
    {
        string SerializeObject(object value, bool indented);
        T DeserializeObject<T>(string json);
        object DeserializeObject(string json, object defaultValue);
    }

    public interface ISimpleJsonFile
    {
        Task<IList<T>> ReadFile<T>(string filePath);
        Task SaveFile<T>(string filePath, IList<T> list, bool indented);
    }

    public class SimpleJson : ISimpleJson, ISimpleJsonFile
    {
        private readonly AsyncFile _asyncFile = AsyncFile.Instance;

        public string SerializeObject(object value, bool indented)
        {
            return JsonConvert.SerializeObject(value, indented ? Formatting.Indented : Formatting.None);
        }

        public T DeserializeObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(json);
        }

        public object DeserializeObject(string json, object defaultValue)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return defaultValue;
            }
            return JsonConvert.DeserializeObject(json);
        }

        public async Task<IList<T>> ReadFile<T>(string filePath)
        {
            var list = new List<T>();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return list;
            }

            var json = await _asyncFile.ReadAllText(filePath).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                return list;
            }
            list = DeserializeObject<List<T>>(json);
            return list;
        }

        public Task SaveFile<T>(string filePath, IList<T> list, bool indented)
        {
            if (string.IsNullOrWhiteSpace(filePath) || list == null)
            {
                return Task.FromResult(0);
            }
            
            var json = SerializeObject(list, indented);
            return _asyncFile.WriteAllText(filePath, json);
        }

        #region for di extensions

        private static readonly Lazy<SimpleJson> Instance = new Lazy<SimpleJson>(() => new SimpleJson());
        public static Func<ISimpleJson> Resolve { get; set; } = () => Instance.Value;
        public static Func<ISimpleJsonFile> ResolveSimpleJsonFile { get; set; } = () => Instance.Value;

        #endregion
    }

    public static class SimpleJsonExtensions
    {
        public static async Task<T> ReadFileAsSingle<T>(this ISimpleJsonFile simpleJsonFile, string filePath)
        {
            var list = await simpleJsonFile.ReadFile<T>(filePath).ConfigureAwait(false);
            if (list == null || list.Count == 0)
            {
                return default(T);
            }
            return list.SingleOrDefault();
        }

        public static Task SaveFileAsSingle<T>(this ISimpleJsonFile simpleJsonFile, string filePath, T model, bool indented)
        {
            if (model == null)
            {
                return Task.FromResult(0);
            }
            return simpleJsonFile.SaveFile(filePath, new T[] { model }, indented);
        }

        public static async Task AppendFile<T>(this ISimpleJsonFile simpleJsonFile, string filePath, IList<T> list, bool indented)
        {
            if (string.IsNullOrWhiteSpace(filePath) || list == null || list.Count == 0)
            {
                return;
            }
            var saveItems = new List<T>();
            var currents = await simpleJsonFile.ReadFile<T>(filePath).ConfigureAwait(false);
            if (currents != null)
            {
                saveItems.AddRange(currents);
            }
            saveItems.AddRange(list);
            await simpleJsonFile.SaveFile(filePath, saveItems, indented).ConfigureAwait(false);
        }
    }
    
    public static class JsonExtensions
    {
        /// <summary>
        /// object as json
        /// </summary>
        /// <param name="model"></param>
        /// <param name="indented"></param>
        /// <returns></returns>
        public static string ToJson(this object model, bool indented)
        {
            var simpleJson = SimpleJson.Resolve();
            return simpleJson.SerializeObject(model, indented);
        }

        /// <summary>
        /// json as T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="failThrowEx"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string json, bool failThrowEx = false)
        {
            var simpleJson = SimpleJson.Resolve();
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }
            try
            {
                return simpleJson.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                if (failThrowEx)
                {
                    throw;
                }
                //ignored
                return default(T);
            }
        }

        /// <summary>
        /// json as object
        /// </summary>
        /// <param name="json"></param>
        /// <param name="defaultValue"></param>
        /// <param name="failThrowEx"></param>
        /// <returns></returns>
        public static object FromJson(this string json, object defaultValue = null, bool failThrowEx = false)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return defaultValue;
            }
            try
            {
                var simpleJson = SimpleJson.Resolve();
                return simpleJson.DeserializeObject(json, defaultValue);
            }
            catch (Exception)
            {
                if (failThrowEx)
                {
                    throw;
                }
                //ignored
                return defaultValue;
            }
        }
    }
}
