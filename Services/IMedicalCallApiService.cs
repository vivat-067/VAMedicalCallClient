using System.Collections.Generic;
using System.Threading.Tasks;
using VAMedicalCallClient.Models;

namespace VAMedicalCallClient.Services;

public interface IMedicalCallApiService
{
    /// <summary>
    /// Получить список всех заявок (GET)
    /// </summary>
    Task<List<MedicalCall>> GetCallsAsync();

    /// <summary>
    /// Создать новую заявку вызова СМП (POST)
    /// </summary>
    /// <param name="newCall">Объект с заполненными данными пациента</param>
    /// <returns>Созданный объект MedicalCall с присвоенным сервером ID или null при ошибке</returns>
    Task<MedicalCall?> CreateCallAsync(MedicalCall newCall);

    /// <summary>
    /// Удалить заявку вызова по ID (DELETE)
    /// </summary>
    Task<bool> DeleteCallAsync(int id);
}