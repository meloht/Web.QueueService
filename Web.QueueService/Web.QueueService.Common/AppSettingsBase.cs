using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class AppSettingsBase
    {
        public AppSettingsBase(IConfiguration config)
        {
            serviceBusConnectionString = config["ServiceBusConnectionString"];
            requstQueueName = config["RequstQueueName"];
            responseQueueName = config["ResponseQueueName"];


            storageBlobConnectionString = config["StorageBlobConnectionString"];
            blobContainName = config["BlobContainName"];
            //urlMapList = config.GetSection("UrlMap").Get<List<UrlMap>>();


            redisResponse = config["RedisResponse"];

            httpTimeOut = HttpUtils.ConvertInt(config["HttpTimeOut"], 200);
            poolSize = HttpUtils.ConvertInt(config["PoolSize"], 100);


        }
        private int poolSize;
        private string serviceBusConnectionString;
        private string requstQueueName;
        private string responseQueueName;
        private string storageBlobConnectionString;
        private string blobContainName;

        private int httpTimeOut;


        private string redisResponse;



        public string RedisResponse
        {
            get { return redisResponse; }
        }





        public int HttpTimeOut
        {
            get { return httpTimeOut; }
        }


        public string ServiceBusConnectionString
        {
            get { return serviceBusConnectionString; }
        }


        public string RequstQueueName
        {
            get { return requstQueueName; }
        }


        public string ResponseQueueName
        {
            get { return responseQueueName; }
        }

        public string StorageBlobConnectionString
        {
            get { return storageBlobConnectionString; }
        }



        public string BlobContainName
        {
            get
            {
                return blobContainName;
            }
        }


        public int PoolSize
        {
            get { return poolSize; }
        }


    }
    public class CosmosDbModel
    {
        public CosmosDbModel()
        { }

        public CosmosDbModel(IConfiguration config)
        {
            databaseName = config["cosmosdb:databaseName"];
            collectionName = config["cosmosdb:collectionName"];
            endpointUrl = config["cosmosdb:endpointUrl"];
            authorizationKey = config["cosmosdb:authorizationKey"];
        }

        private string databaseName;

        public string DatabaseName
        {
            get { return databaseName; }
        }

        private string collectionName;

        public string CollectionName
        {
            get { return collectionName; }
        }

        private string endpointUrl;

        public string EndpointUrl
        {
            get { return endpointUrl; }
        }

        private string authorizationKey;

        public string AuthorizationKey
        {
            get { return authorizationKey; }
        }



    }
}
