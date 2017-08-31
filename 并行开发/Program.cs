using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using 并行开发.BeginEnd使用;
using 并行开发.Parallel的使用;
using 并行开发.PLinq;
using 并行开发.Task的使用;
using 并行开发.同步机制;

namespace 并行开发
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Parallel的使用
            //Parallels.MainRun();
            //for 操作
            //Parallels.ForRun();
            //foreach 操作
            //Parallels.ForeachRun();
            //break操作
            //Parallels.ForBreak();
            //抛出异常
           // Parallels.ExcepationRun();
            //指定线程操作
            // Parallels.ParallelOption();
            #endregion

            #region Task的使用
            //开启Task
            //TaskUse.StartTask();
            //取消Task
            //TaskUse.TaskCancel();
            //Task获取返回值
            //TaskUse.GetReturnValue();
            //串行和并行
            //TaskUse.ContinueWithWaitAll();
            #endregion

            #region
            //Plinq.MainRun();
            //Plinq.OrderbyRun();
            //Plinq.ParallelEnumes();
            // Plinq.MapReduce();
            #endregion

            #region
            //基础用法
            //Barriers.MainRun();
            //死锁
            //Barriers.DieRun();
            //加载超时解决
            //Barriers.LoadTimeOut();
            //SpinLocks.MainRun();
            //CountdownEvents.MainRun();
          //CountdownEvents.SlimRun();
            #endregion

            #region
            BeginEnd.MainRun();
           // BeginEnd.TaskAsyncReadreWriter();
            //BeginEnd.ListTaskAsyncReadreWriters();
            BeginEnd.EventAsync();
            BeginEnd.DownloadTask();
            #endregion
            Console.ReadLine();
        }
    }
}
