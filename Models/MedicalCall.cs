using System;
using System.Text.Json.Serialization;

namespace VAMedicalCallClient.Models;

public enum CallType { DoctorAtHome, Ambulance }

public enum CallStatus
{
    New = 1, csPending = 2, csFromInsurance = 3,
    csInWork = 4, csCancelled = 5, csCompleted = 6
}

public class MedicalCall
{
    public int Id { get; set; }
    public int StatusId { get; set; }

    
    [JsonIgnore]
    public string Status => (CallStatus)StatusId switch
    {
        CallStatus.New => "Новая заявка",
        CallStatus.csPending => "На согласовании",
        CallStatus.csFromInsurance => "Заявка от страховой",
        CallStatus.csInWork => "Взята в работу бригадой",
        CallStatus.csCancelled => "Отменена",
        CallStatus.csCompleted => "Завершена",
        _ => $"Статус №{StatusId}"
    };

    public CallType TypeOfCall { get; set; }
    public int Number { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string AddressStreet { get; set; } = string.Empty;
    public string? AddressDetails { get; set; }
    public string? ContactInfo { get; set; }

    public string? Complaints { get; set; }
    public string? Comment { get; set; }
    public string? Diagnosis { get; set; }
    public string? Conclusion { get; set; }
    public string? Note { get; set; }

    public DateTime? CallDate { get; set; }
    public DateTime? ReceptionTime { get; set; }
    public DateTime? TransferTime { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public DateTime? CompletionTime { get; set; }

    
    [JsonIgnore]
    public TimeSpan? WorkDuration { get; set; }

    public string? BrigadeNumber { get; set; }
    public string? Doctor { get; set; }
    public string? Paramedic { get; set; }
    public string? Driver { get; set; }
    public string? Dispatcher1 { get; set; }
    public string? Dispatcher2 { get; set; }

    public string PaymentType { get; set; } = string.Empty;
    public string? InsuranceNumber { get; set; }
    public string? Customer { get; set; }
    public string? CustomerRepresentative { get; set; }
    public decimal? Cost { get; set; }
    public int? MKADDistance { get; set; }
    public bool IsWaiting { get; set; }
}


