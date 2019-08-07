using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Common
{
    //内部使用的简单日志
    public interface ISimpleLog
    {
        SimpleLogLevel EnabledLevel { get; set; }
        Task Log(object message, SimpleLogLevel level);
    }

    public enum SimpleLogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6
    }

    internal class SimpleLog : ISimpleLog
    {
        public string Category { get; set; }

        public SimpleLogLevel EnabledLevel { get; set; }

        public Task Log(object message, SimpleLogLevel level)
        {
            if (level >= EnabledLevel)
            {
                LogMessage(message, level);
            }

            return Task.FromResult(0);
        }

        protected virtual void LogMessage(object message, SimpleLogLevel level)
        {
            Trace.WriteLine(string.Format("{0} [{1}][{2}] {3}",  Category, "SimpleLog", level.ToString(), message));
        }
    }

    #region for simple use

    public static class SimpleLogExtensions
    {
        public static void LogInfo(this ISimpleLog simpleLog, string message)
        {
            simpleLog.Log(message, SimpleLogLevel.Information);
        }

        public static void LogEx(this ISimpleLog simpleLog, Exception ex, string message = null)
        {
            var logMessage = string.Format("{0} => {1}", message ?? ex.Message, ex.StackTrace);
            simpleLog.Log(logMessage, SimpleLogLevel.Error);
        }
    }

    #endregion

    #region factory and settings

    public interface ISimpleLogFactory
    {
        ISimpleLog Create(string category);
        ISimpleLog GetOrCreate(string category);
    }

    public class SimpleLogFactory : ISimpleLogFactory
    {
        public SimpleLogFactory()
        {
            Settings = new SimpleLogSettings();
            SimpleLogs = new ConcurrentDictionary<string, ISimpleLog>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, ISimpleLog> SimpleLogs { get; set; }

        public SimpleLogSettings Settings { get; set; }

        public ISimpleLog Create(string category)
        {
            var tryFixCategory = Settings.TryFixCategory(category);
            var simpleLogLevel = Settings.GetEnabledLevel(tryFixCategory);
            return new SimpleLog() { Category = tryFixCategory, EnabledLevel = simpleLogLevel };
        }

        public ISimpleLog GetOrCreate(string category)
        {
            var tryFixCategory = Settings.TryFixCategory(category);
            var tryGetValue = SimpleLogs.TryGetValue(tryFixCategory, out var theOne);
            if (!tryGetValue || theOne == null)
            {
                theOne = Create(tryFixCategory);
                SimpleLogs.Add(tryFixCategory, theOne);
            }

            return theOne;
        }

        #region for di extensions

        public static Lazy<ISimpleLogFactory> LazyInstance = new Lazy<ISimpleLogFactory>(() => new SimpleLogFactory());
        public static Func<ISimpleLogFactory> Resolve { get; set; } = () => LazyInstance.Value;

        #endregion
    }

    public class SimpleLogSettings
    {
        public SimpleLogSettings()
        {
            Items = new ConcurrentDictionary<string, SimpleLogSetting>(StringComparer.OrdinalIgnoreCase);
            Default = new SimpleLogSetting() { Category = DefaultCategory, EnabledLevel = SimpleLogLevel.Trace };
            Items.Add(DefaultCategory, Default);
        }

        public void SetEnabledLevel(string category, SimpleLogLevel level)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentNullException(nameof(category));
            }

            var key = category.Trim();
            var tryGetValue = Items.TryGetValue(key, out var setting);
            if (!tryGetValue || setting == null)
            {
                setting = new SimpleLogSetting();
                setting.Category = key;
            }
            setting.EnabledLevel = level;
        }

        public SimpleLogLevel GetEnabledLevel(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentNullException(nameof(category));
            }

            var key = category.Trim();
            var tryGetValue = Items.TryGetValue(key, out var setting);
            if (!tryGetValue || setting == null)
            {
                //todo:try find first by key start with?
                return Default.EnabledLevel;
            }
            return setting.EnabledLevel;
        }

        public string TryFixCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return DefaultCategory;
            }

            return category.Trim();
        }

        public SimpleLogSetting Default { get; set; }

        public static string DefaultCategory = "Default";

        public IDictionary<string, SimpleLogSetting> Items { get; set; }
    }

    public class SimpleLogSetting
    {
        public string Category { get; set; }
        public SimpleLogLevel EnabledLevel { get; set; }
    }
    
    public static class SimpleLogFactoryExtensions
    {
        public static ISimpleLog CreateLogFor(this ISimpleLogFactory factory, Type type)
        {
            return factory.Create(type.FullName);
        }

        public static ISimpleLog CreateLogFor<T>(this ISimpleLogFactory factory)
        {
            return factory.CreateLogFor(typeof(T));
        }

        public static ISimpleLog CreateLogFor(this ISimpleLogFactory factory, object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (instance is Type type)
            {
                return factory.CreateLogFor(type);
            }
            return factory.CreateLogFor(instance.GetType());
        }

        //public static ISimpleLog GetOrCreateLogFor(this ISimpleLogFactory factory, Type type)
        //{
        //    return factory.GetOrCreate(type.FullName);
        //}

        //public static ISimpleLog GetOrCreateLogFor<T>(this ISimpleLogFactory factory)
        //{
        //    return factory.GetOrCreateLogFor(typeof(T));
        //}

        //public static ISimpleLog GetOrCreateLogFor(this ISimpleLogFactory factory, object instance)
        //{
        //    if (instance == null)
        //    {
        //        throw new ArgumentNullException(nameof(instance));
        //    }
        //    if (instance is Type type)
        //    {
        //        return factory.GetOrCreateLogFor(type);
        //    }
        //    return factory.GetOrCreateLogFor(instance.GetType());
        //}
    }

    #endregion
}
