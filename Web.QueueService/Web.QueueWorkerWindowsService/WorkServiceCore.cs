using NLog.Config;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.QueueWorker.Core;

namespace Web.QueueWorkerWindowsService
{
    public class WorkServiceCore : WorkerCore
    {
        private Thread thread;
        private volatile bool IsRun = true;
        protected override void StartWork()
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
            }
            IsRun = true;
            thread = new Thread(() => { RunItemAsync().GetAwaiter().GetResult(); });
            thread.Start();

        }
        private async Task RunItemAsync()
        {
            while (IsRun)
            {
                await base.RunTaskAsync();
                await Task.Delay(50);
            }

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

        public void Stop()
        {
            IsRun = false;
            ShutdownLog();
            if (thread != null)
            {
                thread.Join();
            }
        }

        public override void ShutdownLog()
        {
            LogManager.Shutdown();
        }
    }
}
