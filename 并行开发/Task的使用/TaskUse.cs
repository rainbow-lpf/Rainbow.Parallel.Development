using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 并行开发.Task的使用
{
    public class TaskUse
    {
        //1：任务是架构在线程之上的，也就是说任务最终还是要抛给线程去执行。

        //2：任务跟线程不是一对一的关系，比如开10个任务并不是说会开10个线程，这一点任务有点类似线程池，但是任务相比线程池有很小的开销和精确的控制。
        public static void StartTask()
        {
            //实例化Task
            var task1 = new Task(() =>
            {
                Run1();
            });

            //从工厂中创建并执行
            var task2 = Task.Factory.StartNew(() =>
            {
                Run2();
            });

            Console.WriteLine("调用start之前****************************\n");
            //调用start前的任务状态
            Console.WriteLine("task1的状态:{0}", task1.Status);
            Console.WriteLine("task2的状态:{0}", task2.Status);
            task1.Start();
            Console.WriteLine("\n调用start之后****************************");
            Console.WriteLine("task1的状态:{0}", task1.Status);
            Console.WriteLine("task2的状态:{0}", task2.Status);
            Task.WaitAll(task1, task2);
            Console.WriteLine("\n任务执行完后的状态****************************");
            Console.WriteLine("task1的状态:{0}", task1.Status);
            Console.WriteLine("task2的状态:{0}", task2.Status);
        }

        private static void Run1()
        {
            Thread.Sleep(1000);
            Console.WriteLine("\n我是任务1");
        }

        private static void Run2()
        {
            Thread.Sleep(1000);
            Console.WriteLine("\n我是任务2");
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public static void TaskCancel()
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            Task task1 = new Task(() =>
            {
                Run1Cancel(ct);
            }, ct);
            Task task2 = new Task(Run2Cancel);
            try
            {
                task1.Start();
                task2.Start();
                Thread.Sleep(1000);
                cts.Cancel();
                Task.WaitAll(task1, task2);
            }
            catch (AggregateException ex)
            {
                foreach (var item in ex.InnerExceptions)
                {
                    Console.WriteLine("\nhi,我是OperationCanceledException：{0}\n", ex.Message);
                }
                Console.WriteLine("task1是不是被取消了？ {0}", task1.IsCanceled);
                Console.WriteLine("task2是不是被取消了？ {0}", task2.IsCanceled);
            }
        }

        private static void Run1Cancel(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            Console.WriteLine("我是任务1");
            Thread.Sleep(2000);
            ct.ThrowIfCancellationRequested();
            Console.WriteLine("我是任务1的第二部分信息");
        }
        private static void Run2Cancel()
        {
            Console.WriteLine("我是任务2");
        }


        /// <summary>
        /// Task 获取返回值
        /// </summary>
        public static void GetReturnValue()
        {
            var t1 = Task.Factory.StartNew<List<string>>(() => { return GetString(); });
            t1.Wait();
            var t2 = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("t1集合中返回的个数:" + string.Join(",", t1.Result));
            });
            //改造上面几行代码  创建一个目标 完成时 继续延续任务
            var t23 = t1.ContinueWith((i) =>
            {
                 Console.WriteLine("t1集合中返回的个数：" + string.Join(",", i.Result));
            });
        }

        private static List<string> GetString()
        {
            return new List<string> { "0", "2", "4", "8" };
        }

        public static void ContinueWithWaitAll() 
        {
            ConcurrentStack<int> stack = new ConcurrentStack<int>();

            //t1先串行
            var t1 = Task.Factory.StartNew(() => {

                stack.Push(1);
                stack.Push(2);
            });

            //t2 t3并行
            var t2 = t1.ContinueWith(t => {
                int result;
                stack.TryPop(out result);
            });
            var t3 = t1.ContinueWith(t =>
            {
                int result;
                stack.TryPop(out result);
            });
            //等待t2 t3 执行完
            Task.WaitAll(t2, t3);
            //串行
            var t4 = Task.Factory.StartNew(() => {
                stack.Push(1);
                stack.Push(2);

            });
            //t5 t6 并行
            var t5 = t4.ContinueWith(t => {

                int result;
                //尝试弹出并返回顶部的对象
                stack.TryPop(out result);
            });
            var t6 = t4.ContinueWith(t =>
            {
                int result;
                //只弹出 不移除
                stack.TryPeek(out result);
            });

            Task.WaitAll(t5,t6);

            var t7 = Task.Factory.StartNew(() => {
                Console.WriteLine("当前集合元素个数：" + stack.Count);
            });
        }
    }
}
    