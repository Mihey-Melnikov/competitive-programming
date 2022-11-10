using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace concur1
{
    internal static class Program
    {
        private static int _lastManagedThreadId;
        private static readonly List<DateTimeOffset> Measures = new();

        private static void Main()
        {
            var quantums = new List<TimeSpan>();
            var currentProcess = Process.GetCurrentProcess();
            var firstThread = new Thread(() => { Run(TimeSpan.FromSeconds(3)); });
            var secondThread = new Thread(() => { Run(TimeSpan.FromSeconds(3)); });
            currentProcess.ProcessorAffinity = (IntPtr)(1 << (Environment.ProcessorCount - 1));

            firstThread.Start();
            secondThread.Start();
            firstThread.Join();
            secondThread.Join();

            for (var i = 0; i < Measures.Count - 1; i++)
                quantums.Add(Measures[i + 1] - Measures[i]);

            Console.WriteLine($"{quantums.Average(quantum => quantum.TotalMilliseconds)} ms");
        }
        private static void Run(TimeSpan time)
        {
            var currentManagedThreadId = Environment.CurrentManagedThreadId;
            var timer = Stopwatch.StartNew();
            
            while (timer.Elapsed < time)
            {
                if (currentManagedThreadId == _lastManagedThreadId) 
                    continue;
                _lastManagedThreadId = currentManagedThreadId;
                Measures.Add(DateTimeOffset.Now);
            }
        }
    }
}