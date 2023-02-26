using nigo.Controllers;
using nigo.Models;
using nigo.Utility;

namespace nigo.Services
{
  
    public class ApiService
    {
        private readonly DayAheadPricesController _controller;
        private readonly FileWriter<PublicationMarketDocument> _writer;
        //private readonly HttpClient _httpClient;
        //private readonly string _apiUrl;
        private readonly string _cacheFilePath;

        public ApiService(string cacheFilePath)
        {
            _controller = new DayAheadPricesController();
            //_httpClient = new HttpClient();
            //_apiUrl = apiUrl;
            _cacheFilePath = cacheFilePath;
            _writer = new FileWriter<PublicationMarketDocument>(_cacheFilePath);
        }

        public async Task FetchAndStoreDataAsync()
        {
            var data = await _controller.GetAsync();
            _writer.WriteList(data);
            
        }

        public async Task<string> GetDataAsync()
        {
            if (File.Exists(_cacheFilePath))
            {
                var data = await File.ReadAllTextAsync(_cacheFilePath);
                return data;
            }
            else
            {
                await FetchAndStoreDataAsync();
                return await GetDataAsync();
            }
        }
    }

}
