using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common.ResponseHandlers
{
    public class ResponseAttachmentHandle : ResponseHandlerRedis, IResponseFileHandler
    {
        private static ResponseAttachmentHandle responseAttachmentHandle = null;
        public ResponseAttachmentHandle(AppSettingsWorkerJob appSettings)
          : base(appSettings)
        {

        }
        public async Task SendResponseMessageAsync(HttpResponseDataModel httpResponseData, Stream stream)
        {
            InitBlob(AppSettings);

            httpResponseData.ResultType = ResultType.File;
            httpResponseData.Json = null;

            await blobClient.UploadFileAsync(stream, httpResponseData.Guid);

            string resJson = HttpUtils.ToJson(httpResponseData);
            await SendResponseRedisAsync(resJson, httpResponseData.Guid);
        }

        public static ResponseAttachmentHandle GetInstance(AppSettingsWorkerJob appSettings)
        {
            if (responseAttachmentHandle == null)
            {
                lock (objLock)
                {
                    if (responseAttachmentHandle == null)
                    {
                        responseAttachmentHandle = new ResponseAttachmentHandle(appSettings);
                    }
                }
            }

            return responseAttachmentHandle;
        }
    }
}
