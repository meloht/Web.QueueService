using System.ServiceProcess;
using Web.QueueWorkerService;

namespace Web.QueueWorkerWindowsService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (WorkerService worker = new WorkerService())
            {
                ServiceBase.Run(worker);
            }
        }
    }
}