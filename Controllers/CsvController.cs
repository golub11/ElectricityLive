using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using nigo.Utility;
using System.Text;

namespace nigo.Controllers
{
    [ApiController]
    [Route("api/daily/[controller]")]
    public class CsvController : ControllerBase
    {
        /*private readonly AppCache _cache;

        public CsvController(AppCache cache)
        {
            _cache = cache;
        }*/
        public CsvController()
        {

        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            // Try to retrieve data from the cache

            var _filePath = Constants.csvFilePath;

            var _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            return File(_fileStream, "text/csv", "YourFile.csv");

            /*var data = await _cache.GetAsync("fileStream");

            if (data != null)
            {
                // Data was found in the cache
                return Ok(File(data, "text/csv", "YourFile.csv"));
                //return Ok(Encoding.UTF8.GetString(data));
            }
            else
            {

                var _filePath = Constants.csvFilePath;

                var _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);

                //var cacheOptions = new DistributedCacheEntryOptions()
                //.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                //await _cache.SetAsync("fileStream", Encoding.UTF8.GetBytes(_fileStream.ToString()), cacheOptions);

                return File(_fileStream, "text/csv", "YourFile.csv");
            }
            */
        }

    }
}
