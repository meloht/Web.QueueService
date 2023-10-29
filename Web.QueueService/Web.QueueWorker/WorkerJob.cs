using NLog.Config;
using NLog.Targets;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.QueueWorker.Core;

namespace Web.QueueWorker
{
    public class WorkerJob : WorkerCore
    {
        public override void ShutdownLog()
        {
            LogManager.Shutdown();
        }

        protected override LoggingRule AddNLogRule()
        {
            ConsoleTarget consoleTarget = new ConsoleTarget("ConsoleTargetName");
            consoleTarget.Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}";

            LoggingRule ruleConsole = new LoggingRule("*", LogLevel.Trace, consoleTarget);

            return ruleConsole;
        }
        protected override void StartWork()
        {
            RunWorkTaskAsync().GetAwaiter().GetResult();
        }

        private async Task RunWorkTaskAsync()
        {
            while (true)
            {
                await base.RunTaskAsync();
                await Task.Delay(50);
            }
        }
    }
}
