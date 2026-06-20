using System.Collections.Generic;
using System.Threading.Tasks;
using VAMedicalCallClient.Models;

namespace VAMedicalCallClient.Services;

public interface IMedicalBrigadeDataService
{
    /// <summary>
    /// Получает список бригад из источника (файл, API, БД)
    /// </summary>
    Task<List<MedicalBrigade>> GetBrigadesAsync();
}