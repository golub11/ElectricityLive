using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using nigo.Models;
using nigo.Services;
using nigo.Utility;
using System.Text.Json;
using System.Text;

namespace nigo.Controllers
{
    //[Authorize] //TODO
    //[EnableCors("AllowGoluxSoftWebApp")] //TODO
    [ApiController]
    [Route("/api/[controller]")]
    public class DayAheadPricesController : ControllerBase
    {
        private string _token;
        private readonly IMemoryCache _cache;
        private DayAheadService dayAheadService;
        private readonly IConfiguration _configuration;
        private readonly String _baseFastApiUrl;
        private readonly HttpClient _client;


        public DayAheadPricesController(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;
            dayAheadService = new DayAheadService();
            _configuration = configuration;
            _token = _configuration.GetSection("ExternalTokenAPI").Value;
            _baseFastApiUrl = _configuration.GetSection("FastApiBaseUrl").Value;
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromMinutes(10);

        }

        [HttpPost("country")]
        public async Task<IActionResult> PostCountryData(CountryRequestModel energy)
        {
            if (energy == null)
            {
                return BadRequest("Request data is missing.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }



            string apiUrl = $"{_baseFastApiUrl}/country";
            string jsonRequest = JsonSerializer.Serialize(energy);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(apiUrl, content);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, responseBody);
            }

            return Ok(responseBody);

        }

        [HttpPost("chat")]
        public async Task<IActionResult> PostChatBot(BotMessage question)
        {
            if (question == null)
            {
                return BadRequest("Request data is missing.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }


            string apiUrl = $"{_baseFastApiUrl}/chat";
            string jsonRequest = JsonSerializer.Serialize(question);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(apiUrl, content);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, responseBody);
            }

            var countryData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            return Ok(countryData);

        }




        [HttpPost("getExternalDataForDateRange")]
        public async Task<IActionResult> GetRangeDayAheadPriceForAllCountriesExternal(TimeForAPI timeRange)
        {
            if (timeRange == null)
            {
                return BadRequest("Request data is missing.");
            }

            TimeInterval timeInterval = TimeInterval.FromString(
                timeRange.date_start,
                timeRange.date_end
            );
            var data = await FetchDayAheadFromExternalAPI(timeInterval);
            return Ok(data);
        }

        [HttpGet("getDayAheadEurope")]
        public async Task<IActionResult> GetDayAheadPriceForAllCountriesLocal()
        {
            List<dynamic> data = new List<dynamic>();
            string apiUrl = $"{_baseFastApiUrl}/getDayAheadEurope";
            HttpResponseMessage response = await _client.GetAsync(apiUrl);//, content);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, responseBody);
            }

            var countryData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responseBody);

            if (countryData != null && countryData.Count > 0) { 
                data.AddRange(countryData);
                return Ok(data);

            }
            return NoContent();
        }

        [HttpGet("demo")]
        public async Task<IActionResult> demo()
        {
            var data = "asd";            
                return Ok(data);

        }

        //[HttpGet("{country}")]
        //public async Task<IActionResult> GetCountryData(string country)
        //{
        //    string apiUrl = $"/{country}";
        //    HttpResponseMessage response = await _client.GetAsync(apiUrl);
        //    response.EnsureSuccessStatusCode();

        //    string responseBody = await response.Content.ReadAsStringAsync();

        //    var countryData = JsonSerializer.Deserialize<object>(responseBody);

        //    // Return the deserialized object as an HTTP OK response
        //    return Ok(countryData);
        //}

        private async Task<List<DayAhead>> FetchDayAheadFromExternalAPI(TimeInterval timeInterval)
        {

            var countries = Constants.countryDomains.Keys;
            int counter = 3;
            List<PublicationMarketDocument> documents = new List<PublicationMarketDocument>();

            string webUrl;
            PublicationMarketDocument document;
            
            foreach (var country in countries)
            {
                //counter--;
                //if (counter == 0)
                //    break;
                List<PublicationMarketDocument> countryDocuments = new List<PublicationMarketDocument>();
                if (Constants.countryDomains[country] is string[] domainList)
                {
                    foreach (var domain in domainList)
                    {
                        webUrl = dayAheadService.BuildAPIUrl(domain, domain, timeInterval, _token)!;
                        document = await FetchDocumentFromAPI(webUrl);

                        if (document != null)
                        {
                            document.Country = country;
                            countryDocuments.Add(document);
                        }
                        else
                        {
                            Console.WriteLine($"Error for country {country}");
                        }
                    }
                    handleMultipleRegions(documents, country, countryDocuments);
                }
                else
                {
                    webUrl = dayAheadService.BuildAPIUrl(
                        Constants.countryDomains[country],
                        Constants.countryDomains[country],
                        timeInterval,
                        _token
                    )!;

                    document = await FetchDocumentFromAPI(webUrl);
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
            }

            List<DayAhead> electricityForEurope = dayAheadService.CalculateElectricity(documents);
            return electricityForEurope;
        }

        private async Task<PublicationMarketDocument> FetchDocumentFromAPI(string webUrl)
        {
            HttpResponseMessage response = await _client.GetAsync(webUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return dayAheadService.DeserializeDocument(responseBody);
        }

        private static void handleMultipleRegions(List<PublicationMarketDocument> documents, string country, List<PublicationMarketDocument> countryDocuments)
        {
            if (countryDocuments.Count > 0)
            {
                List<TimeSeries> averageTimeSeries = new List<TimeSeries>();

                for (int i = 0; i < countryDocuments[0].TimeSeries.Count; i++) {
                    TimeSeries ts = new TimeSeries
                    {
                        Period = new Period
                        {
                            Point = new List<Point>(),
                        }
                    };
                    foreach (var documentTmp in countryDocuments)
                    {
                        TimeSeries currentTimeSeries = documentTmp.TimeSeries[i];
                        foreach (var point in currentTimeSeries.Period.Point)
                        {
                            var existingPoint = currentTimeSeries.Period.Point
                                .FirstOrDefault(p => p.Position == point.Position);
                            var newPoint = ts.Period.Point
                                .FirstOrDefault(p => p.Position == point.Position);
                            if (newPoint == null && existingPoint != null)
                            {
                                ts.Period.Point.Add(existingPoint);
                                ts.Period.xmlTimeIterval = currentTimeSeries.Period.xmlTimeIterval;
                                ts.Period.Resolution = currentTimeSeries.Period.Resolution;
                            }
                            else
                            {
                                newPoint.Price += existingPoint.Price;
                            }
                        }

                    }
                    averageTimeSeries.Add(ts);
                                        
                }
                var averageDocument = new PublicationMarketDocument
                {
                    Country = country,
                    TimeSeries = averageTimeSeries,
                    RevisionNumber = 1,
                    Type = Constants.documentTypeParam
                };
                foreach (var timeSeries in averageDocument.TimeSeries)
                {

                    foreach (var point in timeSeries.Period.Point)
                    {
                        point.Price /= countryDocuments.Count(d => d.Country == country);
                    }
                }

                documents.Add(averageDocument);
            }
        }
    }
}