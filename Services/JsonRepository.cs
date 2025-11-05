using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ElectronicsInventoryApp.Services
{
    public class JsonRepository<T>
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        public JsonRepository(string filePath)
        {
            _filePath = filePath;
            EnsureFile();
        }

        private void EnsureFile()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public List<T> Load()
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        public void Save(List<T> data)
        {
            string json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(_filePath, json);
        }
    }
}
