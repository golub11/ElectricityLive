using nigo.Controllers;
using nigo.Models;
using nigo.Utility;

namespace nigo.Services
{
  
    public class ApiService
    {
        private readonly DayAheadPricesController _controller;

        public ApiService(DayAheadPricesController controller)
        {
            _controller = controller;
        }

        public async Task FetchAndStoreDataAsync()
        {
            var data = await _controller.GetAsync();
        }

        
    }

}
