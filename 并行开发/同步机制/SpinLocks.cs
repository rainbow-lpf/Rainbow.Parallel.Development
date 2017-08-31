using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 并行开发.同步机制
{
    public class SpinLocks
    {
        //自炫锁
       private static SpinLock slock = new SpinLock(false);
       private static int sum1 = 0;
       private static int sum2 = 0;

       public static void MainRun() 
       {
           Task [] tasks=new  Task[100];
           for (int i = 1; i <= 100; i++)
           {
               tasks[i-1] = Task.Factory.StartNew((num) => {
                   Add1((int)num);
                   Add2((int)num);
               },i);
           }
           Task.WaitAll(tasks);
           Console.WriteLine("Add1数字总和:{0}", sum1);
           Console.WriteLine("Add2数字总和:{0}", sum2);
       }

        //无锁
       private static void Add1(int num) 
       {
           Thread.Sleep(200);
           sum1 += num;
       }
       private static void Add2(int num) 
       {
           bool LockTaken = false;
           Thread.Sleep(100);
           try
           {
               //获取锁
               slock.Enter(ref LockTaken);
               sum2 += num;
           }
           catch (Exception)
           {

               throw;
           }
           finally {

               if (LockTaken) 
               {
                   //释放锁
                   slock.Exit(false);
               }
           }

       }
    }
}
