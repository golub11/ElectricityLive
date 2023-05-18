using CsvHelper;
using nigo.Models;
using System.Text.Json;

namespace nigo.Utility
{
    public class FileWriter<T>
    {
        private readonly string _filePath;

        public FileWriter(string filePath)
        {
            _filePath = filePath;
        }

        public async void WriteList(List<T> data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            using var writer = new StreamWriter(_filePath);
            await writer.WriteAsync(jsonString);
        }

        public async void Write(T data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            using var writer = new StreamWriter(_filePath);
            await writer.WriteAsync(jsonString);
        }

        public async void WriteCsv(List<PublicationMarketDocument> data)
        {
            double avgDailyCountryPrice;
            List<CsvModel> csvData= new List<CsvModel>();
            foreach (var d in data)
            {
                avgDailyCountryPrice = d.getAvgPriceAmount();
                CsvModel csvRow = new CsvModel(country: d.Country, price: avgDailyCountryPrice);
                csvData.Add(csvRow);
            }

            using var writer = new StreamWriter(_filePath);
            using var csvWriter = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            csvWriter.WriteRecords(csvData);
        }
    }
}
