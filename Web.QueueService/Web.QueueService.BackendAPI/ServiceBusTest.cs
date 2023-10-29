
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Web.QueueService.BackendAPI.Models;
using Web.QueueService.Common;

namespace Web.QueueService.BackendAPI
{
    public class ServiceBusTest
    {
        private const string requestqueue = "queuetest";
        private const string con = "Endpoint=sb://.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=";



        protected static ConcurrentDictionary<string, TaskCompletionSource<QueueJson>> RequstQueue = new ConcurrentDictionary<string, TaskCompletionSource<QueueJson>>();
        private ServiceBusQueueClient QueueBusClient = null;

        protected static readonly object locker = new object();

        public async Task ProcessRequestAsync(string reqJson, string guid, JsonTimeTestModel model)
        {
            InitData();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            QueueJson json = new QueueJson();
            json.Json = reqJson;
            json.Guid = guid;

            sw.Start();
            await QueueBusClient.SendMessagesAsync(HttpUtils.ToJson(json), guid);
            sw.Stop();

            model.CreateTime = sw.Elapsed.ToString();
            sw.Restart();
            var res = await GetReceiveMessageAsync(guid, 200);
            sw.Stop();

            model.ReadTime = sw.Elapsed.ToString();

        }

        protected async Task<QueueJson> GetReceiveMessageAsync(string guid, int timeOut)
        {
            var tcs = new TaskCompletionSource<QueueJson>();
            var ct = new CancellationTokenSource(timeOut * 1000);
            RequstQueue[guid] = tcs;
            ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

            return await tcs.Task;
        }

        private async Task ProcessMessagesHandleAsync(Message message, CancellationToken token)
        {
           // await QueueBusClient.CompleteAsync(message.SystemProperties.LockToken);
            string msgbody = Encoding.UTF8.GetString(message.Body);
            QueueJson model = HttpUtils.ToObject<QueueJson>(msgbody);

            // logger.Info($"get message {model.Guid} from bus {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff")} ThreadId:{ Thread.CurrentThread.ManagedThreadId}");

            if (!RequstQueue.ContainsKey(model.Guid))
                return;

            var tempTCS = RequstQueue[model.Guid];

            tempTCS.TrySetResult(model);

            RequstQueue.TryRemove(model.Guid, out tempTCS);
            await Task.CompletedTask;
        }

        private void InitData()
        {
            if (QueueBusClient == null)
            {
                lock (locker)
                {
                    if (QueueBusClient == null)
                    {
                        QueueBusClient = new ServiceBusQueueClient(con, requestqueue, 100, ProcessMessagesHandleAsync);



                    }
                }
            }
        }
    }
}
