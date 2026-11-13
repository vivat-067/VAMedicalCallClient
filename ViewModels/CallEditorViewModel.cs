// Базовые типы (.NET)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Реактивное программирование (System.Reactive)
using System.Reactive;
using System.Reactive.Linq;

// ReactiveUI 24+ и генераторы
using ReactiveUI;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

// Пространства имён проекта
using VAMedicalCallClient.Models;

namespace VAMedicalCallClient.ViewModels;

public partial class CallEditorViewModel : ViewModelBase
{
    public string WindowTitle { get; }
    public bool IsNewCall { get; }
    private readonly MedicalCall _sourceCall;

    // Списки для ComboBox
    public IEnumerable<CallType> CallTypes => Enum.GetValues<CallType>();
    public IEnumerable<CallStatus> CallStatuses => Enum.GetValues<CallStatus>();

    // Редактируемые поля
    [Reactive] public partial CallType TypeOfCall { get; set; }
    [Reactive] public partial CallStatus Status { get; set; }
    [Reactive] public partial int Number { get; set; }
    [Reactive] public partial string PatientName { get; set; } = string.Empty;
    [Reactive] public partial int? Age { get; set; }
    [Reactive] public partial string AddressStreet { get; set; } = string.Empty;
    [Reactive] public partial string? AddressDetails { get; set; }
    [Reactive] public partial string? ContactInfo { get; set; }
    [Reactive] public partial string? Complaints { get; set; }
    [Reactive] public partial string? Comment { get; set; }
    [Reactive] public partial string? BrigadeNumber { get; set; }
    [Reactive] public partial string PaymentType { get; set; } = "ОМС";
    [Reactive] public partial decimal? Cost { get; set; }

    // Команды и Interaction закрытия окна (возвращает сохраненный MedicalCall или null при отмене)
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public Interaction<MedicalCall?, Unit> CloseDialogInteraction { get; } = new();

    public CallEditorViewModel(MedicalCall? callToEdit = null)
    {
        IsNewCall = callToEdit == null;
        WindowTitle = IsNewCall ? "Новая заявка вызова СМП" : $"Редактирование заявки №{callToEdit!.Number}";

        _sourceCall = callToEdit ?? new MedicalCall
        {
            StatusId = (int)CallStatus.New,
            TypeOfCall = CallType.Ambulance,
            CallDate = DateTime.Today,
            ReceptionTime = DateTime.Now,
            PaymentType = "ОМС"
        };

        // Копируем данные в свойства формы
        TypeOfCall = _sourceCall.TypeOfCall;
        Status = (CallStatus)_sourceCall.StatusId;
        Number = _sourceCall.Number;
        PatientName = _sourceCall.PatientName;
        Age = _sourceCall.Age;
        AddressStreet = _sourceCall.AddressStreet;
        AddressDetails = _sourceCall.AddressDetails;
        ContactInfo = _sourceCall.ContactInfo;
        Complaints = _sourceCall.Complaints;
        Comment = _sourceCall.Comment;
        BrigadeNumber = _sourceCall.BrigadeNumber;
        PaymentType = string.IsNullOrWhiteSpace(_sourceCall.PaymentType) ? "ОМС" : _sourceCall.PaymentType;
        Cost = _sourceCall.Cost;

        // Валидация: ФИО и улица обязательны для сохранения
        var canSave = this.WhenAnyValueUnsafe(
            x => x.PatientName,
            x => x.AddressStreet,
            (patientName, addressStreet) =>
                !string.IsNullOrWhiteSpace(patientName) && !string.IsNullOrWhiteSpace(addressStreet));

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync, canSave);
        CancelCommand = ReactiveCommand.CreateFromTask(CancelAsync);
    }

    private async Task SaveAsync()
    {
        // Переносим данные из формы обратно в объект заявки
        _sourceCall.TypeOfCall = TypeOfCall;
        _sourceCall.StatusId = (int)Status;
        _sourceCall.Number = Number;
        _sourceCall.PatientName = PatientName.Trim();
        _sourceCall.Age = Age;
        _sourceCall.AddressStreet = AddressStreet.Trim();
        _sourceCall.AddressDetails = AddressDetails;
        _sourceCall.ContactInfo = ContactInfo;
        _sourceCall.Complaints = Complaints;
        _sourceCall.Comment = Comment;
        _sourceCall.BrigadeNumber = BrigadeNumber;
        _sourceCall.PaymentType = PaymentType;
        _sourceCall.Cost = Cost;

        await CloseDialogInteraction.Handle(_sourceCall);
    }

    private async Task CancelAsync()
    {
        await CloseDialogInteraction.Handle(null);
    }
}
