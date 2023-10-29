using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.NLogTarget;
using Microsoft.Extensions.Configuration;
using NLog.Config;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.QueueService.Common;

namespace Web.QueueWorker.Core
{
    public abstract class WorkerCore
    {
        public static AppSettingsWorkerJob Config;
        private static QueueReceiveClientCore queueReceiveClient;
        private static string InstrumentationKey = "";

        public void Init()
        {
            Logger logger = null;

            try
            {
                //var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                //var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                var config = new ConfigurationBuilder()
                   .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .Build();

                InstrumentationKey = config["ApplicationInsights:InstrumentationKey"];

                TelemetryConfiguration.Active.InstrumentationKey = InstrumentationKey;

                ConfigurationItemFactory.Default.Targets.RegisterDefinition("ApplicationInsightsTarget",
                    typeof(Microsoft.ApplicationInsights.NLogTarget.ApplicationInsightsTarget));


                InitNlog();

                logger = LogManager.GetCurrentClassLogger();

                Config = new AppSettingsWorkerJob(config);
                HttpUtils.InitHttpClient();

                StorageBlobClient.Init(Config.StorageBlobConnectionString, Config.BlobContainName);
                RedisResponseClient.InitializeConnectionString(Config.RedisResponse, Config.HttpTimeOut);

                queueReceiveClient = new QueueReceiveClientCore(Config);

                Console.WriteLine($"RegisterOnMessageHandler start ");

                logger.Info($"nlog QueueWorker start");

                StartWork();

            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                if (logger != null)
                {
                    logger.Error(ex, "Stopped program because of exception");
                }
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                ShutdownLog();
            }
        }

        public abstract void ShutdownLog();

        private void InitNlog()
        {
            var loggingconfig = new LoggingConfiguration();

            ApplicationInsightsTarget target = new ApplicationInsightsTarget();
            target.InstrumentationKey = InstrumentationKey;
            target.Layout = "${longdate}|QueueWorker|${machinename}|${message} ${exception:format=tostring}";

            LoggingRule rule = new LoggingRule("*", LogLevel.Trace, target);
            loggingconfig.LoggingRules.Add(rule);

            var newRule = AddNLogRule();
            if (newRule != null)
            {
                loggingconfig.LoggingRules.Add(newRule);
            }

            LogManager.Configuration = loggingconfig;
        }

        protected virtual LoggingRule AddNLogRule()
        {
            return null;
        }

        protected abstract void StartWork();



        protected async Task RunTaskAsync()
        {
            await queueReceiveClient.ProcessMessageTaskAsync();
        }
    }
}
