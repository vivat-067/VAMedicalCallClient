// Базовые типы и коллекции .NET
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using System.IO;
using System.Text.Json;

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

public partial class MedBrigadesViewModel : ReactiveObject, IRoutableViewModel, IModuleViewModel
{
    #region Metadata Properties
    public string? UrlPathSegment => "med-brigades";
    public IScreen HostScreen { get; }
    public string ModuleTitle => "Бригады";
    public string ModuleSubTitle => "Мониторинг статуса и загрузки бригад СМП";
    #endregion

    #region Fields
    private List<MedicalBrigade> _allSourceBrigades = new();
    #endregion

    #region Collections
    public ObservableCollection<MedicalBrigade> Brigades { get; } = new();
    #endregion

    #region Commands
    public ReactiveCommand<BrigadeStatus?, Unit> ChangeFilterCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDetailsCommand { get; }
    #endregion

    #region Properties
    [Reactive] public partial MedicalBrigade? SelectedBrigade { get; set; }
    [Reactive] public partial BrigadeStatus? SelectedStatusFilter { get; set; }

    [Reactive] public partial int CountAll { get; set; }  

    [Reactive] public partial int CountAvailable { get; set; }
    [Reactive] public partial int CountConfirming { get; set; }
    [Reactive] public partial int CountArrived { get; set; }
    [Reactive] public partial int CountWorking { get; set; }
    [Reactive] public partial int CountNoConnection { get; set; }
    #endregion


    public MedBrigadesViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;
        
        ChangeFilterCommand = ReactiveCommand.Create<BrigadeStatus?>(status => SelectedStatusFilter = status);
        
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);

        var isBrigadeSelected = this.WhenAnyValueUnsafe(x => x.SelectedBrigade)
          .Select(brigade => brigade != null);

        OpenDetailsCommand = ReactiveCommand.Create(() => { /* Логика открытия карточки */ }, isBrigadeSelected);

        //RefreshCommand.Execute().Subscribe();
        this.WhenAnyValueUnsafe(x => x.SelectedStatusFilter)
         .Skip(1)
         .ObserveOn(RxSchedulers.MainThreadScheduler)
         .Subscribe(ApplyFilter);

        Observable.Return(Unit.Default).InvokeCommand(RefreshCommand);
    }

    

    /// <summary>
    /// Асинхронная загрузка данных из JSON-файла 
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "brigades.json");
            string jsonRawData = File.Exists(filePath) ? await File.ReadAllTextAsync(filePath) : "[]";
            
            var parsedBrigades = JsonSerializer.Deserialize<List<MedicalBrigade>>(jsonRawData);

            _allSourceBrigades = parsedBrigades ?? new List<MedicalBrigade>();
            RecalculateCounters();
            ApplyFilter(SelectedStatusFilter);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки бригад: {ex.Message}");

               _allSourceBrigades.Clear();
                Brigades.Clear();
                ResetCounters();      
        }
    }

    /// <summary>
    /// Применение выбранного фильтра к отображаемой таблице
    /// </summary>
    private void ApplyFilter(BrigadeStatus? targetStatus)
    {        
            Brigades.Clear();

            var filtered = targetStatus == null
                ? _allSourceBrigades
                : _allSourceBrigades.Where(b => b.Status == targetStatus);

            foreach (var brigade in filtered)
            {
                Brigades.Add(brigade);
            }

        SelectedBrigade = Brigades.FirstOrDefault();

    }

    /// <summary>
    /// Пересчет индикаторов-тайлов на основе неизменяемого кэша данных
    /// </summary>
    private void RecalculateCounters()
    {
        CountAll = _allSourceBrigades.Count;
        CountAvailable = _allSourceBrigades.Count(b => b.Status == BrigadeStatus.Available);
        CountConfirming = _allSourceBrigades.Count(b => b.Status == BrigadeStatus.Confirming);
        CountArrived = _allSourceBrigades.Count(b => b.Status == BrigadeStatus.Arrived);
        CountWorking = _allSourceBrigades.Count(b => b.Status == BrigadeStatus.Working);
        CountNoConnection = _allSourceBrigades.Count(b => b.Status == BrigadeStatus.NoConnection);
    }

    private void ResetCounters()
    {
        CountAll = CountAvailable = CountConfirming = CountArrived = CountWorking = CountNoConnection = 0;
    }
}
