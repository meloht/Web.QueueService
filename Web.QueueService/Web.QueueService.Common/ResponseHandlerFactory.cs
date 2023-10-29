using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class ResponseHandlerFactory
    {

        public static IResponseJsonHandler GetResponseHandler(long size, AppSettingsWorkerJob appSettings)
        {
            IResponseJsonHandler handler = GetRedisSubscribeRedis(size, appSettings);
            if (handler != null)
            {
                return handler;
            }

            handler = GetRedisSubscribeBlob(size, appSettings);

            if (handler != null)
            {
                return handler;
            }

            handler = GetQueueBusSubscribeQueue(size, appSettings);
            if (handler != null)
            {
                return handler;
            }

            handler = GetQueueBusSubscribeRedis(size, appSettings);
            if (handler != null)
            {
                return handler;
            }

            handler = GetQueueBusSubscribeBlob(size, appSettings);
            if (handler != null)
            {
                return handler;
            }

            return ResponseHandlerRedis.GetRedisHandler(appSettings);
        }

        public static IResponseFileHandler GetAttachmentHandler(AppSettingsWorkerJob appSettings)
        {
            return ResponseHandlerRedis.GetRedisFileHandler(appSettings);
        }

        private static IResponseJsonHandler GetRedisSubscribeRedis(long size, AppSettingsWorkerJob appSettings)
        {
            long max = appSettings.ResponseMode.RedisSubscribe.RedisResponse.MaxSize;
            long min = appSettings.ResponseMode.RedisSubscribe.RedisResponse.MinSize;
            if (max > size && size >= min)
            {
                return ResponseHandlerRedis.GetRedisHandler(appSettings);
            }
            return null;
        }

        private static IResponseJsonHandler GetRedisSubscribeBlob(long size, AppSettingsWorkerJob appSettings)
        {
            long max = appSettings.ResponseMode.RedisSubscribe.BlobResponse.MaxSize;
            long min = appSettings.ResponseMode.RedisSubscribe.BlobResponse.MinSize;
            if (max > size && size >= min)
            {
                return ResponseHandlerRedis.GetBlobHandler(appSettings);
            }
            return null;
        }

        private static IResponseJsonHandler GetQueueBusSubscribeBlob(long size, AppSettingsWorkerJob appSettings)
        {
            long max = appSettings.ResponseMode.ServiceBusSubscribe.BlobResponse.MaxSize;
            long min = appSettings.ResponseMode.ServiceBusSubscribe.BlobResponse.MinSize;
            if (max > size && size >= min)
            {
                return ResponseHandlerQueueBus.GetBlobHandler(appSettings);
            }
            return null;
        }
        private static IResponseJsonHandler GetQueueBusSubscribeQueue(long size, AppSettingsWorkerJob appSettings)
        {
            long max = appSettings.ResponseMode.ServiceBusSubscribe.BusQueueResponse.MaxSize;
            long min = appSettings.ResponseMode.ServiceBusSubscribe.BusQueueResponse.MinSize;

            long limitSize = 200 * 1024;
            if (size >= limitSize)
                return null;

            if (max > size && size >= min)
            {
                return ResponseHandlerQueueBus.GetQueueBusHandler(appSettings);
            }
            return null;
        }

        private static IResponseJsonHandler GetQueueBusSubscribeRedis(long size, AppSettingsWorkerJob appSettings)
        {
            long max = appSettings.ResponseMode.ServiceBusSubscribe.RedisResponse.MaxSize;
            long min = appSettings.ResponseMode.ServiceBusSubscribe.RedisResponse.MinSize;
            if (max > size && size >= min)
            {
                return ResponseHandlerQueueBus.GetRedisHandler(appSettings);
            }
            return null;
        }
    }
}
