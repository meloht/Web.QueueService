

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Web.QueueService.API.Middleware;
using Web.QueueService.Common;

namespace BPM.QueueService.Test
{
    [TestClass]
    public class UnitTestCommon
    {
        public HttpClient Client;
        [TestInitialize]
        public void Init()
        {
            var server = new TestServer(WebHost.CreateDefaultBuilder());
            
            Client = server.CreateClient();
            RedisResponseClient.InitializeConnectionString(RequestMiddlewareExtensions.Config.RedisResponse, RequestMiddlewareExtensions.Config.HttpTimeOut);
        }
        [TestMethod]
        public void TestMethod1()
        {
            HttpRequestModel model = new HttpRequestModel();

            model.Body = "body";

            model.ContentType = "ContentType";

            model.IsHttps = false;
            model.Method = "post";
            model.FormValues = new Dictionary<string, string[]>();

            model.FormValues.Add("123", new[] { "ddd" });
            model.FormValues.Add("124", new[] { "aaa", "bbbb" });

            string json = HttpUtils.ToJson(model);

            HttpRequestModel obj = HttpUtils.ToObject<HttpRequestModel>(json);

            Assert.IsNotNull(obj);
        }

        //[TestMethod]
        //public void TestReadDoc()
        //{
        //    DocumentJsonModel model = new DocumentJsonModel();
        //    model.id = Guid.NewGuid().ToString();
        //    model.Json = "12321312";
        //    using (CosmosDBClient client = new CosmosDBClient(Startup.Config.CosmosDb))
        //    {
        //        var doc = client.CreateItemAsync<DocumentJsonModel>(model).GetAwaiter().GetResult();

        //        var res = client.GetItem<DocumentJsonModel>(model.id);

        //        Assert.IsNotNull(res);
        //    }
        //}

        [TestMethod]
        public void TestSplitString()
        {
            string json = "1234567890abcdefghijklmn";

            int num = 5;
            int n = json.Length % num;

            int len = json.Length / num;

            var dd = Math.Ceiling((double)json.Length / (double)num);
            int begin = 0;
            int end = len;
            string newjson = "";
            List<string> listJson = new List<string>();
            for (int i = 0; i < num; i++)
            {
                newjson = "";
                if (i == num - 1)
                {
                    newjson = json.Substring(begin);
                }
                else
                {
                    newjson = json.Substring(begin, len);
                    begin += len;
                }
                listJson.Add(newjson);
            }
            Assert.AreEqual(listJson.Count, num);
        }

        [TestMethod]
        public void TestRedis()
        {
            string str = "123";
            string key = Guid.NewGuid().ToString();
            bool bl = RedisResponseClient.Set(str, key);
            Assert.IsTrue(bl);

            string s = RedisResponseClient.Get(key);

            Assert.AreEqual(s, str);
        }

    }
}
