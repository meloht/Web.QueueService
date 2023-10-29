using Microsoft.AspNetCore.Http;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class QueueSendRedisClient : QueueSendClientBase, IQueueService
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static RedisRequestClient RequestRedisChannel = null;


        public async Task ProcessRequestAsync(HttpContext httpContext, AppSettingsQueueApi appSettings)
        {
            InitData(appSettings);

            var requestData = HttpUtils.GetRequestDataModel(httpContext.Request, appSettings);

            System.Diagnostics.Stopwatch sws = new System.Diagnostics.Stopwatch();
            sws.Start();

            logger.Info($"{requestData.TargetUrl} ==QueueID:{requestData.Guid}=={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}");

            string reqJson = HttpUtils.ToJson(requestData);

            var taskSend = RequestRedisChannel.SendMessageAsync(reqJson, requestData.Guid, logger);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var res = await GetReceiveMessageAsync(requestData.Guid, appSettings.HttpTimeOut, taskSend);
            sw.Stop();

            logger.Info($"QueueID:{requestData.Guid}==ReceiveMessage executionTime: {sw.Elapsed.ToString()}");

            sw.Restart();

            await WriteResponseAsync(res, httpContext.Response, appSettings);

            sw.Stop();

            logger.Info($"QueueID:{requestData.Guid}==WriteResponse executionTime: {sw.Elapsed.ToString()}");

            sws.Stop();
            logger.Info($"QueueID:{requestData.Guid}==end executionTime: {sws.Elapsed.ToString()}");
        }

        public static void InitData(AppSettingsQueueApi appSettings)
        {
            if (RequestRedisChannel == null)
            {
                lock (locker)
                {
                    if (RequestRedisChannel == null)
                    {
                        RequestRedisChannel = new RedisRequestClient(appSettings.RequstQueueName);
                    }

                }
            }

            InitReceiveQueue(appSettings);
        }
    }
}
