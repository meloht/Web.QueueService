using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Web.QueueWorkerWindowsService;

namespace Web.QueueWorkerService
{
    public class WorkerService : ServiceBase
    {
        WorkServiceCore serviceCore;
        public WorkerService()
        {
            ServiceName = "bpmWorkerService";
            serviceCore = new WorkServiceCore();

        }
        protected override void OnStart(string[] args)
        {
            serviceCore.Init();

        }

        protected override void OnStop()
        {
            serviceCore.Stop();
        }


    }
}
