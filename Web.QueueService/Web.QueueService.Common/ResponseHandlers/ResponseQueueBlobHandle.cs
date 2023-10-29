using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common.ResponseHandlers
{
    public class ResponseQueueBlobHandle : ResponseHandlerQueueBus, IResponseJsonHandler
    {
        private static ResponseQueueBlobHandle responseQueueBlobHandle = null;
        public ResponseQueueBlobHandle(AppSettingsWorkerJob appSettings)
         : base(appSettings)
        {

        }
        public async Task SendResponseMessageAsync(HttpResponseDataModel httpResponseData, string json)
        {

            InitBlob(AppSettings);

            httpResponseData.ResultType = ResultType.Blob;
            httpResponseData.Json = null;

            await blobClient.UploadStringAsync(json, httpResponseData.Guid);

            string resJson = HttpUtils.ToJson(httpResponseData);
            await ResponseQueueClient.SendMessagesAsync(resJson, httpResponseData.Guid);
        }

        public static ResponseQueueBlobHandle GetInstance(AppSettingsWorkerJob appSettings)
        {
            if (responseQueueBlobHandle == null)
            {
                lock (objLock)
                {
                    if (responseQueueBlobHandle == null)
                    {
                        responseQueueBlobHandle = new ResponseQueueBlobHandle(appSettings);
                    }
                }
            }

            return responseQueueBlobHandle;
        }
    }
}
