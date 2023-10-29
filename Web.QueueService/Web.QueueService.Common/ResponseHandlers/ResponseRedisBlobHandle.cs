using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common.ResponseHandlers
{
    public class ResponseRedisBlobHandle : ResponseHandlerRedis, IResponseJsonHandler
    {
        private static ResponseRedisBlobHandle responseRedisBlobHandle = null;

        public ResponseRedisBlobHandle(AppSettingsWorkerJob appSettings)
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
            await SendResponseRedisAsync(resJson, httpResponseData.Guid);
        }

        public static ResponseRedisBlobHandle GetInstance(AppSettingsWorkerJob appSettings)
        {
            if (responseRedisBlobHandle == null)
            {
                lock (objLock)
                {
                    if (responseRedisBlobHandle == null)
                    {
                        responseRedisBlobHandle = new ResponseRedisBlobHandle(appSettings);
                    }
                }
            }

            return responseRedisBlobHandle;
        }
    }
}
