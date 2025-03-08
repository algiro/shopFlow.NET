namespace shopFlow.Utils
{
    using shopFlow.Config;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class HealthcheckPing
    {
        private static readonly ILogger _logger = LoggerUtils.CreateLogger<HealthcheckPing>();
        private readonly string _healthchecksUrl;
        private readonly HttpClient _httpClient;

        public HealthcheckPing(string healthchecksUrl)
        {
            _healthchecksUrl = healthchecksUrl;
            _logger.LogInformation($"HealthcheckPing C.tor URL: {_healthchecksUrl}");
            _httpClient = new HttpClient();
        }

        public async Task PingAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(_healthchecksUrl);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Healthchecks.io ping successful.");
                }
                else
                {
                    _logger.LogError($"Healthchecks.io ping failed: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured while pinging Healthchecks.io: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unexpected error occured while pinging Healthchecks.io: {ex.Message}");
            }
        }
    }
}
