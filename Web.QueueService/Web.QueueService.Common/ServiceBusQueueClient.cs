using Microsoft.Azure.ServiceBus;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.QueueService.Common
{
    public class ServiceBusQueueClient : IDisposable
    {
        private readonly string ConnectionString;
        private readonly string QueueName;
        private readonly IQueueClient QueueClient;
        private int PoolSize = 100;

        private Logger logger = LogManager.GetCurrentClassLogger();
        public ServiceBusQueueClient(string connectionString, string queueName)
        {
            try
            {
                ConnectionString = connectionString;
                QueueName = queueName;

                QueueClient = new QueueClient(ConnectionString, QueueName, ReceiveMode.ReceiveAndDelete);
                QueueClient.PrefetchCount = 300;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }
        public ServiceBusQueueClient(string connectionString, string queueName, int poolSize, Func<Message, CancellationToken, Task> handler)
            : this(connectionString, queueName)
        {
            PoolSize = poolSize;
            RegisterOnMessageHandler(handler);
        }



        private void RegisterOnMessageHandler(Func<Message, CancellationToken, Task> handler)
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 100,

                AutoComplete = true
            };

            // Register the function that processes messages.
            QueueClient.RegisterMessageHandler(handler, messageHandlerOptions);

        }

        // Use this handler to examine the exceptions received on the message pump.
        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            logger.Error($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception.ToString()}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            logger.Info("Exception context for troubleshooting:");
            logger.Info($"- Endpoint: {context.Endpoint}");
            logger.Info($"- Entity Path: {context.EntityPath}");
            logger.Info($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        public async Task CompleteAsync(string lockToken)
        {
            await QueueClient.CompleteAsync(lockToken);
        }

        public void Dispose()
        {
            try
            {
                QueueClient.CloseAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        public async Task SendMessagesAsync(string json, string guid)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                var message = new Message(Encoding.UTF8.GetBytes(json));

                await QueueClient.SendAsync(message);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            sw.Stop();

            logger.Info($"QueueID:{guid} QueueClient SendAsync executionTime:{sw.Elapsed.ToString()}");

        }

        public async Task SendMessagesAsync(string json)
        {

            try
            {
                var message = new Message(Encoding.UTF8.GetBytes(json));

                await QueueClient.SendAsync(message);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }


        }



    }
}
