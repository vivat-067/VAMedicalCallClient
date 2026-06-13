using System;
using System.Text.Json.Serialization;

namespace VAMedicalCallClient.Models;

public class MedicalBrigade
{
    private static readonly string[] BrigadeStatusStrings =
        { "Свободна", "Подтверждение вызова", "Прибыла", "В работе", "Без связи" };

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("status")]
    public int StatusValue { get; set; }

    [JsonPropertyName("brigadeNumber")]
    public string BrigadeNumber { get; set; } = string.Empty;

    [JsonPropertyName("doctor")]
    public string Doctor { get; set; } = string.Empty;

    [JsonPropertyName("paramedic")]
    public string Paramedic { get; set; } = string.Empty;

    [JsonPropertyName("driver")]
    public string Driver { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("doctorPhotoPath")]
    public string? DoctorPhotoPath { get; set; }

    [JsonPropertyName("commPhone1")]
    public string CommPhone1 { get; set; } = string.Empty;

    [JsonPropertyName("commPhone2")]
    public string CommPhone2 { get; set; } = string.Empty;

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    // ================= ВЫЧИСЛЯЕМЫЕ UI-СВОЙСТВА =================

    /// <summary>
    /// Строго типизированное перечисление статуса 
    /// </summary>
    [JsonIgnore]
    public BrigadeStatus Status
    {
        get => Enum.IsDefined(typeof(BrigadeStatus), StatusValue) ? (BrigadeStatus)StatusValue : BrigadeStatus.Available;
        set => StatusValue = (int)value;
    }

    /// <summary>
    /// Текстовое описание статуса 
    /// </summary>
    [JsonIgnore]
    public string StatusName
    {
        get
        {
            int index = StatusValue - 1;
            return (index >= 0 && index < BrigadeStatusStrings.Length)
                ? BrigadeStatusStrings[index]
                : $"Статус №{StatusValue}";
        }
    }

    /// <summary>
    /// Конструктор по умолчанию 
    /// </summary>
    public MedicalBrigade()
    {
        Id = 0;
        Status = BrigadeStatus.Available;
        Lat = 0.0;
        Lon = 0.0;
    }
}
