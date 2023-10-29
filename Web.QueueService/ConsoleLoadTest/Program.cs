using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace ConsoleLoadTest
{
    class Program
    {
        static int TaskCount = 0;
        private static IHttpClientFactory httpClientFactory;
        private static HttpClient client;
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

            client = httpClientFactory.CreateClient();

            List<Task<JsonModel>> listTask = new List<Task<JsonModel>>();


            for (int i = 0; i < 200; i++)
            {
                string index = (i + 1).ToString();
                var task = PostMethodApiAsync(index);

                listTask.Add(task);
            }

            Task.WaitAll(listTask.ToArray());

            Console.WriteLine("ok");

        }

        private static void MutiTestSync()
        {
            for (int i = 0; i < 100; i++)
            {
                ThreadPool.QueueUserWorkItem(PostMethodFlow);
            }
        }
        private static async Task<JsonModel> PostMethodApiAsync(object index)
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

            JsonModel obj = ToObject<JsonModel>(json);

            sw.Stop();
            Interlocked.Increment(ref TaskCount);

            var ts = obj.Date - dt;
            string bl = obj == null ? "Failed" : "Success";
            Console.WriteLine($"end {TaskCount} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff")}  time:{sw.Elapsed.ToString()} result:{bl}");

            return obj;
        }

        private static async Task<JsonModel> PostMethodFlowAsync()
        {
            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("name", DateTime.Now.ToString());
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            string url = "https://apidev.azurewebsites.net/api/GetDataLazy";
            Console.WriteLine($"begin  {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff")}");
            string json = await GetHttpPostAsync(url, paras);

            JsonModel obj = ToObject<JsonModel>(json);

            sw.Stop();
            Interlocked.Increment(ref TaskCount);
            string bl = obj == null ? "Failed" : "Success";
            Console.WriteLine($"end {TaskCount}  {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff")}  time:{sw.Elapsed.ToString()} result:{bl}");

            return obj;
        }
        private static void PostMethodFlow(object state)
        {
            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("name", DateTime.Now.ToString());
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            string url = "https://apidev.azurewebsites.net/api/GetData";
            Console.WriteLine($"begin  {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff")}");
            string json = GetHttpPostAsync(url, paras).GetAwaiter().GetResult();

            JsonModel obj = ToObject<JsonModel>(json);

            sw.Stop();
            Interlocked.Increment(ref TaskCount);
            string bl = obj == null ? "Failed" : "Success";
            Console.WriteLine($"end {TaskCount} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff")}  time:{sw.Elapsed.ToString()} result:{bl}");

        }
        public static async Task<string> GetHttpPostAsync(string url, Dictionary<string, string> paras)
        {

            try
            {
                var content = new FormUrlEncodedContent(paras);

                var response = await client.PostAsync(url, content);

                var responseString = await response.Content.ReadAsStringAsync();


                return responseString;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }



            return string.Empty;
        }


        private const string con = "Endpoint=sb://queue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=";
        private const string queueName = "queuetest";


        public static T ToObject<T>(string json)
        {
            try
            {
                JsonSerializerSettings setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                return JsonConvert.DeserializeObject<T>(json, setting);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return default(T);
        }

        public static string ToJson(object obj)
        {
            try
            {
                JsonSerializerSettings setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                return JsonConvert.SerializeObject(obj, setting);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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