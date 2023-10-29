using Microsoft.ApplicationInsights.NLogTarget;
using NLog.Config;
using NLog.Targets;
using NLog;
using Web.QueueService.Common;
using Microsoft.Extensions.Configuration;
using LogLevel = NLog.LogLevel;

namespace Web.QueueService.API.Middleware
{
    public static class RequestMiddlewareExtensions
    {
        public static AppSettingsQueueApi Config;
        public static IApplicationBuilder UseRequestHandler(
           this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestHandlerMiddleware>();
        }


        public static IServiceCollection AddConfig(
            this IServiceCollection services, IConfiguration config)
        {
            Config = new AppSettingsQueueApi(config);
            InitNlog(config);
            RedisResponseClient.InitializeConnectionString(Config.RedisResponse, Config.HttpTimeOut);
            StorageBlobClient.Init(Config.StorageBlobConnectionString, Config.BlobContainName);

            return services;
        }

        private static void InitNlog(IConfiguration configuration)
        {
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("ApplicationInsightsTarget",
                 typeof(Microsoft.ApplicationInsights.NLogTarget.ApplicationInsightsTarget));

            var loggingconfig = new LoggingConfiguration();

            ApplicationInsightsTarget target = new ApplicationInsightsTarget();
            target.InstrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];
            target.Layout = "${longdate}|QueueAPI|${machinename}|${message} ${exception:format=tostring}";

            LoggingRule rule = new LoggingRule("*", LogLevel.Trace, target);
            loggingconfig.LoggingRules.Add(rule);

            ConsoleTarget consoleTarget = new ConsoleTarget("ConsoleTargetName");
            consoleTarget.Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}";



            LoggingRule ruleConsole = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            loggingconfig.LoggingRules.Add(ruleConsole);


            FileTarget fileTarget = new FileTarget("fileTargetName");
            fileTarget.Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}";
            fileTarget.FileName = "logs/${shortdate}.log";

            //LoggingRule ruleFile = new LoggingRule("*", LogLevel.Trace, fileTarget);
            //loggingconfig.LoggingRules.Add(ruleFile);

            LogManager.Configuration = loggingconfig;
        }


    }
}
