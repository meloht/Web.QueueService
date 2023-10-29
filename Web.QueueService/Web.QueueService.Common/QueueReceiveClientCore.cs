using Microsoft.Azure.ServiceBus;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class QueueReceiveClientCore
    {
        protected AppSettingsWorkerJob AppSettings;
        protected Queue<HttpRequestModel> MessageQueue = new Queue<HttpRequestModel>();
        protected List<string> MessagesWorking = new List<string>();
        protected static readonly object objLock = new object();
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        private ServiceBusQueueClient RequestQueueClient;


        public QueueReceiveClientCore(AppSettingsWorkerJob appSettings)
        {
            AppSettings = appSettings;
            RequestQueueClient = new ServiceBusQueueClient(appSettings.ServiceBusConnectionString, appSettings.RequstQueueName, appSettings.PoolSize, ProcessBusMessagesAsync);
        }

        private async Task ProcessBusMessagesAsync(Message message, CancellationToken token)
        {
            string msgbody = Encoding.UTF8.GetString(message.Body);
            HttpRequestModel data = HttpUtils.ToObject<HttpRequestModel>(msgbody);

            logger.Info($"QueueID:{data.Guid} get request message from bus queue {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")} ThreadId:{Thread.CurrentThread.ManagedThreadId.ToString()}");

            lock (objLock)
            {
                MessageQueue.Enqueue(data);
            }

            await ProcessMessageTaskAsync();

        }



        protected async Task ProcessResponseAsync(HttpRequestModel data, HttpResponseMessage response)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            HttpResponseDataModel httpResponseData = new HttpResponseDataModel();
            httpResponseData.Guid = data.Guid;

            var contentTypes = response.Content.Headers.GetValues("Content-Type").ToList();
            string contentType = string.Join(";", contentTypes);

            httpResponseData.ContentType = contentType;
            long? resultSize = response.Content.Headers.ContentLength;
            string size = HttpUtils.CountSize(resultSize.Value);

            IResponseJsonHandler handler = ResponseHandlerFactory.GetResponseHandler(resultSize.Value, AppSettings);

            if (contentType.Contains("application/octet-stream"))
            {
                httpResponseData.Attachment = new Attachment();
                var stream = await response.Content.ReadAsStreamAsync();

                httpResponseData.Attachment.BlobGuid = data.Guid;
                httpResponseData.Attachment.ContentDisposition = response.Content.Headers.GetValues("Content-Disposition").ToArray();


                IResponseFileHandler fileHandler = ResponseHandlerFactory.GetAttachmentHandler(AppSettings);

                logger.Info($"QueueID:{data.Guid} {fileHandler.GetType().Name} file size: {size}");

                await fileHandler.SendResponseMessageAsync(httpResponseData, stream);
            }
            else if (contentType.Contains("application/json"))
            {
                sw.Start();
                string json = await response.Content.ReadAsStringAsync();

                sw.Stop();
                logger.Info($"QueueID:{data.Guid} Read json from Request executionTime :{sw.Elapsed.ToString()}");

                logger.Info($"QueueID:{data.Guid} {handler.GetType().Name} json size: {size}");


                await handler.SendResponseMessageAsync(httpResponseData, json);


            }
            else
            {

                sw.Start();
                string json = await response.Content.ReadAsStringAsync();
                sw.Stop();
                logger.Info($"QueueID:{data.Guid} Read json from Request executionTime :{sw.Elapsed.ToString()}");

                logger.Info($"QueueID:{data.Guid} {handler.GetType().Name} {contentType} size: {size}");

                await handler.SendResponseMessageAsync(httpResponseData, json);

            }

            logger.Info($"QueueID:{data.Guid} Send Response Successfully *****");
        }

        protected List<HttpRequestModel> GetNumMessageByPoolSize()
        {
            lock (objLock)
            {
                List<HttpRequestModel> list = new List<HttpRequestModel>();
                int num = AppSettings.PoolSize - MessagesWorking.Count;

                for (int i = 0; i < num; i++)
                {
                    if (MessageQueue.Count > 0)
                    {
                        list.Add(MessageQueue.Dequeue());
                    }
                    else
                    {
                        break;
                    }
                }
                return list;
            }
        }
        protected async Task ProcessMessageListAsync()
        {
            var list = GetNumMessageByPoolSize();

            List<Task> listTask = new List<Task>(list.Count);
            foreach (var item in list)
            {
                lock (objLock)
                {
                    MessagesWorking.Add(item.Guid);
                }
                var task = ProcessMessageItemAsync(item);
                listTask.Add(task);

            }

            foreach (var item in listTask)
            {
                await item;
            }

        }
        protected async Task ProcessMessageItemAsync(HttpRequestModel item)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var response = await HttpUtils.SendRequestAsync(item);
            sw.Stop();
            if (response != null)
            {
                logger.Info($"QueueID:{item.Guid} url:{item.TargetUrl} {response.IsSuccessStatusCode} SendRequest executionTime :{sw.Elapsed.ToString()}");

                if (response.IsSuccessStatusCode)
                {
                    await ProcessResponseAsync(item, response);
                }
            }
            else
            {
                logger.Info($"QueueID:{item.Guid} url:{item.TargetUrl}  failed ");
            }

            lock (objLock)
            {
                MessagesWorking.Remove(item.Guid);
            }
        }

        public async Task ProcessMessageTaskAsync()
        {
            lock (objLock)
            {
                if (MessagesWorking.Count >= AppSettings.PoolSize)
                {
                    logger.Info("PoolSize Reaches the maximum value =======================");
                    return;
                }
            }

            await ProcessMessageListAsync();
        }
    }
}
