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
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int MAX_ITERATIONS = 100;
    
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
        public async Task<IActionResult> GetExternalPriceData(TimeForAPI timeRange, CancellationToken cancellationToken)
        {
        try 
                {
                    if (timeRange == null)
                    {
                        return BadRequest("Request data is missing.");
                    }

                    var timeInterval = TimeInterval.FromString(timeRange.date_start, timeRange.date_end);
                    var priceData = await FetchDayAheadFromExternalAPI(timeInterval, cancellationToken);
                    return Ok(priceData);
                }
                catch (OperationCanceledException)
                {
                    return StatusCode(408, "Request timeout");
                }
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

        private async Task<List<DayAhead>> FetchDayAheadFromExternalAPI(TimeInterval timeInterval, CancellationToken cancellationToken)
        {
            var documents = new List<PublicationMarketDocument>();
            
            foreach (var country in Constants.countryDomains.Keys)
            {
                if (Constants.countryDomains[country] is string[] domainList)
                {
                    await ProcessMultipleRegions(country, domainList, timeInterval, documents,cancellationToken);
                }
                else
                {
                    await ProcessSingleRegion(country, Constants.countryDomains[country].ToString(), timeInterval, documents,cancellationToken);
                }
            }

            return _dayAheadService.CalculateElectricity(documents);
        }

        private async Task ProcessMultipleRegions(string country, string[] domains, TimeInterval originalInterval, List<PublicationMarketDocument> documents, CancellationToken cancellationToken)
        {
            List<PublicationMarketDocument> countryDocuments = new List<PublicationMarketDocument>();
        int iterations = 0;

        foreach (var domain in domains)
        {
            iterations = 0;
            var domainDocument = new PublicationMarketDocument
            {
                Country = country,
                TimeSeries = new List<TimeSeries>(),
                RevisionNumber = 1,
                Type = Constants.documentTypeParam
            };

            TimeInterval currentInterval = originalInterval;

            while (currentInterval.from < currentInterval.to && iterations++ < MAX_ITERATIONS)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var document = await FetchDocumentWithRetry(domain, currentInterval, cancellationToken);
                if (document == null)
                    break;
                    else { Console.WriteLine("obradio dokument"); }


                    if (document.TimeSeries != null && document.TimeSeries.Any())
                    {
                        domainDocument.TimeSeries.AddRange(document.TimeSeries);
                        Console.WriteLine("doda range");
                    }

                    DateTime maxDate = document.GetMaxDate();
                    if (maxDate > originalInterval.to)
                    {
                        maxDate = originalInterval.to;
                        Console.WriteLine("zamenio max");
                    }
                    if (maxDate >= currentInterval.to)
                        break;
                    Console.WriteLine("pre menjanja intervala");
                    currentInterval = new TimeInterval(maxDate.AddHours(1), originalInterval.to);
                    Console.WriteLine("Zamenio intervale");

                }

                if (domainDocument.TimeSeries.Any())
                {
                    countryDocuments.Add(domainDocument);
                }
            if (iterations >= MAX_ITERATIONS)
            {
                throw new TimeoutException($"Maximum iterations reached for domain {domain}");
            }

            }

            AggregateRegionalData(documents, countryDocuments);

            //if (countryDocuments.Any())
            //{
            //    AggregateRegionalData(documents, country, countryDocuments);
            //}
        }

        private async Task ProcessSingleRegion(string country, string domain, TimeInterval originalInterval, List<PublicationMarketDocument> documents, CancellationToken cancellationToken)
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
                var document = await FetchDocument(domain, currentInterval, cancellationToken);
                if (document == null)
                    break;
                else { Console.WriteLine("obradio dokument"); }

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
                Console.WriteLine("Zamenio intervale");
            }

            if (domainDocument.TimeSeries.Any())
            {
                documents.Add(domainDocument);
            }
        }

            private async Task<PublicationMarketDocument> FetchDocumentWithRetry(string domain, 
        TimeInterval timeInterval, CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < MAX_RETRY_ATTEMPTS; attempt++)
        {
            try
            {
                return await FetchDocument(domain, timeInterval, cancellationToken);
            }
            catch (Exception ex) when (attempt < MAX_RETRY_ATTEMPTS - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
            }
        }
        return null;
    }
     private async Task<PublicationMarketDocument> FetchDocument(string domain, 
        TimeInterval timeInterval, CancellationToken cancellationToken)
    {
        try
        {
            var url = _dayAheadService.BuildAPIUrl(domain, domain, timeInterval, _authToken);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode){
                Console.WriteLine($"Error fetching document for {domain}");
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(response.ReasonPhrase);
                Console.WriteLine(response.Content);
            }
                else
                {
                    Console.WriteLine("Ma dobro je eee");
                }
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine(content);

                return await _dayAheadService.DeserializeDocument(content, domain, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error fetching document for {domain}", ex);
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