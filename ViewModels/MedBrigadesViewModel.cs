using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;



using VAMedicalCallClient.Models;

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
        OpenDetailsCommand = ReactiveCommand.Create(() => { /* Логика открытия карточки */ });

        RefreshCommand.Execute().Subscribe();

        this.ObservableForProperty(x => x.SelectedStatusFilter)
            .Subscribe(change => ApplyFilter(change.Value));
    }

    

    /// <summary>
    /// Асинхронная загрузка данных из JSON-файла 
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "brigades.json");
            string jsonRawData = File.Exists(filePath) ? await File.ReadAllTextAsync(filePath) : "[]";

            var parsedBrigades = JsonSerializer.Deserialize<MedicalBrigade[]>(jsonRawData);

            Dispatcher.UIThread.Post(() =>
            {
                _allSourceBrigades = parsedBrigades?.ToList() ?? new List<MedicalBrigade>();
                RecalculateCounters();
                ApplyFilter(SelectedStatusFilter);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки бригад: {ex.Message}");

            Dispatcher.UIThread.Post(() =>
            {
                _allSourceBrigades.Clear();
                Brigades.Clear();
                ResetCounters();
            });
        }
    }

    /// <summary>
    /// Применение выбранного фильтра к отображаемой таблице
    /// </summary>
    private void ApplyFilter(BrigadeStatus? targetStatus)
    {        
        Dispatcher.UIThread.Post(() =>
        {
            Brigades.Clear();

            var filtered = targetStatus == null
                ? _allSourceBrigades
                : _allSourceBrigades.Where(b => b.Status == targetStatus);

            foreach (var brigade in filtered)
            {
                Brigades.Add(brigade);
            }
        });
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
