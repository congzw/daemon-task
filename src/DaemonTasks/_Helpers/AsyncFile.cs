using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Common
{
    public class AsyncFile
    {
        private AsyncFile()
        {
            AutoCreateDirectoryIfNotExist = true;
        }

        public static AsyncFile Instance = new AsyncFile();

        private readonly AsyncFileLocks _fileLocks = new AsyncFileLocks();

        public bool AutoCreateDirectoryIfNotExist { get; set; }

        public Task<string> ReadAllText(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Task.FromResult((string)null);
            }
            return Task.Run(() =>
            {
                lock (_fileLocks.TryGetLock(filePath))
                {
                    if (!File.Exists(filePath))
                    {
                        return Task.FromResult((string)null);
                    }
                    return Task.FromResult(File.ReadAllText(filePath));
                }
            });
        }

        public Task WriteAllText(string filePath, string content)
        {
            if (string.IsNullOrWhiteSpace(filePath) || content == null)
            {
                return Task.FromResult(0);
            }

            return Task.Run(() =>
            {
                lock (_fileLocks.TryGetLock(filePath))
                {
                    if (AutoCreateDirectoryIfNotExist)
                    {
                        CreateDirectoryIfNotExist(filePath);
                    }
                    File.WriteAllText(filePath, content, Encoding.UTF8);
                }
            });
        }
        
        public Task AppendAllText(string filePath, string content, bool appendLine = false)
        {
            if (string.IsNullOrWhiteSpace(filePath) || content == null)
            {
                return Task.FromResult(0);
            }

            return Task.Run(() =>
            {
                lock (_fileLocks.TryGetLock(filePath))
                {
                    if (AutoCreateDirectoryIfNotExist)
                    {
                        CreateDirectoryIfNotExist(filePath);
                    }

                    if (appendLine)
                    {
                        content = content + Environment.NewLine;
                    }
                    File.AppendAllText(filePath, content, Encoding.UTF8);
                }
            });
        }

        private static void CreateDirectoryIfNotExist(string filePath)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        internal class AsyncFileLocks
        {
            private static readonly object Lock = new object();
            private readonly IDictionary<string, object> _locks = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            public object TryGetLock(string filePath)
            {
                lock (Lock)
                {
                    if (!_locks.ContainsKey(filePath))
                    {
                        _locks.Add(filePath, new object());
                    }

                    return _locks[filePath];
                }
            }
        }
    }
}
