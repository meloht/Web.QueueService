using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common.ResponseHandlers
{
    public class ResponseQueueBusHandle : ResponseHandlerQueueBus, IResponseJsonHandler
    {
        private static ResponseQueueBusHandle responseQueueBusHandle = null;
        public ResponseQueueBusHandle(AppSettingsWorkerJob appSettings)
            : base(appSettings)
        {

        }

        public async Task SendResponseMessageAsync(HttpResponseDataModel httpResponseData, string json)
        {

            httpResponseData.ResultType = ResultType.Json;
            httpResponseData.Json = json;

            string resJson = HttpUtils.ToJson(httpResponseData);
            await ResponseQueueClient.SendMessagesAsync(resJson, httpResponseData.Guid);
        }

        public static ResponseQueueBusHandle GetInstance(AppSettingsWorkerJob appSettings)
        {
            if (responseQueueBusHandle == null)
            {
                lock (objLock)
                {
                    if (responseQueueBusHandle == null)
                    {
                        responseQueueBusHandle = new ResponseQueueBusHandle(appSettings);
                    }
                }
            }

            return responseQueueBusHandle;
        }
    }
}
