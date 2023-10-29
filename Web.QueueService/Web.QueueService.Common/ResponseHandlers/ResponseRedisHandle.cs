using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common.ResponseHandlers
{
    public class ResponseRedisHandle : ResponseHandlerRedis, IResponseJsonHandler
    {
        private static ResponseRedisHandle responseRedisHandle = null;
        public ResponseRedisHandle(AppSettingsWorkerJob appSettings)
            : base(appSettings)
        {

        }
        public async Task SendResponseMessageAsync(HttpResponseDataModel httpResponseData, string json)
        {

            httpResponseData.ResultType = ResultType.Json;
            httpResponseData.Json = json;

            string resJson = HttpUtils.ToJson(httpResponseData);
            await SendResponseRedisAsync(resJson, httpResponseData.Guid);
        }

        public static ResponseRedisHandle GetInstance(AppSettingsWorkerJob appSettings)
        {
            if (responseRedisHandle == null)
            {
                lock (objLock)
                {
                    if (responseRedisHandle == null)
                    {
                        responseRedisHandle = new ResponseRedisHandle(appSettings);
                    }
                }
            }
            return responseRedisHandle;
        }
    }
}
