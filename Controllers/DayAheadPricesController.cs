using Microsoft.AspNetCore.Mvc;
using nigo.Models;
using nigo.Utility;
using System.Xml.Serialization;

namespace nigo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DayAheadPricesController : ControllerBase
    {
        private readonly String _token = "10eaf78f-5db9-4d0f-aba9-604485bc646e";
        //private readonly ILogger<DayAheadPricesController> _logger;

        public DayAheadPricesController()
        {
           
        }



        [HttpPost(Name = "PostEnergyData")]
        public async Task<XmlDocument> PostAsync(EnergyData energy)
        {
            HttpClient client = new HttpClient();
       /*     if(energy.PeriodStart > energy.PeriodEnd) // if EndDate is earlier then StartDate TODO check api response if this occurs
            {
                return null;
            }*/

            if (Constants.countryDomains.TryGetValue(energy.InDomain,out string inDomain))
            {

                if (Constants.countryDomains.TryGetValue(energy.OutDomain, out string outDomain))
                {
                    string webUrl = Constants.apiUrl + Constants.documentTypeParam + "=" + energy.DocumentType + "&" + Constants.inDomainParam + "=" + inDomain + "&" + Constants.outDomainParam + "=" + outDomain + "&" + Constants.periodStartParam + "=" + energy.PeriodStart + "&" + Constants.periodEndParam + "=" + energy.PeriodEnd + "&" + Constants.securityTokenParam + "=" + _token;
                    
                    using HttpResponseMessage response = await client.GetAsync(webUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    XmlSerializer serializer = new XmlSerializer(typeof(PublicationMarketDocument));

                    PublicationMarketDocument document;

                    serializer.UnknownNode += new XmlNodeEventHandler(XmlHelper.serializerUnknownNode);
                    serializer.UnknownAttribute += new XmlAttributeEventHandler(XmlHelper.serializerUnknownAttribute);

                    using (TextReader sr = new StringReader(responseBody))
                    {
                        document = (PublicationMarketDocument)serializer.Deserialize(sr);
                    }

                    return document;
                }

                ErrorTypeDocument outDomainError = generateDomainErrorDocument(energy.OutDomain);
                return outDomainError;

            }
            else
            {
                ErrorTypeDocument inDomainError = generateDomainErrorDocument(energy.InDomain);
                return inDomainError;
            }
            
        }

        private static ErrorTypeDocument generateDomainErrorDocument(string domain)
        {
            Console.WriteLine($"There is error using {domain} Country value");
            ErrorTypeDocument domainError = new ErrorTypeDocument
            {
                Error = "Bad request",
                Message = $"There is error using {domain} Country value"
            };

            XmlSerializer errorSerializer = new XmlSerializer(typeof(ErrorTypeDocument));
            StringWriter sw = new StringWriter();
            errorSerializer.Serialize(sw, domainError);
            return domainError;
        }

        [HttpGet(Name = "GetAllDayAheadEnergyData")]
        public async Task<List<PublicationMarketDocument>> GetAsync()
        {
            HttpClient client = new HttpClient();
            List<PublicationMarketDocument> document = new List<PublicationMarketDocument>();

            foreach (var country in Constants.countryDomains.Values){
                Console.WriteLine(country);
                
                var date = DateTime.Today.AddDays(-3);
                string dateDateAhead = date.Year.ToString() + DateTime.Today.AddDays(-3).ToString("MM") + DateTime.Today.AddDays(-3).ToString("dd") + "14" + "00";

                string webUrl = Constants.apiUrl + Constants.documentTypeParam + "=" + Constants.dayAheadCode + "&" + Constants.inDomainParam + "=" + country + "&" + Constants.outDomainParam + "=" + country + "&" + Constants.periodStartParam + "=" + dateDateAhead + "&" + Constants.periodEndParam + "=" + dateDateAhead + "&" + Constants.securityTokenParam + "=" + _token;

                Console.WriteLine(webUrl);
                
                using HttpResponseMessage response = await client.GetAsync(webUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                XmlSerializer serializer = new XmlSerializer(typeof(PublicationMarketDocument));

                PublicationMarketDocument d;

                serializer.UnknownNode += new XmlNodeEventHandler(XmlHelper.serializerUnknownNode);
                serializer.UnknownAttribute += new XmlAttributeEventHandler(XmlHelper.serializerUnknownAttribute);

                try
                {
                    using (TextReader sr = new StringReader(responseBody))
                    {
                        d = (PublicationMarketDocument)serializer.Deserialize(sr);
                    }
                    document.Add(d);
                    break;
                }
                catch (Exception e){
                    Console.WriteLine($"Error for country code {country}");
                    Console.WriteLine(e.Message);
                }

            }

            return document;

        }

    }
}


public abstract class XmlHelper
{
    static public void serializerUnknownNode
    (object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

    static public void serializerUnknownAttribute
    (object sender, XmlAttributeEventArgs e)
    {
        System.Xml.XmlAttribute attr = e.Attr;
        Console.WriteLine("Unknown attribute " +
        attr.Name + "='" + attr.Value + "'");
    }
}