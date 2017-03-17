using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleApplication2
{
    class Program
    {
        static HashSet<int> threadlist = new HashSet<int>();
        static void Main(string[] args)
        {
            var sites = new List<string>()
            {
                "http://www.cnn.com",
                "http://www.centriq.com",
                "http://www.foxnews.com",
                "http://www.microsoft.com"
            };


            var sw = new Stopwatch();
            sw.Start();
            RunSynch(sites);
            sw.Stop();
            Console.WriteLine("Elapsed Sync:  {0}\r\nthread count: {1}", sw.ElapsedMilliseconds, threadlist.Count);
            threadlist.Clear();

            sw.Reset();
            sw.Start();
            var results = RunAsync(sites).Result;
            sw.Stop();
            Console.WriteLine("Elapsed Async:  {0}\r\nthread count: {1}", sw.ElapsedMilliseconds, threadlist.Count);
            threadlist.Clear();

            sw.Reset();
            sw.Start();
            RunParallelSync(sites);
            sw.Stop();
            Console.WriteLine("Elapsed Parallel With Sync:  {0}\r\nthread count: {1}", sw.ElapsedMilliseconds, threadlist.Count);
            threadlist.Clear();

            sw.Reset();
            sw.Start();
            RunParallel(sites);
            sw.Stop();
            Console.WriteLine("Elapsed Parallel With Async:  {0}\r\nthread count: {1}", sw.ElapsedMilliseconds, threadlist.Count);
            threadlist.Clear();
        }

        private static void RunSynch(List<string> sites)
        {
            //  No way to call this synchronously - so this man-handles it
            //  It fires off the async and then BLOCKS until it is done.
            using (var webclient = new HttpClient())
            {
                foreach (var item in sites)
                {
                    var threadid = Thread.CurrentThread.ManagedThreadId;
                    if (!threadlist.Contains(threadid))
                    {
                        threadlist.Add(threadid);
                    }

                    Task<string> t = webclient.GetStringAsync(new Uri(item));
                    t.Wait(); // <-  blocks
                    var data = t.Result;
                }
            }
        }
        private static async Task<string[]> RunAsync(List<string> sites)
        {
            using (var webclient = new HttpClient())
            {
                var tasklist = new List<Task<string>>();
                foreach (var item in sites)
                {
                    var threadid = Thread.CurrentThread.ManagedThreadId;
                    if (!threadlist.Contains(threadid))
                    {
                        threadlist.Add(threadid);
                    }
                    Task<string> t = webclient.GetStringAsync(new Uri(item));
                    tasklist.Add(t);
                }
                return await Task.WhenAll(tasklist);
            }
        }
        private static void RunParallelSync(List<string> sites)
        {
            var webclient = new HttpClient();
            Parallel.ForEach(sites, (site) =>
                        {
                            var threadid = Thread.CurrentThread.ManagedThreadId;
                            if (!threadlist.Contains(threadid))
                            {
                                threadlist.Add(threadid);
                            }

                            Task<string> t = webclient.GetStringAsync(new Uri(site));
                            t.Wait(); // <-  blocks
                            var data = t.Result;
                        });
        }
        private static void RunParallel(List<string> sites)
        {
            var webclient = new HttpClient();
            Parallel.ForEach(sites, async (site) => 
                {
                    var threadid = Thread.CurrentThread.ManagedThreadId;
                    if (!threadlist.Contains(threadid))
                    {
                        threadlist.Add(threadid);
                    }
                    await webclient.GetStringAsync(new Uri(site));
                });
        }
    }
}
