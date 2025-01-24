using System.Text.Json.Serialization;

namespace nigo.Controllers
{
    public class EnergyData
    {
        public string InDomain { get; set; } // represent the name of the country. Map the name to the domain code from CountryDomain class
        public string OutDomain { get; set; } // represent the name of the country. Map the name to the domain code from CountryDomain class
        public string PeriodStart { get; set; }
        public string PeriodEnd { get; set; }


    }
    public class CountryRequestModel
    {
        public string country { get; set; }
        public string date_start { get; set; }
        public string date_end { get; set; }

    }

    public class TimeForAPI
    {
        public string date_start { get; set; }
        public string date_end { get; set; }


    }

    public class BotMessage
    {
        public string session_id { get; set; }
        public string input { get; set; }


    }


}