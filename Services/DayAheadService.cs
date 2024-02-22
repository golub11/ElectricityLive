using System;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using nigo.Models;
using nigo.Utility;

namespace nigo.Services
{
	public class DayAheadService
	{
        public DayAheadService()
        { }
        public string FormatDateToUTCMidnightForAPI(DateTime date)
        {
            date = date.AddDays(-1);
            return $"{date.Year}{date:MM}{date:dd}2200";
        }

        public string? BuildAPIUrl(
            string inDomain, string outDomain, TimeInterval timeInterval, string _token)
        {
            if (Constants.countryDomains.TryGetValue(inDomain, out string _inDomain))
            {
                if (Constants.countryDomains.TryGetValue(outDomain, out string _outDomain))
                {
                    string dateFromAPIFormat = FormatDateToUTCMidnightForAPI(timeInterval.from);
                    string dateToAPIFormat = FormatDateToUTCMidnightForAPI(timeInterval.to);
                    return
                        Constants.apiUrl +
                        Constants.documentTypeParam + "=" + DocumentType.priceDocument + "&" +
                        Constants.inDomainParam + "=" + _inDomain + "&" +
                        Constants.outDomainParam + "=" + _outDomain + "&" +
                        Constants.periodStartParam + "=" + dateFromAPIFormat + "&" +
                        Constants.periodEndParam + "=" + dateToAPIFormat + "&" +
                        Constants.securityTokenParam + "=" + _token;
                    
                }
            }
            return null;
            //return "Some of required fields missing or in wrong format.";
        }

        public PublicationMarketDocument DeserializeDocument(string xmlContent)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PublicationMarketDocument));
            PublicationMarketDocument? document;

            serializer.UnknownNode += new XmlNodeEventHandler(XmlHelper.serializerUnknownNode!);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(XmlHelper.serializerUnknownAttribute!);

            using TextReader sr = new StringReader(xmlContent);
            try
            {
                document = serializer.Deserialize(sr) as PublicationMarketDocument;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                document = null;
            }

            return document;
        }

        public List<DayAhead> CalculateElectricity(
            List<PublicationMarketDocument> documents
        ){
            List<DayAhead> electricityForEurope = new List<DayAhead>();
            TimeInterval timeInterval;
            foreach (PublicationMarketDocument record in documents)
            {
                foreach(TimeSeries ts in record.TimeSeries)
                {
                    List<Point> points = ts.Period.Point;
                    var numberOfMeasures = points.Count;
                    var country = record.Country;
                    double dailySum = points.Sum(p => p.PriceAmount);
                    double averagePrice = dailySum / numberOfMeasures;
                    var measurementIntervalMinutes = ts.Period.Resolution;
                    
                    timeInterval = TimeInterval.FromUTCString(
                        ts.Period.xmlTimeIterval.Start,
                        ts.Period.xmlTimeIterval.End
                    );

                    foreach(Point p in points)
                    {
                        p.Date = timeInterval.from.AddHours(
                            p.Position - 1
                            );
                    }

                    electricityForEurope.Add(
                        new DayAhead(
                            timeInterval,
                            country,
                            points,
                            averagePrice,
                            measurementIntervalMinutes
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

