using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VAMedicalCallClient.Models;

namespace VAMedicalCallClient.Services;

public class MedicalBrigadeDataService : IMedicalBrigadeDataService
{
    public async Task<List<MedicalBrigade>> GetBrigadesAsync()
    {
        string jsonRawData = string.Empty;
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "brigades.json");

        if (File.Exists(filePath))
        {
            jsonRawData = await File.ReadAllTextAsync(filePath);
        }
        else
        {
            jsonRawData = "[]";
        }

        // To-DO данные по Rest API от СЕРВЕРА: 
        // jsonRawData = await _httpClient.GetStringAsync("api/brigades");

        var parsedBrigades = JsonSerializer.Deserialize<MedicalBrigade[]>(jsonRawData);

        return parsedBrigades?.ToList() ?? new List<MedicalBrigade>();
    }
}