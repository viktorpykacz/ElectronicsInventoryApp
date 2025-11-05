using ElectronicsInventoryApp.Models;
using System.IO;
using System.Text.Json;
public static class DataInit
{
    public static void EnsureDataFiles(string dataDir)
    {
        Directory.CreateDirectory(dataDir);

        void Ensure<T>(string fileName, T sample)
        {
            var path = Path.Combine(dataDir, fileName);
            if (!File.Exists(path))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(path, JsonSerializer.Serialize(sample, options));
            }
        }
    }
}