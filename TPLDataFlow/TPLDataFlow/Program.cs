using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLDataFlow
{
    class Program
    {
        public static void BasicTPLDataFlow()
        {
            var actionBlock = new ActionBlock<int>(i =>
            {
                Console.WriteLine($"Processed {i}");
            });

            for (var i = 0; i < 10000; i++)
            {
                var index = i;
                Task.Run(() => actionBlock.Post(index));
            }
        }

        public static void TPLDataFlowPriority()
        {
            var actionBlock = new ActionBlock<string>(
                (job) => Console.Write($"{job}  "),
                new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 });

            var prio = new ConcurrentDictionary<int, ConcurrentQueue<string>>();
            Enumerable.Range(0, 4).ToList().ForEach(priority =>
                prio.TryAdd(priority, new ConcurrentQueue<string>())
            );

            // Consumer
            Task.Run(async () =>
            {
                string job;
                while (true)
                {
                    if (prio[0].TryDequeue(out job))
                    {
                        await actionBlock.SendAsync(job);
                        continue;
                    }

                    if (prio[1].TryDequeue(out job))
                    {
                        await actionBlock.SendAsync(job);
                        continue;
                    }

                    if (prio[2].TryDequeue(out job))
                    {
                        await actionBlock.SendAsync(job);
                        continue;
                    }

                    if (prio[3].TryDequeue(out job))
                    {
                        await actionBlock.SendAsync(job);
                    }
                }
            });

            // Producer
            while (true)
            {
                Console.WriteLine();
                var count = new Random().Next(10, 20);
                var myDict = new Dictionary<int, List<string>>();
                for (var i = 0; i < count; i++)
                {
                    var priority = new Random().Next(0, 3);
                    var val = $"{priority}a";
                    if (myDict.TryGetValue(priority, out List<string> items))
                    {
                        items.Add(val);
                    }
                    else
                    {
                        myDict[priority] = new List<string> { val };
                    }
                }

                Console.WriteLine("Produced {0} items : {1}",
                    count,
                    string.Join(", ", myDict.Values.SelectMany(v => v).ToList()));

                foreach (var (priority, items) in myDict)
                {
                    foreach (var item in items)
                        prio[priority].Enqueue(item);
                }

                Console.Write("Processed Order :   ");
                Thread.Sleep(2000);
            }
        }

        static void Main(string[] args)
        {

            // BasicTPLDataFlow();
            TPLDataFlowPriority();

            Console.ReadLine();
        }
    }
}
