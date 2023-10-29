using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using Web.QueueService.BackendAPI.Models;
using Web.QueueService.Common;

namespace Web.QueueService.BackendAPI.Controllers
{
    public class ApiController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IHttpClientFactory _httpClient;
        public ApiController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetData(string name)
        {
            string body = "";
            JsonModel model = new JsonModel();
            model.Date = DateTime.Now;
            model.Id = Guid.NewGuid().ToString();
            model.Name = name ?? body;
            model.Ip = Request.Host.Value.ToString();

            logger.Info($"Request begin==========ThreadId:{ Thread.CurrentThread.ManagedThreadId}===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));

            //Thread.Sleep(10000);
            return Json(model);
        }

        [HttpPost]
        public JsonResult GetDataLazy(string name)
        {
            string body = "";
            JsonModel model = new JsonModel();
            model.Date = DateTime.Now;
            model.Id = Guid.NewGuid().ToString();
            model.Name = name ?? body;
            model.Ip = Request.Host.Value.ToString();

            logger.Info($"Request begin==========ThreadId:{ Thread.CurrentThread.ManagedThreadId}===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
            Thread.Sleep(3000);
            return Json(model);
        }

        [HttpGet]
        public JsonResult GetDataById(string id)
        {
            JsonModel model = new JsonModel();
            model.Date = DateTime.Now;
            model.Id = Guid.NewGuid().ToString();
            model.Name = id;
            model.Ip = Request.Host.Value.ToString();

            logger.Info($"Request begin==========ThreadId:{ Thread.CurrentThread.ManagedThreadId}===" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
            //Thread.Sleep(10000);
            return Json(model);
        }
        [HttpGet]
        public async Task<JsonResult> GetDataLazyById(string id)
        {
            JsonModel model = new JsonModel();
            model.Date = DateTime.Now;
            model.Id = Guid.NewGuid().ToString();
            model.Name = id;
            model.Ip = Request.Host.Value.ToString();


            //Thread.Sleep(3000);
            await Task.Delay(3000);

            return Json(model);
        }

        [HttpGet]
        public ActionResult GetFileById(string id)
        {
            var stream = System.IO.File.Open(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Teradata.Client.Provider.dll"), FileMode.Open);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Octet, "Teradata.Client.Provider.dll");
        }

        [HttpPost]
        public ActionResult GetLargeJson(int maxSize)
        {
            int size = maxSize;
            List<DateTime> arrs = new List<DateTime>();

            for (int i = 0; i < size; i++)
            {
                arrs.Add(DateTime.Now);
            }
            return Json(arrs);
        }

        [HttpPost]
        public ActionResult GetFileByName(string name)
        {

            var stream = System.IO.File.Open(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Teradata.Client.Provider.dll"), FileMode.Open);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Octet, name);
        }

        [HttpPost]
        public ActionResult GetDbJson(int maxSize)
        {
            int size = maxSize;
            List<string> arrs = new List<string>();

            for (int i = 0; i < size; i++)
            {
                arrs.Add(i.ToString());
            }

            DbTimeModel result = new DbTimeModel();

            JsonDocModel docModel = new JsonDocModel();

            docModel.id = Guid.NewGuid().ToString();
            docModel.List = arrs;

            result.StringSize = HttpUtils.CountSize(System.Text.Encoding.UTF8.GetByteCount(HttpUtils.ToJson(arrs)));

            //using (CosmosDBClient client = new CosmosDBClient(Startup.Config.CosmosDb))
            //{
            //    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            //    sw.Start();
            //    var doc = client.CreateItemAsync<JsonDocModel>(docModel).GetAwaiter().GetResult();
            //    sw.Stop();
            //    result.CosmosDbCreateTime = sw.Elapsed.ToString();

            //    sw.Restart();
            //    client.GetItem<JsonDocModel>(docModel.id);

            //    sw.Stop();
            //    result.CosmosDbReadTime = sw.Elapsed.ToString();

            //    sw.Restart();
            //    client.DeleteItem(docModel.id);
            //    sw.Stop();
            //    result.CosmosDbDeleteTime = sw.Elapsed.ToString();


            //    sw.Restart();
            //    RedisResponseClient.Set<JsonDocModel>(docModel, docModel.id);
            //    sw.Stop();
            //    result.RedisCreateTime = sw.Elapsed.ToString();

            //    sw.Restart();
            //    RedisResponseClient.Get<JsonDocModel>(docModel.id);
            //    sw.Stop();
            //    result.RedisReadTime = sw.Elapsed.ToString();

            //    sw.Restart();
            //    RedisResponseClient.Remove(docModel.id);
            //    sw.Stop();
            //    result.RedisDeleteTime = sw.Elapsed.ToString();
            //}



            return Json(result);

        }

        private static readonly StorageBlobClient blobClient = new StorageBlobClient("DefaultEndpointsProtocol=https;AccountName=;AccountKey=;EndpointSuffix=core.windows.net",
            "requestcontain");
        [HttpPost]
        public JsonResult BlobJson(int maxSize)
        {
            int size = maxSize;

            List<DateTime> arrs = new List<DateTime>();

            for (int i = 0; i < size; i++)
            {
                arrs.Add(DateTime.Now);
            }
            string newguid = Guid.NewGuid().ToString();

            JsonTimeTestModel model = new JsonTimeTestModel();


            string json = HttpUtils.ToJson(arrs);
            model.StringSize = HttpUtils.CountSize(System.Text.Encoding.UTF8.GetByteCount(json));

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            byte[] jsonbs = System.Text.Encoding.UTF8.GetBytes(json);

            using (var stream = new MemoryStream(jsonbs))
            {
                sw.Start();
                blobClient.UploadFileAsync(stream, newguid).GetAwaiter().GetResult();
                sw.Stop();
            }

            model.CreateTime = sw.Elapsed.ToString();

            using (var stream = new MemoryStream())
            {
                sw.Restart();
                blobClient.DownLoadFile(stream, newguid);

                stream.Position = 0;
                StreamReader sr = new StreamReader(stream);
                string ss = sr.ReadToEnd();
                sr.Close();
                sw.Stop();
            }
            model.ReadTime = sw.Elapsed.ToString();

            sw.Restart();

            blobClient.DeleteFile(newguid);
            sw.Stop();

            model.DeleteTime = sw.Elapsed.ToString();

            return Json(model);

        }

        [HttpPost]
        public JsonResult RedisJson(int maxSize)
        {
            int size = maxSize;
            List<DateTime> arrs = new List<DateTime>();

            for (int i = 0; i < size; i++)
            {
                arrs.Add(DateTime.Now);
            }

            JsonTimeTestModel result = new JsonTimeTestModel();

            string id = Guid.NewGuid().ToString();

            string json = HttpUtils.ToJson(arrs);
            result.StringSize = HttpUtils.CountSize(System.Text.Encoding.UTF8.GetByteCount(json));


            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            RedisResponseClient.Set(json, id);
            sw.Stop();
            result.CreateTime = sw.Elapsed.ToString();

            sw.Restart();
            RedisResponseClient.Get(id);
            sw.Stop();
            result.ReadTime = sw.Elapsed.ToString();

            sw.Restart();
            RedisResponseClient.Remove(id);
            sw.Stop();
            result.DeleteTime = sw.Elapsed.ToString();
            return Json(result);
        }

        private static readonly ServiceBusTest serviceBusTest = new ServiceBusTest();
        [HttpPost]
        public async Task<JsonResult> ServiceBusJsonAsync(int maxSize)
        {
            int size = maxSize;
            List<DateTime> arrs = new List<DateTime>();

            for (int i = 0; i < size; i++)
            {
                arrs.Add(DateTime.Now);
            }
            string newguid = Guid.NewGuid().ToString();

            JsonTimeTestModel model = new JsonTimeTestModel();


            string json = HttpUtils.ToJson(arrs);

            model.StringSize = HttpUtils.CountSize(System.Text.Encoding.UTF8.GetByteCount(json));

            await serviceBusTest.ProcessRequestAsync(json, Guid.NewGuid().ToString(), model);

            model.DeleteTime = model.ReadTime;

            return Json(model);

        }



        [HttpGet]
        public ActionResult FileDownload()
        {
            var stream = System.IO.File.Open(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZoomInstaller.zip"), FileMode.Open);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Octet, "ZoomInstaller.zip");
        }



        [HttpPost]
        public ActionResult FileDown(string name)
        {
            JsonFileModel model = new JsonFileModel();
            string newguid = Guid.NewGuid().ToString();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            StorageBlobClient blobClient = new StorageBlobClient(Program.Config.StorageBlobConnectionString, Program.Config.BlobContainName);

            using (var stream = System.IO.File.Open(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZoomInstaller.zip"), FileMode.Open))
            {

                sw.Start();
                blobClient.UploadFileAsync(stream, newguid).GetAwaiter().GetResult();
                sw.Stop();
            }

            System.IO.FileInfo fileInfo = new FileInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZoomInstaller.zip"));
            model.FileSize = HttpUtils.CountSize(fileInfo.Length);

            model.UploadFileTime = sw.Elapsed.ToString();

            string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newguid + "ZoomInstaller.zip");
            using (FileStream fileStream = System.IO.File.Create(fileName))
            {
                sw.Restart();
                blobClient.GetFile(fileStream, newguid);
                sw.Stop();

                model.DownLoadFileTime = sw.Elapsed.ToString();
            }

            System.IO.File.Delete(fileName);

            sw.Restart();

            using (ServiceBusQueueClient serviceBus = new ServiceBusQueueClient(Program.Config.ServiceBusConnectionString, "queuetest"))
            {
                sw.Stop();
                sw.Restart();

                string json = HttpUtils.ToJson(model);
                serviceBus.SendMessagesAsync(json, HttpContext.TraceIdentifier).GetAwaiter().GetResult();
                sw.Stop();

                model.SendMessageQueueTime = sw.Elapsed.ToString();

            }

            return Json(model);

        }
        [HttpGet]
        public JsonResult PostTest(int id)
        {


            RequestLoadTest test = new RequestLoadTest(_httpClient);

            return Json(test.PostTest(id));
        }
        private const string requestqueue = "queuetest";
        private const string con = "Endpoint=sb://.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=";
        static Lazy<ServiceBusQueueClient> QueueBusClient = new Lazy<ServiceBusQueueClient>(() => { return new ServiceBusQueueClient(con, requestqueue); });

       // static Lazy<RedisMessage> RedisClientTest = new Lazy<RedisMessage>(() => { return new RedisMessage(Startup.Config.RedisConnectionString, "testredis"); });

        [HttpGet]
        public async Task<JsonResult> TestQueue()
        {
            ReqModel model = new ReqModel();
            model.Guid = Guid.NewGuid().ToString();
            model.Time = DateTime.Now;

            await QueueBusClient.Value.SendMessagesAsync(HttpUtils.ToJson(model));

            return Json(model.Guid);
        }

        [HttpGet]
        public JsonResult TestRedis()
        {
            ReqModel model = new ReqModel();
            model.Guid = Guid.NewGuid().ToString();
            model.Time = DateTime.Now;

           // RedisClientTest.Value.SendMessage(HttpUtils.ToJson(model));

            return Json(model.Guid);
        }
    }
}