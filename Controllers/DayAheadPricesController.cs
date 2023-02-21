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
        private readonly ILogger<DayAheadPricesController> _logger;

        public DayAheadPricesController(ILogger<DayAheadPricesController> logger)
        {
            _logger = logger;
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

        [HttpGet(Name = "GetEnergyData")]
        public async Task<int> GetAsync()
        {
            return 1;
            
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