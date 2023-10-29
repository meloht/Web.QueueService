namespace Web.QueueWorker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WorkerJob worker = new WorkerJob();
            worker.Init();
        }
    }
}