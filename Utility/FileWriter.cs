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
    }
}
