using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using nigo.Models;
using nigo.Utility;

namespace nigo.Services
{
	public class DayAheadService
	{
        private const string HOURLY_FREQ = "PT60M";
        private const string MIN_15_FREQ = "PT15M";

        public DayAheadService()
        { }
        public string FormatDateToUTCMidnightForAPI(DateTime date)
        {
            date = date.AddDays(-1);
            return $"{date.Year}{date:MM}{date:dd}2300";
        }

        public string? BuildAPIUrl(
            string inDomain, string outDomain, TimeInterval timeInterval, string _token)
        {
                    
                    string dateFromAPIFormat = FormatDateToUTCMidnightForAPI(timeInterval.from);
                    string dateToAPIFormat = FormatDateToUTCMidnightForAPI(timeInterval.to);
                    return
                        Constants.apiUrl +
                        Constants.documentTypeParam + "=" + DocumentType.priceDocument + "&" +
                        Constants.inDomainParam + "=" + inDomain + "&" +
                        Constants.outDomainParam + "=" + outDomain + "&" +
                        Constants.periodStartParam + "=" + dateFromAPIFormat + "&" +
                        Constants.periodEndParam + "=" + dateToAPIFormat + "&" +
                        Constants.securityTokenParam + "=" + _token;
                    

        }
        private string GetCountryFromDomain(string domain)
        {
            return Constants.countryDomains.FirstOrDefault(x =>
                x.Value is string ? x.Value == domain :
                x.Value is string[] && ((string[])x.Value).Contains(domain)
            ).Key;
        }
        
            public async Task<PublicationMarketDocument> DeserializeDocument(string xmlContent, string domain, CancellationToken cancellationToken = default)
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        string namespaceVersion = ExtractNamespaceVersion(xmlContent);
                        var overrides = new XmlAttributeOverrides();
                        var attrs = new XmlAttributes();
                        attrs.XmlRoot = new XmlRootAttribute
                        {
                            ElementName = "Publication_MarketDocument",
                            Namespace = $"{PublicationMarketDocument.BaseNamespace}{namespaceVersion}"
                        };

                        overrides.Add(typeof(PublicationMarketDocument), attrs);
                        var serializer = new XmlSerializer(typeof(PublicationMarketDocument), overrides);
                        
                        using TextReader sr = new StringReader(xmlContent);
                        var document = (PublicationMarketDocument)serializer.Deserialize(sr);
                        
                        if (document == null)
                        {
                            throw new InvalidOperationException($"Failed to deserialize document for domain {domain}");
                        }

                        document.Country = GetCountryFromDomain(domain);
                        ProcessTimeSeries(document);
                        
                        return document;
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }, cancellationToken);
            }

            private void ProcessTimeSeries(PublicationMarketDocument document)
            {
                document.TimeSeries = document.TimeSeries
                    .Where(ts => (ts.Period.Resolution == MIN_15_FREQ || 
                                ts.Period.Resolution == HOURLY_FREQ) && 
                                ts.Period.Point.Count <= 25)
                    .ToList();

                foreach (var ts in document.TimeSeries)
                {
                    ProcessTimeSeriesPoints(ts);
                }
            }

            private void ProcessTimeSeriesPoints(TimeSeries ts)
            {
                double? lastValue = null;
                var resolution = ts.Period.Resolution;
                var points = ts.Period.Point;
                var processedPoints = new List<Point>();
                var timeInterval = ts.Period.xmlTimeIterval.ToTimeInterval();
                
                int factor = resolution == HOURLY_FREQ ? 1 : 4;
                
                for (int i = 1; i <= 24; i++)
                {
                    int index = resolution == HOURLY_FREQ ? i : factor * i - 3;
                    ProcessPoint(points, processedPoints, timeInterval, index, ref lastValue);
                }
                
                ts.Period.Point = processedPoints;
            }

            private void ProcessPoint(List<Point> points, List<Point> processedPoints, 
                TimeInterval timeInterval, int index, ref double? lastValue)
            {
                var currentPoint = points.FirstOrDefault(p => p.Position == index);
                if (currentPoint != null)
                {
                    lastValue = currentPoint.Price;
                    currentPoint.Date = timeInterval.from.AddHours(index - 1);
                    processedPoints.Add(currentPoint);
                }
                else if (lastValue.HasValue)
                {
                    processedPoints.Add(new Point
                    {
                        Position = index,
                        Price = lastValue.Value,
                        Date = timeInterval.from.AddHours(index - 1)
                    });
                }
            }

        private string ExtractNamespaceVersion(string xmlContent)
        {
            var regex = new Regex(@"publicationdocument:(\d+:\d+)""");
            var match = regex.Match(xmlContent);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "7:0";
        }

        public List<DayAhead> CalculateElectricity(
            List<PublicationMarketDocument> documents
        ){
            List<DayAhead> electricityForEurope = new List<DayAhead>();
            foreach (PublicationMarketDocument record in documents)
            {
                foreach(TimeSeries ts in record.TimeSeries)
                {
                    List<Point> points = ts.Period.Point;
                    var country = record.Country;
                    electricityForEurope.Add(
                        new DayAhead(
                            country,
                            points
                            )
                    );
                }
                
            }

            return electricityForEurope;
        }

        public ErrorTypeDocument generateDomainErrorDocument(string domain)
        {
            Console.WriteLine($"There is error using {domain} Country value");
            ErrorTypeDocument domainError = new ErrorTypeDocument
            {
                Error = "Bad request",
                Message = $"There is error using {domain} Country value"
            };

            XmlSerializer errorSerializer = new XmlSerializer(
                typeof(ErrorTypeDocument)
            );
            StringWriter sw = new StringWriter();
            errorSerializer.Serialize(sw, domainError);
            return domainError;
        }

        public string GetTodayDateString()
        {
            DateTime today = DateTime.Today;

            string todayString = today.ToString("yyyy-MM-dd");

            return todayString;
        }

        public string GetTomorrowDateString()
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);

            string tomorrowString = tomorrow.ToString("yyyy-MM-dd");

            return tomorrowString;
        }
    }
	
}

