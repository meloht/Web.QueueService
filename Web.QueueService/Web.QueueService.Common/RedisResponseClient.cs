using CSRedis;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class RedisResponseClient
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// second
        /// </summary>
        private static int Timeout = 120;
        private static CSRedis.CSRedisClient redisClient;


        public static void InitializeConnectionString(string cnxString, int timeout)
        {
            Timeout = timeout;
            if (string.IsNullOrWhiteSpace(cnxString))
                throw new ArgumentNullException(nameof(cnxString));
            redisClient = new CSRedis.CSRedisClient(cnxString);

        }
        private string ChannelName;

        public RedisResponseClient(string channelName, Action<CSRedisClient.SubscribeMessageEventArgs> receiveMessageHandle)
        {
            ChannelName = channelName;
            redisClient.Subscribe((channelName, receiveMessageHandle));
        }


        public RedisResponseClient(string channelName)
        {

            ChannelName = channelName;
        }
        public async Task SendMessageAsync(string message)
        {
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(ChannelName))
                {
                    redisClient.Publish(ChannelName, message);
                }
            });


        }


        public static bool Set(string entity, string key)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {

                bool bl = redisClient.Set(key, entity, Timeout);
                return bl;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                sw.Stop();
                logger.Info($"QueueID:{key} Redis set string executionTime:{sw.Elapsed.ToString()}");
            }

            return false;
        }


        public static bool Set<T>(T entity, string key) where T : class
        {
            try
            {
                string json = HttpUtils.ToJson(entity);
                return redisClient.Set(key, json, Timeout);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }


        public static T Get<T>(string key) where T : class
        {
            try
            {
                string value = redisClient.Get(key);
                return string.IsNullOrEmpty(value) ? null : HttpUtils.ToObject<T>(value);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return default(T);
        }

        public static string Get(string key)
        {
            try
            {
                string value = redisClient.Get(key);
                return value;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return null;
        }

        public static bool Remove(string key)
        {
            try
            {
                redisClient.Del(key);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;

        }

    }
}
