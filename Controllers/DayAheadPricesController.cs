using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using nigo.Models;
using nigo.Services;
using nigo.Utility;
using System.Text.Json;
using System.Text;
using System.Reflection.Metadata;

namespace nigo.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DayAheadPricesController : ControllerBase
    {
        private readonly string _authToken;
        private readonly IMemoryCache _cache;
        private readonly DayAheadService _dayAheadService;
        private readonly string _fastApiBaseUrl;
        private readonly HttpClient _httpClient;

        public DayAheadPricesController(IMemoryCache cache, IConfiguration configuration, HttpClient httpClient)
        {
            _cache = cache;
            _dayAheadService = new DayAheadService();
            _authToken = configuration.GetSection("ExternalTokenAPI").Value;
            _fastApiBaseUrl = configuration.GetSection("FastApiBaseUrl").Value;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        [HttpPost("country")]
        public async Task<IActionResult> GetCountryPrices(CountryRequestModel request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }

            var response = await SendFastApiRequest($"{_fastApiBaseUrl}/country", request);
            return response.IsSuccessStatusCode 
                ? Ok(await response.Content.ReadAsStringAsync())
                : StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ProcessChatMessage(BotMessage message)
        {
            if (message == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }

            var response = await SendFastApiRequest($"{_fastApiBaseUrl}/chat", message);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            return Ok(chatResponse);
        }

        [HttpPost("getExternalDataForDateRange")]
        public async Task<IActionResult> GetExternalPriceData(TimeForAPI timeRange)
        {
            if (timeRange == null)
            {
                return BadRequest("Request data is missing.");
            }

            var timeInterval = TimeInterval.FromString(timeRange.date_start, timeRange.date_end);
            var priceData = await FetchDayAheadFromExternalAPI(timeInterval);
            return Ok(priceData);
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Welcome to Day Ahead Prices API");
        }

        [HttpGet("lastRecord")]
        public async Task<IActionResult> GetLastRecord()
        {
            var response = await _httpClient.GetAsync($"{_fastApiBaseUrl}/last_record");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var lastRecord = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            return Ok(lastRecord);
        }

        [HttpGet("getDayAheadEurope")]
        public async Task<IActionResult> GetEuropePrices()
        {
            var response = await _httpClient.GetAsync($"{_fastApiBaseUrl}/getDayAheadEurope");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var priceData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responseBody);
            
            return priceData?.Count > 0 ? Ok(priceData) : NoContent();
        }

        private async Task<List<DayAhead>> FetchDayAheadFromExternalAPI(TimeInterval timeInterval)
        {
            var documents = new List<PublicationMarketDocument>();
            
            foreach (var country in Constants.countryDomains.Keys)
            {
                if (Constants.countryDomains[country] is string[] domainList)
                {
                    await ProcessMultipleRegions(country, domainList, timeInterval, documents);
                }
                else
                {
                    await ProcessSingleRegion(country, Constants.countryDomains[country].ToString(), timeInterval, documents);
                }
            }

            return _dayAheadService.CalculateElectricity(documents);
        }

        private async Task ProcessMultipleRegions(string country, string[] domains, TimeInterval originalInterval, List<PublicationMarketDocument> documents)
        {
            List<PublicationMarketDocument> countryDocuments = new List<PublicationMarketDocument>();

            foreach (var domain in domains)
            {
                var domainDocument = new PublicationMarketDocument
                {
                    Country = country,
                    TimeSeries = new List<TimeSeries>(),
                    RevisionNumber = 1,
                    Type = Constants.documentTypeParam
                };

                TimeInterval currentInterval = originalInterval;

                while (currentInterval.from < currentInterval.to)
                {
                    var document = await FetchDocument(domain, currentInterval);
                    if (document == null)
                        break;

                    if (document.TimeSeries != null && document.TimeSeries.Any())
                    {
                        domainDocument.TimeSeries.AddRange(document.TimeSeries);
                    }

                    DateTime maxDate = document.GetMaxDate();
                    if (maxDate > originalInterval.to)
                        maxDate = originalInterval.to;

                    if (maxDate >= currentInterval.to)
                        break;

                    currentInterval = new TimeInterval(maxDate.AddHours(1), originalInterval.to);
                }

                if (domainDocument.TimeSeries.Any())
                {
                    countryDocuments.Add(domainDocument);
                }


            }

            AggregateRegionalData(documents, countryDocuments);

            //if (countryDocuments.Any())
            //{
            //    AggregateRegionalData(documents, country, countryDocuments);
            //}
        }

        private async Task ProcessSingleRegion(string country, string domain, TimeInterval originalInterval, List<PublicationMarketDocument> documents)
        {

            TimeInterval currentInterval = originalInterval;
            var domainDocument = new PublicationMarketDocument
            {
                Country = country,
                TimeSeries = new List<TimeSeries>(),
                RevisionNumber = 1,
                Type = Constants.documentTypeParam
            };
            while (currentInterval.from < currentInterval.to)
            {
                var document = await FetchDocument(domain, currentInterval);
                if (document == null)
                    break;

                if (document.TimeSeries != null && document.TimeSeries.Any())
                {
                    domainDocument.TimeSeries.AddRange(document.TimeSeries);
                }

                DateTime maxDate = document.GetMaxDate();
                if (maxDate > originalInterval.to)
                    maxDate = originalInterval.to;

                if (maxDate >= currentInterval.to)
                    break;

                currentInterval = new TimeInterval(maxDate.AddHours(1), originalInterval.to);
            }

            if (domainDocument.TimeSeries.Any())
            {
                documents.Add(domainDocument);
            }
        }

        private async Task<PublicationMarketDocument> FetchDocument(string domain, TimeInterval timeInterval)
        {
            try
            {
                var url = _dayAheadService.BuildAPIUrl(domain, domain, timeInterval, _authToken);
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return _dayAheadService.DeserializeDocument(content, domain);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Request timed out for domain: {domain}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{domain}, {timeInterval.from.ToString()} - {timeInterval.to.ToString()}");
                Console.WriteLine(e.ToString());
                return null;
            }
            
        }

        private async Task<HttpResponseMessage> SendFastApiRequest<T>(string url, T data)
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json");
                
            return await _httpClient.PostAsync(url, jsonContent);
        }

        private static void AggregateRegionalData(List<PublicationMarketDocument> documents, List<PublicationMarketDocument> regionalDocuments)
        {
            // Validate input data
            if (!regionalDocuments.Any() ||
                regionalDocuments.Any(d => d.TimeSeries == null || !d.TimeSeries.Any()) ||
                regionalDocuments[0].TimeSeries[0]?.Period?.Point == null)
            {
                return;
            }

            var averageTimeSeries = new List<TimeSeries>();

            for (int seriesIndex = 0; seriesIndex < regionalDocuments[0].TimeSeries.Count; seriesIndex++)
            {
                var basePoints = regionalDocuments[0].TimeSeries[seriesIndex].Period.Point;

                var newTimeSeries = new TimeSeries
                {
                    Period = new Period
                    {
                        Point = basePoints.Select(p => new Point
                        {
                            Position = p.Position,
                            Price = regionalDocuments
                                .Where(doc => doc.TimeSeries.Count > seriesIndex &&
                                            doc.TimeSeries[seriesIndex]?.Period?.Point != null)
                                .Select(doc => doc.TimeSeries[seriesIndex].Period.Point
                                    .FirstOrDefault(pp => pp.Position == p.Position)?.Price ?? 0)
                                .DefaultIfEmpty(0)
                                .Average(),
                            Date = p.Date
                        }).ToList(),
                        xmlTimeIterval = regionalDocuments[0].TimeSeries[seriesIndex].Period.xmlTimeIterval,
                        Resolution = regionalDocuments[0].TimeSeries[seriesIndex].Period.Resolution,
                    }
                };

                averageTimeSeries.Add(newTimeSeries);
            }

            documents.Add(new PublicationMarketDocument
            {
                Country = regionalDocuments[0].Country,
                TimeSeries = averageTimeSeries,
                RevisionNumber = 1,
                Type = Constants.documentTypeParam
            });
        }
    }
}