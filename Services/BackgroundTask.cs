using System;
using Microsoft.Extensions.Configuration;

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
                try
                { 
                    await _apiService.FetchAndStoreDataAsync();
                }
                catch(Exception e)
                {
                    Console.WriteLine("ERROR: in background service. " + e.Message);
                }
                await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
            }
        }
    }
}
