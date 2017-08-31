using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 并行开发.PLinq
{
    public class Plinq
    {
        //plinq的查询引擎会尽量利用cpu的所有硬件线程。
        /// <summary>
        /// 基本使用
        /// </summary>
        public static void MainRun()
        {
            var dicList = LoadData();
            var watch = Stopwatch.StartNew();
            //串行语句
            watch.Start();
            var query1 = (from n in dicList.Values where n.Age > 20 && n.Age < 25 select n).ToList();
            watch.Stop();
            Console.WriteLine("串行计算Linq耗费时间：{0}", watch.ElapsedMilliseconds);

            watch.Restart();
            var query1Lambda = dicList.Values.Where(s => s.Age > 20 && s.Age < 25).ToList();
            watch.Stop();
            Console.WriteLine("串行计算Lambda耗费时间：{0}", watch.ElapsedMilliseconds);

            //以下是并行语句
            watch.Restart();
            var query2 = (from n in dicList.Values.AsParallel() where n.Age > 20 && n.Age < 25 select n).ToList();
            watch.Stop();
            Console.WriteLine("并行计算Linq耗费时间：{0}", watch.ElapsedMilliseconds);

            watch.Restart();
            var query2Lambda = dicList.Values.AsParallel().Where(s => s.Age > 20 && s.Age < 25).ToList();
            watch.Stop();
            Console.WriteLine("并行计算Lambda耗费时间：{0}", watch.ElapsedMilliseconds);

        }

        /// <summary>
        /// Orderby使用 和WithDegreeOfParallelism(Environment.ProcessorCount-1) 使用
        /// </summary>
        public static void OrderbyRun()
        {
            var dicList = LoadData();
            //为了不让并行计算占用全部的硬件线程，或许可能要留一个线程做其他事情 WithDegreeOfParallelism(Environment.ProcessorCount-1)
            var query2 = (from n in dicList.Values.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount - 1) where n.Age > 20 && n.Age < 25 select n).ToList();
            Console.WriteLine("默认的时间排序如下：");
            query2.Take(10).ToList().ForEach((i) =>
            {
                Console.WriteLine(i.CreateTime);
            });
            var query2Orderby = (from n in dicList.Values.AsParallel() where n.Age > 20 && n.Age < 25 orderby n.CreateTime descending select n).ToList();
            Console.WriteLine("排序后的时间排序如下：");
            query2Orderby.Take(10).ToList().ForEach((i) =>
            {
                Console.WriteLine(i.CreateTime);
            });
        }

        /// <summary>
        /// 了解ParallelEnumerable类
        /// </summary>
        public static void ParallelEnumes()
        {
            ConcurrentBag<int> bag = new ConcurrentBag<int>();
            var list = ParallelEnumerable.Range(0, 10000);
            list.ForAll((i) =>
            {
                bag.Add(i);
            });
            Console.WriteLine("bag集合中元素个数有:{0}", bag.Count);
            Console.WriteLine("list集合中元素个数总和为:{0}", list.Sum());
            Console.WriteLine("list集合中元素最大值为:{0}", list.Max());
            Console.WriteLine("list集合中元素第一个元素为:{0}", list.FirstOrDefault());
        }

        /// <summary>
        /// MapReduce是一种编程模型，用于大规模数据集（大于1TB）的并行运算。概念"Map（映射）"和"Reduce（归约）"，是它们的主要思想，都是从函数式编程语言里借来的，还有从矢量编程语言里借来的特性。它极大地方便了编程人员在不会分布式并行编程的情况下，将自己的程序运行在分布式系统上。 当前的软件实现是指定一个Map（映射）函数，用来把一组键值对映射成一组新的键值对，指定并发的Reduce（归约）函数，用来保证所有映射的键值对中的每一个共享相同的键组。
        /// </summary>
        public static void MapReduce()
        {
            //map：  也就是“映射”操作，可以为每一个数据项建立一个键值对，映射完后会形成一个键值对的集合。
            //reduce：“化简”操作，我们对这些巨大的“键值对集合“进行分组，统计等等。
            List<Student> list = new List<Student>()
            {
            new Student(){ ID=1, Name="jack", Age=20},
            new Student(){ ID=1, Name="mary", Age=25},
            new Student(){ ID=1, Name="joe", Age=29},
            new Student(){ ID=1, Name="Aaron", Age=25},
            };
            //我们会对age建立一组键值对
            var map = list.AsParallel().ToLookup(i=>i.Age,count=>1);
            //化简统计
            var reduce = from IGrouping<int, int> singMap in map.AsParallel()
                         select new { 
                           Age=singMap.Key,
                           Count=singMap.Count()
                         };
            reduce.ForAll(i=> {
                Console.WriteLine("当前Age={0}的人数有:{1}人", i.Age, i.Count);
            });
        }
        private static ConcurrentDictionary<int, Student> LoadData()
        {
            ConcurrentDictionary<int, Student> dic = new ConcurrentDictionary<int, Student>();
            //启用计算机中所有闲置的线程去操作计算
            Parallel.For(0, 15000000, (i) =>
            {
                var single = new Student()
                {
                    ID = i,
                    Name = "hxc" + i,
                    Age = i % 151,
                    CreateTime = DateTime.Now.AddSeconds(i)
                };
                dic.TryAdd(i, single);
            });
            return dic;
        }

        public class Student
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public DateTime CreateTime { get; set; }
        }
    }
}
