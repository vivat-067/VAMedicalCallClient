using Avalonia.Threading;
using ReactiveUI;
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

public class MedBrigadesViewModel : ReactiveObject, IRoutableViewModel, IModuleViewModel
{
    public string? UrlPathSegment => "med-brigades";
    public IScreen HostScreen { get; }
    public string ModuleTitle => "Бригады";
    public string ModuleSubTitle => "Мониторинг статуса и загрузки бригад СМП";

    // Полный список бригад, загруженный из источника (выступает в роли кэша)
    private List<MedicalBrigade> _allSourceBrigades = new();

    // Коллекция, к которой привязана сетка DataGrid в UI
    public ObservableCollection<MedicalBrigade> Brigades { get; } = new();

    private MedicalBrigade? _selectedBrigade;
    public MedicalBrigade? SelectedBrigade
    {
        get => _selectedBrigade;
        set => this.RaiseAndSetIfChanged(ref _selectedBrigade, value);
    }

    private BrigadeStatus? _selectedStatusFilter;
    public BrigadeStatus? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set => this.RaiseAndSetIfChanged(ref _selectedStatusFilter, value);
    }

    // ================= СЧЕТЧИКИ ДЛЯ ТАЙЛОВ СТАТУСОВ (СМП) =================
    private int _countAll;
    public int CountAll { get => _countAll; private set => this.RaiseAndSetIfChanged(ref _countAll, value); }

    private int _countAvailable;
    public int CountAvailable { get => _countAvailable; private set => this.RaiseAndSetIfChanged(ref _countAvailable, value); }

    private int _countConfirming;
    public int CountConfirming { get => _countConfirming; private set => this.RaiseAndSetIfChanged(ref _countConfirming, value); }

    private int _countArrived;
    public int CountArrived { get => _countArrived; private set => this.RaiseAndSetIfChanged(ref _countArrived, value); }

    private int _countWorking;
    public int CountWorking { get => _countWorking; private set => this.RaiseAndSetIfChanged(ref _countWorking, value); }

    private int _countNoConnection;
    public int CountNoConnection { get => _countNoConnection; private set => this.RaiseAndSetIfChanged(ref _countNoConnection, value); }


    // ================= КОМАНДЫ УПРАВЛЕНИЯ И ФИЛЬТРАЦИИ =================
    public ReactiveCommand<BrigadeStatus?, Unit> ChangeFilterCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDetailsCommand { get; }


    // ================= КОНСТРУКТОР =================
    public MedBrigadesViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;

        // Команда изменения фильтра 
        ChangeFilterCommand = ReactiveCommand.Create<BrigadeStatus?>(status => SelectedStatusFilter = status);

        // Асинхронные команды управления
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
        OpenDetailsCommand = ReactiveCommand.Create(() => { /* Логика открытия карточки */ });
        
        RefreshCommand.Execute().Subscribe();
        
        this.ObservableForProperty(x => x.SelectedStatusFilter)
            .Subscribe(change => ApplyFilter(change.Value));
    }


    // ================= МЕТОДЫ ЛОГИКИ И АНАЛИЗА ДАННЫХ =================

    /// <summary>
    /// Асинхронная загрузка данных из JSON-файла конфигурации
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "brigades.json");
            string jsonRawData = File.Exists(filePath) ? await File.ReadAllTextAsync(filePath) : "[]";

            var parsedBrigades = JsonSerializer.Deserialize<MedicalBrigade[]>(jsonRawData);

            // Перенаправляем обновление списков и счетчиков строго в UI-поток Avalonia
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
        // Выполняем очистку и наполнение коллекции внутри потока отрисовки интерфейса
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
