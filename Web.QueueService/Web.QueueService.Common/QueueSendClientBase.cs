using CSRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Primitives;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class QueueSendClientBase
    {
        protected static readonly object locker = new object();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static StorageBlobClient storageBlobClient;

        private static ServiceBusQueueClient ResponseQueueClient = null;
        private static RedisResponseClient ResponseRedisChannel = null;

        //private static ServiceBusQueueClient TestQueueClient = null;

        protected static ConcurrentDictionary<string, TaskCompletionSource<HttpResponseDataModel>> RequstQueue = new ConcurrentDictionary<string, TaskCompletionSource<HttpResponseDataModel>>();

        protected async Task WriteResponseAsync(HttpResponseDataModel model, HttpResponse httpResponse, AppSettingsQueueApi appSettings)
        {
            httpResponse.ContentType = model.ContentType;
            httpResponse.Headers.Add("QueueId", model.Guid);
            if (model.Attachment != null && !string.IsNullOrEmpty(model.Attachment.BlobGuid))
            {
                GetAttchmentFile(httpResponse, appSettings, model);
            }
            else
            {

                string json = "";
                if (model.ResultType == ResultType.Json)
                {
                    json = model.Json;
                }
                else if (model.ResultType == ResultType.Blob)
                {
                    InitStorageBlobClient(appSettings);

                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    json = await storageBlobClient.DownloadStringAsync(model.Guid);
                    await storageBlobClient.DeleteFileAsync(model.Guid);

                    sw.Stop();

                    logger.Info($"QueueID:{model.Guid} read json from blob  and delete json executionTime:{sw.Elapsed.ToString()}");

                }
                else if (model.ResultType == ResultType.Redis)
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    json = RedisResponseClient.Get(model.Guid);
                    RedisResponseClient.Remove(model.Guid);

                    sw.Stop();

                    logger.Info($"QueueID:{model.Guid} read json from redis  and delete json executionTime:{sw.Elapsed.ToString()}");
                }

                await httpResponse.WriteAsync(json, Encoding.UTF8);

            }

        }

        protected void GetAttchmentFile(HttpResponse httpResponse, AppSettingsQueueApi appSettings, HttpResponseDataModel model)
        {
            InitStorageBlobClient(appSettings);
            httpResponse.Headers.Add("Content-Disposition", new StringValues(model.Attachment.ContentDisposition));
            httpResponse.ContentType = model.ContentType;

            using (Stream outStream = httpResponse.Body)
            {
                storageBlobClient.GetFile(outStream, model.Attachment.BlobGuid);
                outStream.Close();
            }

        }
        private void InitStorageBlobClient(AppSettingsQueueApi appSettings)
        {
            if (storageBlobClient == null)
            {
                lock (locker)
                {
                    if (storageBlobClient == null)
                    {
                        storageBlobClient = new StorageBlobClient(appSettings.StorageBlobConnectionString, appSettings.BlobContainName);

                    }
                }
            }

        }
        protected static void InitReceiveQueue(AppSettingsQueueApi appSettings)
        {
            if (ResponseQueueClient == null)
            {
                lock (locker)
                {
                    if (ResponseQueueClient == null)
                    {
                        ResponseQueueClient = new ServiceBusQueueClient(appSettings.ServiceBusConnectionString, appSettings.ResponseQueueName,
                            appSettings.PoolSize, ProcessBusMessagesAsync);
                    }
                }
            }

            if (ResponseRedisChannel == null)
            {
                lock (locker)
                {
                    if (ResponseRedisChannel == null)
                    {
                        ResponseRedisChannel = new RedisResponseClient(appSettings.ResponseQueueName, ProcessRedisMessageAsync);
                    }
                }
            }


        }


        private static void ProcessRedisMessageAsync(CSRedisClient.SubscribeMessageEventArgs message)
        {
            HttpResponseDataModel model = HttpUtils.ToObject<HttpResponseDataModel>(message.Body);

            logger.Info($"QueueID:{model.Guid} get message from Redis {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")} ThreadId:{Thread.CurrentThread.ManagedThreadId.ToString()}");

            FinishRequest(model);

        }

        private static async Task ProcessBusMessagesAsync(Message message, CancellationToken token)
        {
            // await ResponseQueueClient.CompleteAsync(message.SystemProperties.LockToken);
            string msgbody = Encoding.UTF8.GetString(message.Body);
            HttpResponseDataModel model = HttpUtils.ToObject<HttpResponseDataModel>(msgbody);

            logger.Info($"QueueID:{model.Guid} get message from bus {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")} ThreadId:{Thread.CurrentThread.ManagedThreadId.ToString()}");

            FinishRequest(model);

            await Task.CompletedTask;

        }
        private static void FinishRequest(HttpResponseDataModel model)
        {
            if (!RequstQueue.ContainsKey(model.Guid))
            {
                logger.Info($"QueueID:{model.Guid} RequstQueue don't ContainsKey  {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");
                return;
            }

            var tempTCS = RequstQueue[model.Guid];

            if (tempTCS.TrySetResult(model))
            {
                logger.Info($"QueueID:{model.Guid} TrySetResult OK {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");
            }
            else
            {
                logger.Info($"QueueID:{model.Guid} TrySetResult Failed {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");
            }

            RequstQueue.TryRemove(model.Guid, out tempTCS);
        }

        protected async Task<HttpResponseDataModel> GetReceiveMessageAsync(string guid, int timeOut, Task taskSend)
        {
            var tcs = new TaskCompletionSource<HttpResponseDataModel>();
            var ct = new CancellationTokenSource(timeOut * 1000);

            RequstQueue.TryAdd(guid, tcs);
            ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

            await taskSend;
            return await tcs.Task;
        }
    }
}
