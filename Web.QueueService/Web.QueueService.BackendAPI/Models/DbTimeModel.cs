namespace Web.QueueService.BackendAPI.Models
{
    public class DbTimeModel
    {
        public string StringSize { get; set; }

        public string CosmosDbCreateTime { get; set; }

        public string CosmosDbReadTime { get; set; }

        public string CosmosDbDeleteTime { get; set; }


        public string RedisCreateTime { get; set; }
        public string RedisReadTime { get; set; }
        public string RedisDeleteTime { get; set; }
    }

    public class DbRedisTimeModel
    {
        public string StringSize { get; set; }

        public string RedisCreateTime { get; set; }
        public string RedisReadTime { get; set; }
        public string RedisDeleteTime { get; set; }
    }

    public class JsonTimeTestModel
    {
        public string StringSize { get; set; }

        public string CreateTime { get; set; }
        public string ReadTime { get; set; }
        public string DeleteTime { get; set; }
    }

    public class QueueJson
    {
        public string Guid { get; set; }

        public string Json { get; set; }
    }

    public class ReqModel
    {
        public DateTime Time { get; set; }
        public string Guid { get; set; }
    }
}
