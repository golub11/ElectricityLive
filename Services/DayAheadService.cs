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

        /*
         * public PublicationMarketDocument DeserializeDocument(string xmlContent)
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
                //Console.WriteLine(e.Message);
                document = null;
            }

            return document;
        }*/
        public PublicationMarketDocument DeserializeDocument(string xmlContent)
        {
            // Extract the namespace version from the XML content
            string namespaceVersion = ExtractNamespaceVersion(xmlContent);

            // Create XmlAttributeOverrides to override the namespace
            var overrides = new XmlAttributeOverrides();
            var attrs = new XmlAttributes();
            attrs.XmlRoot = new XmlRootAttribute
            {
                ElementName = "Publication_MarketDocument",
                Namespace = $"{PublicationMarketDocument.BaseNamespace}{namespaceVersion}"
            };

            overrides.Add(typeof(PublicationMarketDocument), attrs);

            // Create serializer with the overridden namespace
            var serializer = new XmlSerializer(typeof(PublicationMarketDocument), overrides);
            serializer.UnknownNode += new XmlNodeEventHandler(XmlHelper.serializerUnknownNode!);
            serializer.UnknownAttribute += new XmlAttributeEventHandler(XmlHelper.serializerUnknownAttribute!);

            using TextReader sr = new StringReader(xmlContent);
            try
            {
                return (PublicationMarketDocument)serializer.Deserialize(sr);
            }
            catch (Exception e)
            {
                // Log the exception details here
                return null;
            }
        }

        private string ExtractNamespaceVersion(string xmlContent)
        {
            // Use regex to extract the version number from the namespace
            var regex = new Regex(@"publicationdocument:(\d+:\d+)""");
            var match = regex.Match(xmlContent);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Return default version if no match found
            return "7:0";
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
                    var measurementIntervalMinutes = ts.Period.Resolution;
                    if (measurementIntervalMinutes != HOURLY_FREQ) {
                        continue;
                    }
                    List<Point> points = ts.Period.Point;
                    var country = record.Country;
                    
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

