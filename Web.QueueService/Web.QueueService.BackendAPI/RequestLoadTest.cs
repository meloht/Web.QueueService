
using Web.QueueService.BackendAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Web.QueueService.Common;

namespace Web.QueueService.BackendAPI
{
    public class RequestLoadTest
    {
        private  HttpClient httpClient = null;
        int TaskCount = 0;

        public RequestLoadTest(IHttpClientFactory client)
        {
            httpClient = client.CreateClient("client_1");
        }

        public int PostTest(int num)
        {
            List<Task<JsonModel>> listTask = new List<Task<JsonModel>>();


            for (int i = 0; i < num; i++)
            {
                string index = (i + 1).ToString();
                var task = PostMethodApiAsync(index);

                listTask.Add(task);
            }

            Task.WaitAll(listTask.ToArray());

            return TaskCount;
        }

        public  void InitHttpClient()
        {
            if (httpClient != null)
                return;
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            httpClient = httpClientFactory.CreateClient();
        }
        public async Task<string> GetHttpPostAsync(string url, Dictionary<string, string> paras)
        {

            try
            {
                var content = new FormUrlEncodedContent(paras);

                var response = await httpClient.PostAsync(url, content);

                var responseString = await response.Content.ReadAsStringAsync();


                return responseString;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }



            return string.Empty;
        }
        private async Task<JsonModel> PostMethodApiAsync(object index)
        {
            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("name", index.ToString());
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            string url = "http://queueapi.azurewebsites.net/api/GetData";
            // string url = "https://localhost:5001/api/GetData";
            Console.WriteLine($"begin {index.ToString()} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff")} ThreadId:{Thread.CurrentThread.ManagedThreadId}");
            DateTime dt = DateTime.Now;
            string json = await GetHttpPostAsync(url, paras);

            JsonModel obj = HttpUtils.ToObject<JsonModel>(json);

            sw.Stop();
            Interlocked.Increment(ref TaskCount);

            var ts = obj.Date - dt;
            string bl = obj == null ? "Failed" : "Success";
            Console.WriteLine($"end {TaskCount} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff")}  time:{sw.Elapsed.ToString()} result:{bl}");

            return obj;
        }
    }
}
