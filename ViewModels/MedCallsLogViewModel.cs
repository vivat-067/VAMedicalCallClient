using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading; // Используем Dispatcher.UIThread вместо AvaloniaScheduler для стабильности в 12.1
using VAMedicalCallClient.Models;
using VAMedicalCallClient.Services;

namespace VAMedicalCallClient.ViewModels;

public partial class MedCallsLogViewModel : ReactiveObject, IRoutableViewModel, IModuleViewModel
{
    public string? UrlPathSegment => "med-calls";
    public IScreen HostScreen { get; }

    public string ModuleTitle => "Заявки";
    public string ModuleSubTitle => "Регистрация заявок вызова СМП";

    private readonly MedicalCallApiService _apiService;

    // Эталонный архив данных для фильтрации
    private List<MedicalCall> _allSourceCalls = new();
    public ObservableCollection<MedicalCall> Calls { get; } = new();

    #region Commands
    public ReactiveCommand<Unit, List<MedicalCall>> LoadCallsCommand { get; }
    public ReactiveCommand<Unit, Unit> SendCallsCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateCallCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCallCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCallCommand { get; }

    public ReactiveCommand<Unit, Unit> PrintCallCardCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintAccompanyingSheetCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportToExcelCommand { get; }

    // Вместо 7 команд — ОДНА универсальная
    public ReactiveCommand<CallStatus?, Unit> ChangeFilterCommand { get; }
    #endregion

    #region Properties
    [Reactive] public partial MedicalCall? SelectedCall { get; set; }

    [Reactive] public partial int CountAll { get; set; }
    [Reactive] public partial int CountNew { get; set; }
    [Reactive] public partial int CountPending { get; set; }
    [Reactive] public partial int CountInWork { get; set; }
    [Reactive] public partial int CountCompleted { get; set; }
    [Reactive] public partial int CountInsurance { get; set; }
    [Reactive] public partial int CountCancelled { get; set; }

    [Reactive] public partial DateTime? FilterStartDate { get; set; } = DateTime.Today;
    [Reactive] public partial DateTime? FilterEndDate { get; set; } = DateTime.Today;

    // Хранит текущий выбранный фильтр (null — Все)
    [Reactive] public partial CallStatus? SelectedStatusFilter { get; set; }
    #endregion

    public MedCallsLogViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        _apiService = new MedicalCallApiService();

        // 1. Настройка универсальной команды фильтра
        ChangeFilterCommand = ReactiveCommand.Create<CallStatus?>(status => SelectedStatusFilter = status);

        // 2. Обработка загрузки данных
        LoadCallsCommand = ReactiveCommand.CreateFromTask(ExecuteLoadCalls);
        LoadCallsCommand.Subscribe(callsList =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _allSourceCalls = callsList ?? new List<MedicalCall>();
                RecalculateStatuses();
                ApplyFilter(SelectedStatusFilter);
            });
        });

        // 3. Отслеживание изменения фильтра через постоянную реактивную цепочку
        this.ObservableForProperty(x => x.SelectedStatusFilter)
            .Subscribe(change => ApplyFilter(change.Value));

        // Условие активации кнопок управления (реактивное свойство)
        var isCallSelected = this.WhenAnyValue(x => x.SelectedCall)
                         .Select(call => call != null);


        // 4. Инициализация базовых команд
        SendCallsCommand = ReactiveCommand.Create(() => { });
        CreateCallCommand = ReactiveCommand.Create(() => { });
        EditCallCommand = ReactiveCommand.Create(() => { }, isCallSelected);

        DeleteCallCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedCall != null)
            {
                // Удаляем из эталонного списка и обновляем UI
                _allSourceCalls.Remove(SelectedCall);
                RecalculateStatuses();
                ApplyFilter(SelectedStatusFilter);
            }
        }, isCallSelected);

        PrintCallCardCommand = ReactiveCommand.Create(() => { }, isCallSelected);
        PrintAccompanyingSheetCommand = ReactiveCommand.Create(() => { }, isCallSelected);
        ExportToExcelCommand = ReactiveCommand.Create(() => { }, isCallSelected);

        // Автоматический первичный запуск загрузки данных
        LoadCallsCommand.Execute().Subscribe();
    }

    private void ApplyFilter(CallStatus? targetStatus)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Calls.Clear();

            var filtered = targetStatus == null
                ? _allSourceCalls
                : _allSourceCalls.Where(c => c.StatusId == (int)targetStatus);

            foreach (var call in filtered)
            {
                Calls.Add(call);
            }

            SelectedCall = Calls.FirstOrDefault();
        });
    }

    private void RecalculateStatuses()
    {
        // Расчет ведем строго по эталонному кэшу _allSourceCalls, а не по UI-коллекции!
        CountAll = _allSourceCalls.Count;
        CountNew = _allSourceCalls.Count(c => c.StatusId == (int)CallStatus.New);
        CountPending = _allSourceCalls.Count(c => c.StatusId == (int)CallStatus.csPending);
        CountInWork = _allSourceCalls.Count(c => c.StatusId == (int)CallStatus.csInWork);
        CountCompleted = _allSourceCalls.Count(c => c.StatusId == (int)CallStatus.csCompleted);
        CountInsurance = _allSourceCalls.Count(c => c.StatusId == (int)CallStatus.csFromInsurance);
        CountCancelled = _allSourceCalls.Count(c => c.StatusId == (int)CallStatus.csCancelled);
    }

    private async Task<List<MedicalCall>> ExecuteLoadCalls()
    {
        return await _apiService.GetCallsAsync();
    }
}
