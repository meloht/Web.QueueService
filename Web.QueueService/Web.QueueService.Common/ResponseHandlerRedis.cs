using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.QueueService.Common.ResponseHandlers;

namespace Web.QueueService.Common
{
    public class ResponseHandlerRedis : ResponseHandlerBase
    {
        private static RedisResponseClient ResponseQueue = null;

        public ResponseHandlerRedis(AppSettingsWorkerJob appSettings)
            : base(appSettings)
        {

            InitRedis(appSettings);
        }
        private void InitRedis(AppSettingsWorkerJob appSettings)
        {
            if (ResponseQueue == null)
            {
                lock (objLock)
                {
                    if (ResponseQueue == null)
                    {
                        ResponseQueue = new RedisResponseClient(appSettings.ResponseQueueName);
                    }
                }
            }
        }

        public static IResponseJsonHandler GetRedisHandler(AppSettingsWorkerJob appSettings)
        {
            ResponseRedisHandle response = ResponseRedisHandle.GetInstance(appSettings);
            return response;
        }

        public static IResponseFileHandler GetRedisFileHandler(AppSettingsWorkerJob appSettings)
        {
            ResponseAttachmentHandle response = ResponseAttachmentHandle.GetInstance(appSettings);
            return response;
        }

        public static IResponseJsonHandler GetBlobHandler(AppSettingsWorkerJob appSettings)
        {
            ResponseRedisBlobHandle response = ResponseRedisBlobHandle.GetInstance(appSettings);
            return response;
        }


        protected async Task SendResponseRedisAsync(string resJson, string guid)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            await ResponseQueue.SendMessageAsync(resJson);

            sw.Stop();

            logger.Info($"QueueID:{guid} redis SendMessage executionTime:{sw.Elapsed.ToString()}");
        }
    }
}
