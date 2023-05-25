namespace nigo.Models
{
    public class CsvModel
    {
        public string country { get; set; }
        public double? price { get; set; }

        public CsvModel(string country, double? price)
        {
            this.country = country;
            this.price = price;
        }
    }
}
