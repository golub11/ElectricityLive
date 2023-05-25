namespace nigo.Controllers
{
    public class EnergyData
    {
        public string InDomain { get; set; } // represent the name of the country. Map the name to the domain code from CountryDomain class
        public string OutDomain { get; set; } // represent the name of the country. Map the name to the domain code from CountryDomain class
        public string PeriodStart { get; set; }
        public string PeriodEnd { get; set; }


    }

}