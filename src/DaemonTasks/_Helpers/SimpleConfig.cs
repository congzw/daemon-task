using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Common
{
    #region config

    public interface ISimpleConfig
    {
        void AddOrUpdate<T>(string key, T value);
        T TryGet<T>(string key, T defaultValue);
    }

    public class SimpleConfig : ISimpleConfig
    {
        private readonly object _lock = new object();

        public SimpleConfig()
        {
            Items = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, object> Items { get; set; }

        public void AddOrUpdate<T>(string key, T value)
        {
            lock (_lock)
            {
                Items[key] = value;
            }
        }

        public T TryGet<T>(string key, T defaultValue)
        {
            lock (_lock)
            {
                if (!Items.ContainsKey(key))
                {
                    return defaultValue;
                }
                return (T)Items[key];
            }
        }
    }

    public static class SimpleConfigExtensions
    {
        public static T TryGetModel<T>(this ISimpleConfig simpleConfig, T defaultValue)
        {
            return simpleConfig.TryGet(typeof(T).FullName, defaultValue);
        }

        public static void AddOrUpdateModel<T>(this ISimpleConfig simpleConfig, T value)
        {
            simpleConfig.AddOrUpdate(typeof(T).FullName, value);
        }
    }

    #endregion

    #region config file

    public interface ISimpleConfigFile
    {
        Task<T> ReadFile<T>(string filePath, T defaultValue) where T : ISimpleConfig;
        Task SaveFile<T>(string filePath, T config) where T : ISimpleConfig;
    }

    public class SimpleConfigFile : ISimpleConfigFile
    {
        private readonly ISimpleJsonFile _simpleJsonFile;

        public SimpleConfigFile(ISimpleJsonFile simpleJsonFile)
        {
            _simpleJsonFile = simpleJsonFile;
        }

        public Task<T> ReadFile<T>(string filePath, T defaultValue) where T : ISimpleConfig
        {
            return _simpleJsonFile.ReadFileAsSingle<T>(filePath);
        }

        public Task SaveFile<T>(string filePath, T config) where T : ISimpleConfig
        {
            return _simpleJsonFile.SaveFileAsSingle(filePath, config, true);
        }
    }
    
    public static class SimpleConfigFileExtensions
    {
        public static string GetDefaultConfigFilePath<T>(this ISimpleConfigFile simpleConfigFile)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, typeof(T).Name + ".json");
            return filePath;
        }

        public static Task<T> ReadFile<T>(this ISimpleConfigFile simpleConfigFile, T defaultValue) where T : ISimpleConfig
        {
            var filePath = GetDefaultConfigFilePath<T>(simpleConfigFile);
            return simpleConfigFile.ReadFile(filePath, defaultValue);
        }

        public static Task SaveFile<T>(this ISimpleConfigFile simpleConfigFile, T value) where T : ISimpleConfig
        {
            var filePath = GetDefaultConfigFilePath<T>(simpleConfigFile);
            return simpleConfigFile.SaveFile(filePath, value);
        }
    }
    
    #endregion

    public class SimpleConfigFactory
    {
        #region for di extensions

        private static readonly Lazy<ISimpleConfig> LazyInstance = new Lazy<ISimpleConfig>(() => new SimpleConfig());
        private static readonly Lazy<ISimpleConfigFile> LazyInstanceFile = new Lazy<ISimpleConfigFile>(() => new SimpleConfigFile(SimpleJson.ResolveSimpleJsonFile()));
        public static Func<ISimpleConfig> Resolve { get; set; } = () => LazyInstance.Value;
        public static Func<ISimpleConfigFile> ResolveFile { get; set; } = () => LazyInstanceFile.Value;

        #endregion
    }
}
