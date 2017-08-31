using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 并行开发.同步机制
{
    public class CountdownEvents
    {
        //我们看到有两个主要方法：Wait和Signal。每调用一次Signal相当于麻将桌上走了一个人，直到所有人都搓过麻将wait才给放行，这里同样要
        //注意也就是“超时“问题的存在性，尤其是在并行计算中，轻量级别给我们提供了”取消标记“的机制，这是在重量级别中不存在的，比如下面的
        //重载public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)，具体使用可以看前一篇文章的介绍。

        ////默认的容纳大小为“硬件线程“数
        private static CountdownEvent cde = new CountdownEvent(Environment.ProcessorCount);
        //限制同事访问某一资源或资源池的线程数
        private static  SemaphoreSlim slim = new SemaphoreSlim(Environment.ProcessorCount, 12);

        //简化版本
        private static ManualResetEventSlim mrs = new ManualResetEventSlim(false, 2047);

        public static void MainRun()
        {
            var userTaskCount = 5;
            //重置信号
            cde.Reset(userTaskCount);
            for (int i = 0; i < userTaskCount; i++)
            {
                Task.Factory.StartNew((obj) => {
                    LoadUser(obj);
                },i);
            }
            //等待所有的执行完毕
            cde.Wait();
            Console.WriteLine("\nUser表数据全部加载完毕！\n");

            var productTaskCount = 8;
            cde.Reset(productTaskCount);
            for (int i = 0; i < productTaskCount; i++)
            {
                Task.Factory.StartNew((obj) => {
                    LoadProduct(obj);
                },i);
            }
            cde.Wait();
            Console.WriteLine("\nProduct表数据全部加载完毕！\n");
            var orderTaskCount = 12;
            cde.Reset(orderTaskCount);
            for (int i = 0; i < orderTaskCount; i++)
            {
                Task.Factory.StartNew((obj) => {
                    LoadOrder(obj);
                },i);   
            }
            cde.Wait();

            Console.WriteLine("\nOrder表数据全部加载完毕！\n");

            Console.WriteLine("\n(*^__^*) 嘻嘻，恭喜你，数据全部加载完毕\n");
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SlimRun() 
        {
            for (int i = 0; i < 12; i++)
            {
                Task.Factory.StartNew((obj) => {
                    Run(obj);
                },i);
            }
        }

        public static void ManualResetRun() 
        {
            for (int i = 0; i < 12; i++)
            {
                Task.Factory.StartNew((obj) => {
                    Run2(obj);

                },i);
            }
        }

        private static void Run2(object obj) 
        {
            mrs.Wait();
            Console.WriteLine("当前时间:{0}任务 {1}已经进入。", DateTime.Now, obj);
        }
        private static void Run(object obj) 
        {
           //组织当前线程
            slim.Wait();
            Console.WriteLine("当前时间:{0}任务 {1}已经进入。", DateTime.Now, obj);
            Thread.Sleep(3000);
            slim.Release();

        }
        private static void LoadUser(object obj) 
        {
            try
            {
                Console.WriteLine("当前任务:{0}正在加载User部分数据！", obj);
            }
            catch (Exception)
            {

                throw;
            }
            finally 
            {
                //减少其计数
                cde.Signal();
            }
        }

        private static void LoadProduct(object obj)
        {
            try
            {
                Console.WriteLine("当前任务:{0}正在加载Product部分数据！", obj);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                //减少其计数
                cde.Signal();
            }
        }

        private static void LoadOrder(object obj)
        {
            try
            {
                Console.WriteLine("当前任务:{0}正在加载Order部分数据！", obj);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //减少其计数
                cde.Signal();
            }
        }
    }
}
