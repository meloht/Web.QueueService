using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class HttpUtils
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static HttpClient httpClient;
        private static IHttpClientFactory httpClientFactory;

        public static void InitHttpClient()
        {
            if (httpClientFactory == null)
            {
                var serviceProvider = new ServiceCollection().AddHttpClient("HttpClientWithSSLUntrusted").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = delegate { return true; }
                }).Services.BuildServiceProvider(); ;

                httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();


            }

            httpClient = httpClientFactory.CreateClient("HttpClientWithSSLUntrusted");


        }

        public static HttpRequestModel GetRequestDataModel(HttpRequest request, AppSettingsQueueApi appSettings)
        {

            HttpRequestModel model = new HttpRequestModel();
            model.Guid = Guid.NewGuid().ToString();

            model.ContentType = request.ContentType;

            StringValues reqs;
            if (request.Headers.TryGetValue("requestId", out reqs))
            {
                logger.Info($"QueueID:{model.Guid}  RequestId:{string.Join(",", reqs)}");
            }

            model.IsHttps = request.IsHttps;
            model.Method = request.Method;

            model.TargetUrl = GetTargetUrl(request, appSettings);

            if (request.HasFormContentType && request.Form != null)
            {
                model.FormValues = new Dictionary<string, string[]>();
                foreach (var item in request.Form)
                {
                    model.FormValues.Add(item.Key, item.Value.ToArray());
                }
            }
            if (request.Query != null && request.Query.Count > 0)
            {
                model.QueryStringValues = new Dictionary<string, string[]>();
                foreach (var item in request.Query)
                {
                    model.QueryStringValues.Add(item.Key, item.Value.ToArray());
                }
            }

            if (model.Method.ToLower() != "get")
            {
                model.Body = GetRequestBody(request);
            }


            return model;
        }
        public static string GetTargetUrl(HttpRequest request, AppSettingsQueueApi appSettings)
        {

            var urlTarget = appSettings.TargetUrl;

            string http = "http";
            string targetUrl = urlTarget;
            if (request.IsHttps)
            {
                http = "https";
                targetUrl = urlTarget;
            }

            string url = $"{http}://{targetUrl}{request.Path}";
            if (request.Query != null && request.Query.Count > 0)
            {
                List<string> paras = new List<string>();
                foreach (var item in request.Query)
                {
                    if (item.Value.Count > 0)
                    {
                        paras.Add($"{item.Key}={item.Value[0]}");
                    }

                }
                url = $"{url}?{string.Join("&", paras)}";
            }

            return url;
        }




        private static string GetRequestBody(HttpRequest request)
        {
            if (request.Body == null)
                return string.Empty;

            string body = "";
            try
            {

                using (var reader = new StreamReader(request.Body))
                {
                    body = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }


            return body;
        }

        public static async Task<HttpResponseMessage> SendRequestAsync(HttpRequestModel data)
        {
            try
            {
                if (data.IsHttps)
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                }
                if (httpClient == null)
                {
                    InitHttpClient();
                }
                HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod(data.Method), data.TargetUrl);
                requestMessage.Content = GetHttpContent(data);

                var response = await httpClient.SendAsync(requestMessage);
                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                logger.Info($"QueueID:{data.Guid} url:{data.TargetUrl} SendRequest error {ex.Message}");

                InitHttpClient();
            }
            return null;
        }

        public static HttpContent GetHttpContent(HttpRequestModel data)
        {
            if (data.ContentType == null)
                return null;
            string contType = data.ContentType.ToLower().Trim();
            if (contType.Contains("application/json"))
            {
                var content = new StringContent(data.Body, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
                return content;
            }
            if (contType.Contains("application/x-www-form-urlencoded"))
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (var item in data.FormValues)
                {
                    if (item.Value.Length > 0 && !dict.ContainsKey(item.Key))
                    {
                        dict.Add(item.Key, item.Value[0]);
                    }

                }
                var content = new FormUrlEncodedContent(dict);
                return content;
            }
            return null;
        }


        public static T ToObject<T>(string json)
        {
            try
            {
                JsonSerializerSettings setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                return JsonConvert.DeserializeObject<T>(json, setting);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }

            return default(T);
        }

        public static string ToJson(object obj)
        {
            try
            {
                JsonSerializerSettings setting = new JsonSerializerSettings();
                setting.NullValueHandling = NullValueHandling.Ignore;
                return JsonConvert.SerializeObject(obj, setting);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }

            return string.Empty;
        }

        public static int ConvertInt(string s, int defaultValue = 0)
        {
            int num = 0;
            if (int.TryParse(s, out num))
            {
                return num;
            }
            return defaultValue;
        }
        public static long ConvertLongInt(string s, long defaultValue = 0)
        {
            long num = 0;
            if (long.TryParse(s, out num))
            {
                return num;
            }
            return defaultValue;
        }

        public static string CountSize(long size)
        {
            string mStrSize = "";
            long factSize = 0;
            factSize = size;
            if (factSize < 1024.00)
                mStrSize = factSize.ToString("F2") + " Byte";
            else if (factSize >= 1024.00 && factSize < 1048576)
                mStrSize = (factSize / 1024.00).ToString("F2") + " K";
            else if (factSize >= 1048576 && factSize < 1073741824)
                mStrSize = (factSize / 1024.00 / 1024.00).ToString("F2") + " M";
            else if (factSize >= 1073741824)
                mStrSize = (factSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " G";
            return mStrSize;
        }
    }
}
