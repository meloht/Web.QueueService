
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Web.QueueService.Common;

namespace BPM.QueueService.Test
{
    [TestClass]
    public class UnitTestWebApi
    {
        [TestMethod]
        public void TestApi()
        {
            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("name", "123");

            string url = "https://queueapi.azurewebsites.net/api/GetData";

            string json = GetHttpPostAsync(url, paras).GetAwaiter().GetResult();

            JsonModel obj = HttpUtils.ToObject<JsonModel>(json);

            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestApiFIFO()
        {
            var task1 = PostMethodAsync();

            Thread.Sleep(100);
            var task2 = PostMethodAsync();



            Task.WaitAll(task1, task2);

            Assert.IsTrue(task2.Result.Date > task1.Result.Date);
        }

        [TestMethod]
        public void TestIndex()
        {
            List<int> ls = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                SendMessageToRedis(ls);
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestJsonSize()
        {
            int size = 47;
            List<DateTime> arrs = new List<DateTime>();

            for (int i = 0; i < size; i++)
            {
                arrs.Add(DateTime.Now);
            }
            string ss = HttpUtils.ToJson(arrs);
            var sizes = System.Text.Encoding.UTF8.GetByteCount(ss);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestQueueSize()
        {
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            Assert.AreEqual(1, queue.Dequeue());
            queue.Enqueue(1);

            Assert.AreEqual(2, queue.Dequeue());
            queue.Enqueue(2);

            Assert.AreEqual(3, queue.Dequeue());
            queue.Enqueue(3);

            Assert.AreEqual(1, queue.Dequeue());
            queue.Enqueue(1);
        }


        [TestMethod]
        public void TestApiConcurrent()
        {
            var date = DateTime.Now;
            var dd = date.ToString("yyyy-MM-dd HH:mm:ss ffff");
            List<string> ls = new List<string>();
            int err = 0;
            List<Task<JsonModel>> listTask = new List<Task<JsonModel>>();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 50; i++)
            {
                var task = PostMethodEmptyAsync();

                listTask.Add(task);
            }

            Task.WaitAll(listTask.ToArray());

            sw.Stop();
            string time = sw.Elapsed.ToString();


            foreach (var item in listTask)
            {
                if (item.Result == null)
                {
                    err++;
                }
                else
                {
                    ls.Add(item.Result.Date.ToString());
                }
            }
            Assert.AreEqual(err, 0);


        }

        private static readonly object locker = new object();
        private static volatile int RedisIndex = 0;

        private void SendMessageToRedis(List<int> ls)
        {
            var index = RedisIndex;
            ls.Add(index);
            lock (locker)
            {
                if (RedisIndex >= 3)
                {
                    RedisIndex = 0;
                    return;
                }
                Interlocked.Increment(ref RedisIndex);
            }


        }
        private static async Task<JsonModel> PostMethodEmptyAsync()
        {
            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("name", DateTime.Now.ToString());

            string url = "https://localhost:5001/api/GetData";

            string json = await GetHttpPostAsync(url, paras);

            JsonModel obj = HttpUtils.ToObject<JsonModel>(json);

            return obj;
        }
        private static async Task<JsonModel> PostMethodAsync()
        {
            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("name", DateTime.Now.ToString());

            string url = "http://flowmasterapidev.azurewebsites.net/api/GetData";

            string json = await GetHttpPostAsync(url, paras);

            JsonModel obj = HttpUtils.ToObject<JsonModel>(json);

            return obj;
        }
        private static async Task<string> PostMethodStringAsync()
        {
            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("name", DateTime.Now.ToString());

            string url = "https://queueapi.azurewebsites.net/api/GetData";

            string json = await GetHttpPostAsync(url, paras);



            return json;
        }

        public static async Task<string> GetHttpPostAsync(string url, Dictionary<string, string> paras)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None };
            using (HttpClient httpclient = new HttpClient(handler))
            {
                try
                {
                    var content = new FormUrlEncodedContent(paras);

                    var response = await httpclient.PostAsync(url, content);

                    var responseString = await response.Content.ReadAsStringAsync();


                    return responseString;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }


            }
            return string.Empty;
        }

    }

    public class JsonModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public DateTime Date { get; set; }


        public string Ip { get; set; }
    }

}
