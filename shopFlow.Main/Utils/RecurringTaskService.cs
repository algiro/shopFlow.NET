
using BlazorBootstrap;
using shopFlow.Config;

namespace shopFlow.Utils
{
    public class RecurringTaskService : BackgroundService
    {
        private readonly ILogger _logger = LoggerUtils.CreateLogger<RecurringTaskService>();
        private readonly HealthcheckPing _pinger;
        private readonly int _healthchecksIntervalSec;
        public RecurringTaskService() 
        {
            _healthchecksIntervalSec = ShopFlowConfig.Instance.HealtCheckIntervalSec;
            _pinger = new HealthcheckPing(ShopFlowConfig.Instance.HealtCheckUrl);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RecurringTaskService is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                await _pinger.PingAsync();
                await Task.Delay(TimeSpan.FromSeconds(_healthchecksIntervalSec), stoppingToken);
            }
            _logger.LogInformation("RecurringTaskService is stopping.");
        }
    }
}
