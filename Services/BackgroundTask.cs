namespace nigo.Services
{
    public class BackgroundTask : BackgroundService
    {
        private readonly ApiService _apiService;

        public BackgroundTask(ApiService apiService)
        {
            _apiService = apiService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _apiService.FetchAndStoreDataAsync();
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
