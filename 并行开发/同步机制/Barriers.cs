using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 并行开发.同步机制
{
    public class Barriers
    {
        private static Barrier barrier = null;
        private static Task[] tasks = new Task[4];
        //告知其被取消
        private static CancellationTokenSource cts = new CancellationTokenSource();
        //传播有关相应的取消通知
        private static CancellationToken ct = cts.Token;

        //  屏障必须等待4个task通过SignalAndWait()来告知自己已经到达，当4个task全部达到后，我们可以通过
        //barrier.ParticipantsRemaining来获取task到达状态
        public static void MainRun()
        {
            barrier = new Barrier(tasks.Length, (i) =>
            {
                Console.WriteLine("**********************************************************");
                Console.WriteLine("\n屏障中当前阶段编号:{0}\n", i.CurrentPhaseNumber);
                Console.WriteLine("**********************************************************");
            });
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew((obj) =>
                {

                    var single = Convert.ToInt32(obj);
                    LoadUser(single);
                    //发出参与者已经到达的屏障信号 并等待其他参与者到达屏障
                    barrier.SignalAndWait();
                    LoadOrder(single);
                    barrier.SignalAndWait();
                    LoadProduct(single);
                    barrier.SignalAndWait();
                }, i);
            }
            Console.WriteLine("指定数据库中所有数据已经加载完毕！");
        }

        /// <summary>
        /// 死锁问题
        /// </summary>
        public static void DieRun()
        {
            barrier = new Barrier(tasks.Length, (i) =>
            {
                Console.WriteLine("**********************************************************");
                Console.WriteLine("\n屏障中当前阶段编号:{0}\n", i.CurrentPhaseNumber);
                Console.WriteLine("**********************************************************");
            });
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew((obj) =>
                {

                    var single = Convert.ToInt32(obj);
                    LoadUserDie(single);
                    //发出参与者已经到达的屏障信号 并等待其他参与者到达屏障
                    barrier.SignalAndWait();
                    LoadOrder(single);
                    barrier.SignalAndWait();
                    LoadProduct(single);
                    barrier.SignalAndWait();
                }, i);
            }
            Console.WriteLine("指定数据库中所有数据已经加载完毕！");

        }

        /// <summary>
        /// 超时机制
        /// </summary>
        public static void LoadTimeOut()
        {
            barrier = new Barrier(tasks.Length, (i) =>
            {
                Console.WriteLine("**********************************************************");
                Console.WriteLine("\n屏障中当前阶段编号:{0}\n", i.CurrentPhaseNumber);
                Console.WriteLine("**********************************************************");
            });
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew((obj) =>
                {

                    var single = Convert.ToInt32(obj);
                    LoadUserTimeOut(single);
                    if (!barrier.SignalAndWait(2000))
                    {
                        throw new OperationCanceledException(string.Format("我是当前任务{0},我抛出异常了！", single), ct);
                    }
                    barrier.SignalAndWait();
                    LoadOrder(single);
                    barrier.SignalAndWait();
                    LoadProduct(single);
                    barrier.SignalAndWait();
                }, i, ct);
            }
            //等待所有 Task 4s
            Task.WaitAll(tasks, 4000);

            try
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    //判断是由于处理异常而完成的任务
                    if (tasks[i].Status == TaskStatus.Faulted)
                    {
                        foreach (var item in tasks[i].Exception.InnerExceptions)
                        {
                            Console.WriteLine(item.Message);
                        }
                    }

                }
                barrier.Dispose();
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("我是总异常:{0}", ex.Message);
                //throw;
            }
        }

        private static void LoadUserTimeOut(int num)
        {
            Console.WriteLine("\n当前任务:{0}正在加载User部分数据！", num);

            if (num == 0)
            {
                //自旋转5s
                if (!SpinWait.SpinUntil(() => barrier.ParticipantsRemaining == 0, 5000))
                    return;
            }
        }
        private static void LoadUserDie(int num)
        {
            Console.WriteLine("当前任务:{0}正在加载User部分数据！", num);
            if (num == 0)
            {
                //num=0：表示0号任务
                //barrier.ParticipantsRemaining == 0：表示所有task到达屏障才会退出
                // SpinWait.SpinUntil: 自旋锁，相当于死循环
                SpinWait.SpinUntil(() => barrier.ParticipantsRemaining == 0);

            }
        }

        private static void LoadUser(int num)
        {
            Console.WriteLine("当前任务:{0}正在加载User部分数据！", num);
        }

        private static void LoadProduct(int num)
        {
            Console.WriteLine("当前任务:{0}正在加载Product部分数据！", num);
        }

        private static void LoadOrder(int num)
        {
            Console.WriteLine("当前任务:{0}正在加载Order部分数据！", num);
        }
    }
}
