using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigFileOperations
{
    class Program
    {
        static HashSet<int> threadlist = new HashSet<int>();
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();

            var dir = @"bigfiletest";
            Directory.CreateDirectory(dir);
            var dataSize = 10014304;
            var NumberOfFilesToCreate = 25;

            //  Create an array of random bytes to write
            byte[] testData = GenerateTestData(dataSize);
            sw.Start();
            RunSynchronousFileCreationTest(NumberOfFilesToCreate, dir, testData);
            sw.Stop();
            Console.WriteLine(
                "Elapsed time with Synchronous call:\r\n\t{0} ms\r\n\t{1} diff threads",
                sw.ElapsedMilliseconds, threadlist.Count);

            RemoveFiles(dir);
            threadlist.Clear();
            sw.Reset();

            sw.Start();
            RunAsyncFileCreationTest(NumberOfFilesToCreate, dir, testData);
            sw.Stop();
            Console.WriteLine(
                "Elapsed time with TPL (await Task & WhenAll):\r\n\t{0} ms\r\n\t{1} diff threads",
                sw.ElapsedMilliseconds, threadlist.Count);

            RemoveFiles(dir);
            threadlist.Clear();
            sw.Reset();

            sw.Start();
            RunParrallelFileCreationTest(NumberOfFilesToCreate, dir, testData);
            sw.Stop();
            Console.WriteLine(
                "Elapsed time with Parrallel.For (but synchronous method):\r\n\t{0} ms\r\n\t{1} diff threads",
                sw.ElapsedMilliseconds, threadlist.Count);

            RemoveFiles(dir);
            threadlist.Clear();
            sw.Reset();

            sw.Start();
            RunParrallelAsyncFileCreationTest(NumberOfFilesToCreate, dir, testData);
            sw.Stop();
            Console.WriteLine(
                "Elapsed time with Parrallel.For (But Async with await):\r\n\t{0} ms\r\n\t{1} diff threads",
                sw.ElapsedMilliseconds, threadlist.Count);
            RemoveFiles(dir);
            Console.ReadLine();
            Directory.Delete(dir);
        }


        // Misc Methods
        private static void RemoveFiles(string dir)
        {
            foreach (var item in Directory.GetFiles(dir))
            {
                File.Delete(item);
            }
        }
        private static byte[] GenerateTestData(int dataSize)
        {
            byte[] testData = new byte[dataSize];
            var rand = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < testData.Length; i++)
            {
                testData[i] = (byte)rand.Next(0, 256);
            }
            return testData;
        }

        // Test Methods

        private static void RunSynchronousFileCreationTest(int numTimes, string dir, byte[] text)
        {
            for (int i = 0; i < numTimes; i++)
            {
                WriteBigFile(Path.Combine(dir, Path.GetRandomFileName()), text);
            }
        }
        private static async void RunAsyncFileCreationTest(int numtimes, string dir, byte[] text)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < numtimes; i++)
            {
                Task t = WriteBigFileAsync(Path.Combine(dir, "tempfile-async"+i.ToString() ), text);
                tasks.Add(t);
            }

            await Task.WhenAll(tasks.ToArray());
        }
        private static void RunParrallelFileCreationTest(int numTimes, string dir, byte[] text)
        {
            Parallel.For(0, numTimes, (x) => WriteBigFile(Path.Combine(dir, Path.GetRandomFileName()), text));
        }
        private static void RunParrallelAsyncFileCreationTest(int numTimes, string dir, byte[] text)
        {
            Parallel.For(0, numTimes, async (x) => await WriteBigFileAsync(Path.Combine(dir, Path.GetRandomFileName()), text));
        }

        //  Worker methods
        public static void WriteBigFile(string path, byte[] text)
        {
            var threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (!threadlist.Contains(threadid))
            {
                threadlist.Add(threadid);
            }
            using (FileStream strm = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: false))
            {
                strm.Write(text, 0, text.Length);
            };
        }
        public static async Task WriteBigFileAsync(string path, byte[] text)
        {
            var threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (!threadlist.Contains(threadid))
            {
                threadlist.Add(threadid);
            }
            using (FileStream strm = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await strm.WriteAsync(text, 0, text.Length);
            };
        }
    }
}
