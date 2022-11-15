using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace concur2
{
    public interface IMultiLock
    {
        public IDisposable AcquireLock(params string[] keys);
    }

    internal class MultiLock : IMultiLock
    {
        private readonly object _lockObject = new();
        private readonly Dictionary<string, object> _monitorLocks;

        public MultiLock()
        {
            _monitorLocks = new Dictionary<string, object>();
        }

        public IDisposable AcquireLock(params string[] keys)
        {
            var markedKey = false;
            try
            {
                foreach (var currentKey in keys.OrderBy(key => key))
                    MarkKey(currentKey);
                markedKey = true;
                return new Disposer(keys, _monitorLocks);
            }
            finally
            {
                if (!markedKey)
                    foreach (var key in keys
                        .Where(key => Monitor.IsEntered(_monitorLocks[key]))
                        .OrderByDescending(key => key))
                        Monitor.Exit(_monitorLocks[key]);
            }
        }

        private void MarkKey(string key)
        {
            lock (_lockObject)
                if (!_monitorLocks.ContainsKey(key))
                    _monitorLocks[key] = new object();
            Monitor.Enter(_monitorLocks[key]);
        }
    }

    public class Disposer : IDisposable
    {
        private readonly Dictionary<string, object> _locksObjects;
        private readonly IEnumerable<string> _keys;
        public Disposer(IEnumerable<string> keys, Dictionary<string, object> locksObjects)
        {
            _locksObjects = locksObjects;
            _keys = keys;
        }

        public void Dispose()
        {
            foreach (var key in _keys.OrderByDescending(key => key))
            {
                var locksObject = _locksObjects[key];
                if (Monitor.IsEntered(locksObject))
                    Monitor.Exit(locksObject);
            }
        }
    }

    public static class Program
    {
        private static void Main()
        {
            IMultiLock multiLock = new MultiLock();

            var thread1 = new Thread(() =>
            {
                using var @lock = multiLock.AcquireLock("1", "2", "3");
                Console.WriteLine("1");
                Thread.Sleep(2000);
            });

            var thread2 = new Thread(() =>
            {
                using var @lock = multiLock.AcquireLock("1");
                Console.WriteLine("2");
            });

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();
        }
    }
}
