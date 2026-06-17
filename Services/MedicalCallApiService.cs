using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using VAMedicalCallClient.Models;

namespace VAMedicalCallClient.Services;

public class MedicalCallApiService : IMedicalCallApiService
{
    private readonly HttpClient _httpClient;

    private const string ApiBaseUrl = "http://localhost:5285";
    private const string ApiEndpointCalls = "/api/MedicalAssistanceCalls";

    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public MedicalCallApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Получить список всех заявок (GET)
    /// </summary>
    public async Task<List<MedicalCall>> GetCallsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<MedicalCall>>(ApiEndpointCalls, _jsonOptions);
            return response ?? new List<MedicalCall>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка API (GET): {ex.Message}");
            return new List<MedicalCall>();
        }
    }

    /// <summary>
    /// Создать новую заявку вызова СМП (POST)
    /// </summary>
    /// <param name="newCall">Объект с заполненными данными пациента</param>
    /// <returns>Созданный объект MedicalCall с присвоенным сервером ID или null при ошибке</returns>
    public async Task<MedicalCall?> CreateCallAsync(MedicalCall newCall)
    {
        try
        {            
            var response = await _httpClient.PostAsJsonAsync(ApiEndpointCalls, newCall, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MedicalCall>(_jsonOptions);
            }

            System.Diagnostics.Debug.WriteLine($"Ошибка API (POST): Сервер вернул код {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка API (POST): {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Удалить заявку вызова по ID (DELETE)
    /// </summary>    
    public async Task<bool> DeleteCallAsync(int id)
    {
        try        {
            
            string requestUrl = $"{ApiEndpointCalls}/{id}";

            var response = await _httpClient.DeleteAsync(requestUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка API (DELETE): {ex.Message}");
            return false;
        }
    }
}
