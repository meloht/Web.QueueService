using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class AppSettingsWorkerJob : AppSettingsBase
    {
        public AppSettingsWorkerJob(IConfiguration config) : base(config)
        {
            responseMode = new ResponseMode(config);
        }
        private ResponseMode responseMode;
        public ResponseMode ResponseMode
        {
            get { return responseMode; }
        }
    }

    public class ResponseMode
    {
        public ResponseMode(IConfiguration config)
        {
            ServiceBusSubscribe = new ServiceBusSubscribe();
            RedisSubscribe = new RedisSubscribe();

            ServiceBusSubscribe.BlobResponse = new SizeConfig();
            ServiceBusSubscribe.BusQueueResponse = new SizeConfig();
            ServiceBusSubscribe.RedisResponse = new SizeConfig();


            RedisSubscribe.BlobResponse = new SizeConfig();
            RedisSubscribe.RedisResponse = new SizeConfig();

            RedisSubscribe.BlobResponse.MaxSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:RedisSubscribe:BlobResponse:MaxSize"]);
            RedisSubscribe.BlobResponse.MinSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:RedisSubscribe:BlobResponse:MinSize"]);

            RedisSubscribe.RedisResponse.MaxSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:RedisSubscribe:RedisResponse:MaxSize"]);
            RedisSubscribe.RedisResponse.MinSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:RedisSubscribe:RedisResponse:MinSize"]);

            ServiceBusSubscribe.BlobResponse.MaxSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:ServiceBusSubscribe:BlobResponse:MaxSize"]);
            ServiceBusSubscribe.BlobResponse.MinSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:ServiceBusSubscribe:BlobResponse:MinSize"]);

            ServiceBusSubscribe.BusQueueResponse.MaxSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:ServiceBusSubscribe:BusQueueResponse:MaxSize"]);
            ServiceBusSubscribe.BusQueueResponse.MinSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:ServiceBusSubscribe:BusQueueResponse:MinSize"]);

            ServiceBusSubscribe.RedisResponse.MaxSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:ServiceBusSubscribe:RedisResponse:MaxSize"]);
            ServiceBusSubscribe.RedisResponse.MinSize = 1024 * HttpUtils.ConvertLongInt(config["ResponseMode:ServiceBusSubscribe:RedisResponse:MinSize"]);

        }
        public ServiceBusSubscribe ServiceBusSubscribe { get; set; }

        public RedisSubscribe RedisSubscribe { get; set; }


    }

    public class ServiceBusSubscribe
    {
        public SizeConfig BusQueueResponse { get; set; }
        public SizeConfig BlobResponse { get; set; }
        public SizeConfig RedisResponse { get; set; }
    }

    public class RedisSubscribe
    {
        public SizeConfig RedisResponse { get; set; }
        public SizeConfig BlobResponse { get; set; }
    }

    public class SizeConfig
    {
        public long MaxSize { get; set; }
        public long MinSize { get; set; }
    }
}
