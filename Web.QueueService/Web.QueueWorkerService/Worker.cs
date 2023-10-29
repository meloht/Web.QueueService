namespace Web.QueueWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        WorkServiceCore serviceCore;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            serviceCore = new WorkServiceCore();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            serviceCore.Init();
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await serviceCore.RunItemAsync();
                await Task.Delay(50, stoppingToken);
            }
        }
    }
}