using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.QueueService.Common.ResponseHandlers;

namespace Web.QueueService.Common
{
    public class ResponseHandlerQueueBus : ResponseHandlerBase
    {
        protected static ServiceBusQueueClient ResponseQueueClient;


        public ResponseHandlerQueueBus(AppSettingsWorkerJob appSettings)
            : base(appSettings)
        {
            InitQueue(appSettings);
        }
        private void InitQueue(AppSettingsWorkerJob appSettings)
        {
            if (ResponseQueueClient == null)
            {
                lock (objLock)
                {
                    if (ResponseQueueClient == null)
                    {
                        ResponseQueueClient = new ServiceBusQueueClient(appSettings.ServiceBusConnectionString, appSettings.ResponseQueueName);
                    }
                }
            }
        }


        public static IResponseJsonHandler GetRedisHandler(AppSettingsWorkerJob appSettings)
        {
            ResponseQueueRedisHandle response = ResponseQueueRedisHandle.GetInstance(appSettings);
            return response;
        }

        public static IResponseJsonHandler GetBlobHandler(AppSettingsWorkerJob appSettings)
        {
            ResponseQueueBlobHandle response = ResponseQueueBlobHandle.GetInstance(appSettings);
            return response;
        }

        public static IResponseJsonHandler GetQueueBusHandler(AppSettingsWorkerJob appSettings)
        {
            ResponseQueueBusHandle response = ResponseQueueBusHandle.GetInstance(appSettings);
            return response;
        }


    }
}
