using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common.ResponseHandlers
{
    public class ResponseQueueRedisHandle : ResponseHandlerQueueBus, IResponseJsonHandler
    {
        private static ResponseQueueRedisHandle responseQueueRedisHandle = null;
        public ResponseQueueRedisHandle(AppSettingsWorkerJob appSettings)
           : base(appSettings)
        {

        }
        public async Task SendResponseMessageAsync(HttpResponseDataModel httpResponseData, string json)
        {

            httpResponseData.ResultType = ResultType.Redis;
            httpResponseData.Json = null;
            RedisResponseClient.Set(json, httpResponseData.Guid);

            string resJson = HttpUtils.ToJson(httpResponseData);
            await ResponseQueueClient.SendMessagesAsync(resJson, httpResponseData.Guid);
        }

        public static ResponseQueueRedisHandle GetInstance(AppSettingsWorkerJob appSettings)
        {
            if (responseQueueRedisHandle == null)
            {
                lock (objLock)
                {
                    if (responseQueueRedisHandle == null)
                    {
                        responseQueueRedisHandle = new ResponseQueueRedisHandle(appSettings);
                    }
                }
            }

            return responseQueueRedisHandle;
        }
    }
}
