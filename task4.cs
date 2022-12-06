using System;
using System.Collections.Generic;
using System.Threading;

namespace CustomThreadPool
{
    public class MyThreadPoolWrapper : IThreadPool
    {
        private long _currentTask;
        public void EnqueueAction(Action action)
        {
            MyThreadPool.AddAction(delegate
            {
                action.Invoke();
                Interlocked.Increment(ref _currentTask);
            });
        }

        public long GetTasksProcessedCount() => _currentTask;
    }

    public static class MyThreadPool
    {
        private static Queue<Action> _actionsQueue = new Queue<Action>();
        private static Dictionary<int, WorkStealingQueue<Action>> _actionsDict = new Dictionary<int, WorkStealingQueue<Action>>();
        static MyThreadPool()
        {
            void Worker()
            {
                while (true)
                {
                    Action currentAction = delegate { };
                    while (_actionsDict[Thread.CurrentThread.ManagedThreadId].LocalPop(ref currentAction))
                        currentAction.Invoke();
                    
                    var tryFlag = true;
                    tryFlag = TryDequeueAndFindFlag(tryFlag);
                    
                    if (!tryFlag)
                        tryFlag = TryStealActionPool(tryFlag);
                    if (!tryFlag)
                        TryDequeueElseWait();
                }
            }
            RunBackgroundThreads(Worker, 16);
        }

        private static bool TryDequeueAndFindFlag(bool flag)
        {
            lock (_actionsQueue)
            {
                if (_actionsQueue.TryDequeue(out var action))
                    _actionsDict[Thread.CurrentThread.ManagedThreadId].LocalPush(action);
                else
                    flag = false;
            }
            return flag;
        }

        private static bool TryStealActionPool(bool flag)
        {
            foreach (var threadPool in _actionsDict)
            {
                Action action = delegate { };
                if (!threadPool.Value.TrySteal(ref action)) continue;
                _actionsDict[Thread.CurrentThread.ManagedThreadId].LocalPush(action);
                flag = true;
                break;
            }
            return flag;
        }

        private static void TryDequeueElseWait()
        {
            lock (_actionsQueue)
            {
                if (_actionsQueue.TryDequeue(out var action))
                    _actionsDict[Thread.CurrentThread.ManagedThreadId].LocalPush(action);
                else
                    Monitor.Wait(_actionsQueue);
            }
        }

        public static void AddAction(Action action)
        {
            lock (_actionsQueue)
            {
                _actionsQueue.Enqueue(action);
                Monitor.Pulse(_actionsQueue);
            }
        }

        private static Thread[] RunBackgroundThreads(Action action, int count)
        {
            var threads = new List<Thread>();
            for (var i = 0; i < count; i++)
                threads.Add(RunBackgroundThread(action));
            return threads.ToArray();
        }

        private static Thread RunBackgroundThread(Action action)
        {
            var thread = new Thread(() => action()) { IsBackground = true };
            _actionsDict[thread.ManagedThreadId] = new WorkStealingQueue<Action>();
            thread.Start();
            return thread;
        }
    }
}