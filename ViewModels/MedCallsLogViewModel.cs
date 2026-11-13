// Базовые типы и коллекции .NET
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

// Реактивное программирование (System.Reactive)
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

// ReactiveUI 24+ и генераторы исходного кода
using ReactiveUI;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

// пространства имён проекта
using VAMedicalCallClient.Models;
using VAMedicalCallClient.Services;


namespace VAMedicalCallClient.ViewModels;


public partial class MedCallsLogViewModel : ReactiveObject, IRoutableViewModel, IModuleViewModel
{
    #region Metadata Properties
    public string? UrlPathSegment => "med-calls";
    public IScreen HostScreen { get; }
    public string ModuleTitle => "Заявки";
    public string ModuleSubTitle => "Регистрация заявок вызова СМП";
    #endregion

    #region Fields
    private readonly MedicalCallApiService _apiService;
    private List<MedicalCall> _allSourceCalls = new();
    #endregion

    #region Collections
    public ObservableCollection<MedicalCall> Calls { get; } = new();
    #endregion

    #region Commands
    public ReactiveCommand<Unit, List<MedicalCall>> LoadCallsCommand { get; }
    public ReactiveCommand<Unit, Unit> SendCallsCommand { get; }
    public ReactiveCommand<Unit, Unit> CreateCallCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCallCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCallCommand { get; }

    public ReactiveCommand<Unit, Unit> PrintCallCardCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintAccompanyingSheetCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportToExcelCommand { get; }

    public ReactiveCommand<CallStatus?, Unit> ChangeFilterCommand { get; }
    #endregion

    #region Properties
    [Reactive] public partial MedicalCall? SelectedCall { get; set; }
    [Reactive] public partial CallStatus? SelectedStatusFilter { get; set; }

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

        ChangeFilterCommand = ReactiveCommand.Create<CallStatus?>(status => SelectedStatusFilter = status);
        LoadCallsCommand = ReactiveCommand.CreateFromTask(ExecuteLoadCalls);

        LoadCallsCommand
          .ObserveOn(RxSchedulers.MainThreadScheduler)
          .Subscribe(callsList =>
          {
              _allSourceCalls = callsList ?? [];
              RecalculateStatuses();
              ApplyFilter(SelectedStatusFilter);
          });
        
        this.WhenAnyValueUnsafe(x => x.SelectedStatusFilter)
            .Skip(1)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(ApplyFilter);


        // Условие активации кнопок управления 
        var isCallSelected = this.WhenAnyValueUnsafe(x => x.SelectedCall)
                         .Select(call => call != null);

        SendCallsCommand = ReactiveCommand.Create(() => { });
        CreateCallCommand = ReactiveCommand.Create(() => { });
        EditCallCommand = ReactiveCommand.CreateFromTask(ExecuteEditCall, isCallSelected);

        DeleteCallCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedCall != null)
            {
                _allSourceCalls.Remove(SelectedCall);
                RecalculateStatuses();
                ApplyFilter(SelectedStatusFilter);
            }
        }, isCallSelected);

        PrintCallCardCommand = ReactiveCommand.Create(() => { }, isCallSelected);
        PrintAccompanyingSheetCommand = ReactiveCommand.Create(() => { }, isCallSelected);
        ExportToExcelCommand = ReactiveCommand.Create(() => { }, isCallSelected);

        // Автоматический первичный запуск загрузки данных        
        LoadCallsCommand.Execute(Unit.Default).Subscribe();

    }


    private void ApplyFilter(CallStatus? targetStatus)
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

    }

    private void RecalculateStatuses()
    {
        // Расчет по эталонному кэшу _allSourceCalls
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


    private async Task ExecuteEditCall()
    {
        if (SelectedCall == null) return;

        // TODO: 
        // var dialogResult = await _dialogService.ShowCallEditorDialog(SelectedCall);
        // if (dialogResult == DialogResult.Ok) { ... }
        //        await Task.CompletedTask;}}
    }

}