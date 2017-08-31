using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 并行开发.Parallel的使用
{
    public class Parallels
    {
        /// <summary>
        /// 第一：一个任务是可以分解成多个任务，采用分而治之的思想。
       //  /第二：尽可能的避免子任务之间的依赖性，因为子任务是并行执行，所以就没有谁一定在前，谁一定在后的规定了。
        /// </summary>
        public static void MainRun() 
        {
            var watch = Stopwatch.StartNew();
            watch.Start();
            Run1();
            Run2();
            Console.WriteLine("我是串行开发，总共耗时:{0}\n", watch.ElapsedMilliseconds);
            //停止时间测量 把时间重归于0
            watch.Restart();
            Parallel.Invoke(Run1, Run2);
            watch.Stop();
            Console.WriteLine("我是并行开发，总共耗时:{0}", watch.ElapsedMilliseconds);


        }
        public static void Run1() 
        {
            Console.WriteLine("我是任务一,我跑了3s");
            Thread.Sleep(3000);
        }
        public static void Run2() 
        {
            Console.WriteLine("我是任务二,我跑了5s");
            Thread.Sleep(5000);
        }

        /// <summary>
        /// 我们知道串行代码中也有一个for，但是那个for并没有用到多核，而Paraller.for它会在底层根据硬件线程的运行状况来充分的使用所有的可
        //利用的硬件线程，注意这里的Parallel.for的步行是1
        /// </summary>
        public static void ForRun() 
        {
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine("\n第{0}次比较", i);
                //表示对线程安全的无序集合
                ConcurrentBag<int> bag = new ConcurrentBag<int>();
                var watch = Stopwatch.StartNew();
                watch.Start();
                for (int j = 0; j < 50000000; j++)
                {
                    bag.Add(j);
                }
                Console.WriteLine("串行计算：集合有:{0},总共耗时：{1}", bag.Count, watch.ElapsedMilliseconds);
                //对垃圾进行回收
                GC.Collect();
                bag = new ConcurrentBag<int>();
                watch = Stopwatch.StartNew();
                watch.Start();
                Parallel.For(0, 50000000, x => { bag.Add(x); });
                Console.WriteLine("并行计算：集合有:{0},总共耗时：{1}", bag.Count, watch.ElapsedMilliseconds);
                GC.Collect();
            }
        }

        /// <summary>
        /// forEach的独到之处就是可以将数据进行分区，每一个小区内实现串行计算，分区采用Partitioner.Create实现
        /// </summary>
        public static void ForeachRun() 
        {
            for (int i = 0; i < 4; i++)
            {
                 Console.WriteLine("\n第{0}次比较", i);
                 ConcurrentBag<int> bag = new ConcurrentBag<int>();
                 var watch = Stopwatch.StartNew();
                 watch.Start();
                 for (int j = 0; j < 50000000; j++)
                 {
                     bag.Add(j);   
                 }
                 Console.WriteLine("串行计算：集合有:{0},总共耗时：{1}", bag.Count, watch.ElapsedMilliseconds);
                 GC.Collect();

                 bag = new ConcurrentBag<int>();
                 watch = Stopwatch.StartNew();
                 watch.Start();
                 Parallel.ForEach(Partitioner.Create(0, 50000000), x => {
                     for (int m = x.Item1; m < x.Item2; m++)
                     {
                         bag.Add(m);
                     }
                 });
                 Console.WriteLine("并行计算：集合有:{0},总共耗时：{1}", bag.Count, watch.ElapsedMilliseconds);
                 GC.Collect();
            }
        }

        /// <summary>
        /// 如何退出for循环
        /// </summary>
        /// ParallelLoopState，该实例提供了Break和Stop方法来帮我们实现。
        ///Break: 当然这个是通知并行计算尽快的退出循环，比如并行计算正在迭代100，那么break后程序还会迭代所有小于100的。
        ///Stop：这个就不一样了，比如正在迭代100突然遇到stop，那它啥也不管了，直接退出。
        public static void ForBreak() 
        {
            ConcurrentBag<int> bag = new ConcurrentBag<int>();
            var watch = Stopwatch.StartNew();
            watch.Start();
            Parallel.For(0, 5000000, (i, state) => {
                if (bag.Count == 300000)
                {
                    state.Break();
                    return;
                }
                bag.Add(i);
            });
            Console.WriteLine("当前集合有{0}个元素。", bag.Count);
            bag = new ConcurrentBag<int>();
            Parallel.ForEach(Partitioner.Create(0, 5000000), (x, state) => {
                for (int j = x.Item1; j < x.Item2; j++)
                {
                    if (bag.Count == 20000)
                    {
                        state.Break();
                        return;
                    }
                    bag.Add(j);
                }
            });
            Console.WriteLine("当前集合有{0}个元素。", bag.Count);
        }

        /// <summary>
        /// 用指定的线程数量去执行
        /// </summary>
        public static void ParallelOption()
        {
            ParallelOptions options = new ParallelOptions();
            var bag = new ConcurrentBag<int>();
            var watch = Stopwatch.StartNew();
            //指定使用硬件线程数量
            options.MaxDegreeOfParallelism = 30;

            Parallel.For(0, 100000000, options, i =>
            {
                bag.Add(i);
            });
            Console.WriteLine("并行计算：集合有:{0},总共耗时：{1}", bag.Count, watch.ElapsedMilliseconds);
            watch.Stop();


        }
        /// <summary>
        /// 抛出异常
        /// </summary>
        public static void ExcepationRun() 
        {
            try
            {
                Parallel.Invoke(ExRun1, ExRun2);
            }
            catch (AggregateException ex)
            {
                foreach (var item in ex.InnerExceptions)
                {
                    Console.WriteLine(item.Message);
                }
            }
        }

       private static void ExRun1()
        {
            Thread.Sleep(3000);
            throw new Exception("我是任务1抛出的异常");
        }
       private static void ExRun2()
        {
            Thread.Sleep(3000);
            throw new Exception("我是任务1抛出的异常");
        }
    }
}
