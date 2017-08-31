using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace 并行开发.BeginEnd使用
{
    public class BeginEnd
    {
        public static void MainRun()
        {
            var func = new Func<string, string>(i =>
            {
                return i + "I can fly";
            });
            var state = func.BeginInvoke("yes", Callback, func);
        }

        /// <summary>
        /// Task 包装
        /// </summary>
        public static void TaskRun()
        {
            var func = new Func<string, string>(x =>
            {
                return x + "I can fly ";
            });

            Task<string>.Factory.FromAsync(func.BeginInvoke, func.EndInvoke, "yes", null).ContinueWith(x =>
            {
                Console.WriteLine(x.Result);
            });
        }

        /// <summary>
        /// 异步读写文件
        /// </summary>
        public static void AsyncReadreWriter()
        {
            var path = @"C:\Users\Fife\Desktop\面试题.txt";
            FileStream fs = new FileStream(path, FileMode.Open);
            FileInfo info = new FileInfo(path);
            byte[] b = new byte[info.Length];
            var asycState = fs.BeginRead(b, 0, b.Length, (result) =>
            {
                var file = result.AsyncState as FileStream;
                Console.WriteLine("文件内容：{0}", Encoding.Default.GetString(b));
            }, fs);
        }

        /// <summary>
        /// Task包装异步读写
        /// </summary>
        public static void TaskAsyncReadreWriter()
        {
            var path = @"C:\Users\Fife\Desktop\面试题.txt"; ;
            FileStream fs = new FileStream(path, FileMode.Open);
            FileInfo info = new FileInfo(path);
            byte[] b = new byte[info.Length];
            Task<int>.Factory.FromAsync(fs.BeginRead, fs.EndRead, b, 0, b.Length, null, TaskCreationOptions.None).ContinueWith(x =>
            {

                Console.WriteLine("文件内容：{0}", Encoding.Default.GetString(b));
            });
            Console.WriteLine("我是主线程，我不会被阻塞！");
        }

        public static void ListTaskAsyncReadreWriters()
        {
            string[] array = { @"C:\Users\Fife\Desktop\面试题0.txt", @"C:\Users\Fife\Desktop\面试题1.txt", @"C:\Users\Fife\Desktop\面试题2.txt" };
            List<Task<string>> taskList = new List<Task<string>>(3);

            foreach (var item in array)
            {
                taskList.Add(ReadAsyc(item));
            }

            Task.Factory.ContinueWhenAll(taskList.ToArray(), i =>
            {

                string result = string.Empty;
                foreach (var item in i)
                {
                    result += item.Result;
                }
                String content = new String(result.OrderByDescending(j => j).ToArray());

                Console.WriteLine("倒序结果：" + content);
            });
        }

        #region  事件操作
        /// <summary>
        /// 事件操作
        /// </summary>
        public static void EventAsync()
        {
            WebClient clinet = new WebClient();
            clinet.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(client_DownloadFileCompleted);
            clinet.DownloadFileAsync(new Uri("https://pic.cnblogs.com/avatar/883622/20160330153618.png"), "图片下载完了你懂得");
            Console.WriteLine("我是主线程，我不会被阻塞！");
        }

        private static void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine("\n" + e.UserState);
        }

        public static void DownloadTask() 
        {
             var downloadTask = DownLoadFileInTask(
                    new Uri(@"https://pic.cnblogs.com/avatar/883622/20160330153618.png"), "C://1.jpg");
             downloadTask.ContinueWith(i => {
                 Console.WriteLine("图片:" + i.Result + "下载完毕！");
             });

             Console.WriteLine("我是主线程，我不会被阻塞!");
        } 

        private static Task<string> DownLoadFileInTask(Uri address, string saveFile)
        {
            var wc = new WebClient();

            var tcs = new TaskCompletionSource<string>(address);
            //处理异步操作的一个委托
            AsyncCompletedEventHandler handler = null;

            handler = (sender, e) =>
            {

                if (e.Error != null)
                {
                    tcs.TrySetException(e.Error);
                }
                else
                {

                    if (e.Cancelled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(saveFile);
                    }
                }

                wc.DownloadFileCompleted -= handler;
            };

            wc.DownloadFileCompleted += handler;
            try
            {
                wc.DownloadFileAsync(address, saveFile);
            }
            catch (Exception ex)
            {
                wc.DownloadFileCompleted -= handler;
                tcs.TrySetException(ex);
            }
            return tcs.Task;
        }

        #endregion
        private static Task<string> ReadAsyc(string path)
        {
            FileInfo info = new FileInfo(path);
            byte[] b = new byte[info.Length];
            FileStream fs = new FileStream(path, FileMode.Open);
            Task<int> task = Task<int>.Factory.FromAsync(fs.BeginRead, fs.EndRead, b, 0, b.Length, null, TaskCreationOptions.None);
            return task.ContinueWith(x =>
            {
                return x.Result > 0 ? Encoding.Default.GetString(b) : string.Empty;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }


        public static void Callback(IAsyncResult async)
        {
            var resutl = async.AsyncState as Func<string, string>;
            Console.WriteLine(resutl.EndInvoke(async));
        }
    }
}
