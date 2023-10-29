using Microsoft.AspNetCore.Http;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class QueueSendBusClient : QueueSendClientBase, IQueueService
    {
        private static ServiceBusQueueClient RequestQueueClient = null;
        private Logger logger = LogManager.GetCurrentClassLogger();

        public async Task ProcessRequestAsync(HttpContext httpContext, AppSettingsQueueApi appSettings)
        {
            InitSendQueueClient(appSettings);
            System.Diagnostics.Stopwatch sws = new System.Diagnostics.Stopwatch();
            sws.Start();
            var requestData = HttpUtils.GetRequestDataModel(httpContext.Request, appSettings);

            logger.Info($"{requestData.TargetUrl} ==QueueID:{requestData.Guid}=={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");

            string reqJson = HttpUtils.ToJson(requestData);


            var taskSend = RequestQueueClient.SendMessagesAsync(reqJson, requestData.Guid);


            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var res = await GetReceiveMessageAsync(requestData.Guid, appSettings.HttpTimeOut, taskSend);

            sw.Stop();
            logger.Info($"QueueID:{requestData.Guid}==ReceiveMessage executionTime: {sw.Elapsed.ToString()}");

            sw.Restart();
            await WriteResponseAsync(res, httpContext.Response, appSettings);
            sw.Stop();
            sws.Stop();
            logger.Info($"QueueID:{requestData.Guid}==WriteResponse executionTime: {sw.Elapsed.ToString()}");

            logger.Info($"QueueID:{requestData.Guid}==end executionTime: {sws.Elapsed.ToString()}");
        }

        public static void InitSendQueueClient(AppSettingsQueueApi appSettings)
        {
            if (RequestQueueClient == null)
            {
                lock (locker)
                {
                    if (RequestQueueClient == null)
                    {
                        RequestQueueClient = new ServiceBusQueueClient(appSettings.ServiceBusConnectionString, appSettings.RequstQueueName);
                    }
                }
            }

            InitReceiveQueue(appSettings);
        }
    }
}
