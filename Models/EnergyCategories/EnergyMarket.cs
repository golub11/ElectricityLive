namespace nigo.Models
{
    public class EnergyMarket : EnergyCategory

    {
        private String? countryName;
        
        public DayWithPrices[]? prices;
        public TimeInterval timeInterval;

    }
}