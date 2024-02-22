using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using nigo.Models;
using nigo.Services;
using nigo.Utility;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace nigo.Controllers
{
    [Authorize]
    //[EnableCors("AllowGoluxSoftWebApp")] //TODO
    [ApiController]
    [Route("/api/[controller]")]
    public class DayAheadPricesController : ControllerBase
    {
        
        private string _token;
        private readonly IMemoryCache _cache;
        private DayAheadService dayAheadService;
        private readonly IConfiguration _configuration;

        public DayAheadPricesController(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;
            dayAheadService = new DayAheadService();
            _configuration = configuration;
            _token = _configuration.GetSection("ExternalTokenAPI").Value;
;
        }
        
        
        //TODO cache once enabled
        //private readonly ILogger<DayAheadPricesController> _logger;


        [HttpPost("country")]
        public async Task<IActionResult> PostAsync(EnergyData energy)
        {
            HttpClient client = new HttpClient();

            if (energy == null)
            {
                return BadRequest("Request data is missing.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }

            TimeInterval timeInterval = TimeInterval.FromString(
                energy.PeriodStart,
                energy.PeriodEnd
            );

            string webUrl = dayAheadService.BuildAPIUrl(
                        energy.InDomain,
                        energy.OutDomain,
                        timeInterval,
                        _token
                        )!;

           
            if (webUrl is not null)
            {
                using HttpResponseMessage response = await client.GetAsync(webUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();


                PublicationMarketDocument document = dayAheadService.DeserializeDocument(responseBody);
                if (document != null)
                {
                    document.Country = energy.InDomain;
                    List<PublicationMarketDocument> documents = new List<PublicationMarketDocument>
                    {
                        document
                    };


                    List<DayAhead> electricityForCountry = dayAheadService.CalculateElectricity(
                        documents
                    );
                    return Ok(electricityForCountry);

                }
                
                Console.WriteLine($"Error for country {energy.InDomain}");
            }

            return NoContent();
        }

        
        [HttpGet("all")]
        public async Task<IActionResult> GetAsync()
        {   

            if (!_cache.TryGetValue("dayAheadAllCountries", out var cachedData))
            {
                var data = await FetchDataFromExternalAPI();
                _cache.Set("dayAheadAllCountries", data, TimeSpan.FromHours(12));

                cachedData = data;
            }
            return Ok(cachedData);
        

        }

        private async Task<List<DayAhead>> FetchDataFromExternalAPI()
        {
            using HttpClient client = new HttpClient();
            List<PublicationMarketDocument> documents = new List<PublicationMarketDocument>();

            TimeInterval timeInterval = TimeInterval.FromString(
                dayAheadService.GetTodayDateString()!,
                dayAheadService.GetTomorrowDateString()!
                );

            string webUrl;

            foreach (var country in Constants.countryDomains.Keys)
            {


                webUrl = dayAheadService.BuildAPIUrl(
                    country,
                    country,
                    timeInterval,
                    _token
                    )!;

                HttpResponseMessage response = await client.GetAsync(webUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                PublicationMarketDocument document = dayAheadService.DeserializeDocument(responseBody);
                if (document != null)
                {
                    document.Country = country;
                    documents.Add(document);
                }
                else
                {
                    Console.WriteLine($"Error for country {country}");
                }
            }

            List<DayAhead> electricityForEurope = dayAheadService.CalculateElectricity(documents);
            return electricityForEurope;
        }
    }
}


