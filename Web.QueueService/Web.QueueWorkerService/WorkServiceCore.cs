using NLog.Config;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.QueueWorker.Core;

namespace Web.QueueWorkerService
{
    public class WorkServiceCore : WorkerCore
    {
        private volatile bool IsRun = true;
        protected override void StartWork()
        {
            

        }
        public async Task RunItemAsync()
        {
            await base.RunTaskAsync();

        }
        protected override LoggingRule AddNLogRule()
        {
            //FileTarget fileTarget = new FileTarget("fileTargetName");
            //fileTarget.Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}";
            //fileTarget.FileName = "logs/${shortdate}.log";

            //LoggingRule ruleFile = new LoggingRule("*", LogLevel.Trace, fileTarget);

            //return ruleFile;
            //寫文本日志，會使性能下降 
            return null;
        }



        public override void ShutdownLog()
        {
            LogManager.Shutdown();
        }
    }
}
