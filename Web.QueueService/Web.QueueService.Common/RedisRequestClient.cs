using CSRedis;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class RedisRequestClient
    {
        private static CSRedisClient client;

        public static void Init(string conn)
        {
            client = new CSRedisClient(conn);
        }

        private string ChannelName;
        public string GetChannelName
        {
            get
            {
                return ChannelName;
            }
        }

        private List<string> ChannelList = new List<string>();
        public RedisRequestClient(string channelName)
        {
            ChannelName = channelName;
        }

        public RedisRequestClient(string channelName, Action<CSRedisClient.SubscribeMessageEventArgs> receiveMessageHandle)
        {

            ChannelName = channelName;
            client.Subscribe((channelName, receiveMessageHandle));

        }



        public RedisRequestClient(params (string, Action<CSRedisClient.SubscribeMessageEventArgs>)[] channels)
        {
            client.Subscribe(channels);
            ChannelList.AddRange(channels.Select(p => p.Item1).ToList());
        }

        public async Task SendMessageAsync(string message, string guid, ILogger logger)
        {
            await Task.Run(() =>
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                if (!string.IsNullOrEmpty(ChannelName))
                {
                    client.Publish(ChannelName, message);
                }
                foreach (var item in ChannelList)
                {
                    client.Publish(item, message);
                }
                sw.Stop();
                logger.Info($"QueueID:{guid}==SendMessagesAsync executionTime: {sw.Elapsed.ToString()}");

            });


        }

    }
}
