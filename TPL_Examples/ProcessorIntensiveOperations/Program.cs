using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessorIntensiveOperations
{
    class Program
    {
        static HashSet<int> threads = new HashSet<int>();
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            int maxIterations = 300000;

            sw.Reset();
            sw.Start();
            // Open your Task manager and get a view of
            // Your processor Cores...this test should not 
            // really show anything happening
            // 1 core...1 thread
            var list = RunSyncronousCPUTest(maxIterations);
            sw.Stop();
            Console.WriteLine("Primes: Elapsed time with Syncronous: {0} - {1} threads", sw.Elapsed, threads.Count);
            threads.Clear();
            Console.ReadLine();

            sw.Reset();
            sw.Start();
            //  this test will slaughter your proc
            //  ALL cores...multiple threads (8 or more)
            RunCPUTestAsync(maxIterations);
            sw.Stop();
            Console.WriteLine("Primes: Elapsed time with Asyncronous: {0} - {1} threads", sw.Elapsed, threads.Count);
            threads.Clear();
            Console.ReadLine();

            sw.Reset();
            sw.Start();
            //  this test will slaughter your proc as well.
            //  All cores...probably even more threads...
            RunParrallelCPUTest(maxIterations);
            sw.Stop();
            Console.WriteLine("Primes: Elapsed time with Parrallel: {0} - {1} threads", sw.Elapsed, threads.Count);
            threads.Clear();
            Console.ReadLine();

        }
        private static void RunCPUTestAsync(int maxIterations)
        {
            var tasks = new List<Task<bool>>();
            for (int i = 0; i < maxIterations; i++)
            {
                var val = i;
                Task<bool> task = Task.Run<bool>(() => IsPrime(val));
                tasks.Add(task);
            }
            Task<int>.WaitAll(tasks.ToArray());
        }

        private static List<int> RunSyncronousCPUTest(int maxIterations)
        {
            var list = new List<int>();
            for (int i = 1; i < maxIterations; i++)
            {
                if (IsPrime(i))
                    list.Add(i);
            }
            return list;
        }

        private static List<int> RunParrallelCPUTest(int maxIterations)
        {
            var list = new List<int>();
            Parallel.For(0, maxIterations, (val) =>
            {
                if (IsPrime(val))
                    list.Add(val);
                return;
            });
            return list;
        }

        private static bool IsPrime(int x)
        {
            var threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (!threads.Contains(threadid))
            {
                threads.Add(threadid);
            }
            bool ans = true;
            for (int i = 2; i < x; i++)
            {
                if (x % i == 0)
                {
                    ans = false;
                    break;
                }
            }
            return ans;
        }

    }
}
