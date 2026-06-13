using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

    public ReactiveCommand<Unit, Unit> FilterAllCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterNewCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterPendingCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterInWorkCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterCompletedCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterInsuranceCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterCancelledCommand { get; }
    #endregion

    #region Properties
    [Reactive]
    public partial MedicalCall? SelectedCall { get; set; }

    // Реактивные свойства счетчиков для привязки к тайлам в XAML
    [Reactive] public partial int CountAll { get; set; }
    [Reactive] public partial int CountNew { get; set; }
    [Reactive] public partial int CountPending { get; set; }
    [Reactive] public partial int CountInWork { get; set; }
    [Reactive] public partial int CountCompleted { get; set; }
    [Reactive] public partial int CountInsurance { get; set; }
    [Reactive] public partial int CountCancelled { get; set; }

    
    [Reactive] public partial DateTime? FilterStartDate { get; set; } = DateTime.Today;
    [Reactive] public partial DateTime? FilterEndDate { get; set; } = DateTime.Today;
    #endregion

    public MedCallsLogViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        _apiService = new MedicalCallApiService();

        
        LoadCallsCommand = ReactiveCommand.CreateFromTask(ExecuteLoadCalls);
        LoadCallsCommand.Subscribe(callsList =>
        {
            AvaloniaScheduler.Instance.Schedule(_ =>
            {
                Calls.Clear();
                if (callsList != null)
                {
                    foreach (var call in callsList)
                    {
                        Calls.Add(call);
                    }

                    if (Calls.Count > 0)
                    {
                        SelectedCall = Calls[0];
                    }
                    else
                    {
                        SelectedCall = null;
                    }
                }
            });
        });

        // Отслеживаем изменения в коллекции Calls для обновления счетчиков 
        Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
            x => Calls.CollectionChanged += x,
            x => Calls.CollectionChanged -= x)
            .Subscribe(_ => RecalculateStatuses());

        // Условие активации кнопок управления
        IObservable<bool> isCallSelected = this.WhenAnyValue(x => x.SelectedCall)
                                              .Select(call => call != null);

        // Инициализация базовых команд
        SendCallsCommand = ReactiveCommand.Create(() => { });
        CreateCallCommand = ReactiveCommand.Create(() => { });
        EditCallCommand = ReactiveCommand.Create(() => { }, isCallSelected);

        DeleteCallCommand = ReactiveCommand.Create(() =>
        {
            AvaloniaScheduler.Instance.Schedule(_ =>
            {
                if (SelectedCall != null)
                {
                    Calls.Remove(SelectedCall);
                    SelectedCall = null;
                }
            });
        }, isCallSelected);

        PrintCallCardCommand = ReactiveCommand.Create(() => { }, isCallSelected);
        PrintAccompanyingSheetCommand = ReactiveCommand.Create(() => { }, isCallSelected);
        ExportToExcelCommand = ReactiveCommand.Create(() => { }, isCallSelected);

        // заглушки для фильтров-тайлов
        FilterAllCommand = ReactiveCommand.Create(() => { });
        FilterNewCommand = ReactiveCommand.Create(() => { });
        FilterPendingCommand = ReactiveCommand.Create(() => { });
        FilterInWorkCommand = ReactiveCommand.Create(() => { });
        FilterCompletedCommand = ReactiveCommand.Create(() => { });
        FilterInsuranceCommand = ReactiveCommand.Create(() => { });
        FilterCancelledCommand = ReactiveCommand.Create(() => { });
    }

    private void RecalculateStatuses()
    {
        AvaloniaScheduler.Instance.Schedule(_ =>
        {
            CountAll = Calls.Count;
            CountNew = Calls.Count(c => c.StatusId == (int)CallStatus.New);
            CountPending = Calls.Count(c => c.StatusId == (int)CallStatus.csPending);
            CountInWork = Calls.Count(c => c.StatusId == (int)CallStatus.csInWork);
            CountCompleted = Calls.Count(c => c.StatusId == (int)CallStatus.csCompleted);
            CountInsurance = Calls.Count(c => c.StatusId == (int)CallStatus.csFromInsurance);
            CountCancelled = Calls.Count(c => c.StatusId == (int)CallStatus.csCancelled);
        });
    }

    private async Task<List<MedicalCall>> ExecuteLoadCalls()
    {
        return await _apiService.GetCallsAsync();
    }
}
