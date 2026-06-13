using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using ReactiveUI;
using VAMedicalCallClient.Models;

namespace VAMedicalCallClient.ViewModels;

public class MedBrigadesViewModel : ReactiveObject, IRoutableViewModel, IModuleViewModel
{
    public string? UrlPathSegment => "med-brigades";
    public IScreen HostScreen { get; }
    public string ModuleTitle => "Бригады";
    public string ModuleSubTitle => "Мониторинг статуса и загрузки бригад СМП";

    // Полный список бригад, загруженный из источника (Файл/API)
    private List<MedicalBrigade> _allSourceBrigades = new();

    // Отфильтрованная коллекция, к которой привязан DataGrid
    private ObservableCollection<MedicalBrigade> _brigades = new();
    public ObservableCollection<MedicalBrigade> Brigades
    {
        get => _brigades;
        set => this.RaiseAndSetIfChanged(ref _brigades, value);
    }

    private MedicalBrigade? _selectedBrigade;
    public MedicalBrigade? SelectedBrigade
    {
        get => _selectedBrigade;
        set => this.RaiseAndSetIfChanged(ref _selectedBrigade, value);
    }

    // ================= СЧЕТЧИКИ ДЛЯ ТАЙЛОВ СТАТУСОВ (DELPHI) =================
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


    // ================= КОМАНДЫ ФИЛЬТРАЦИИ И УПРАВЛЕНИЯ =================
    public ReactiveCommand<Unit, Unit> FilterAllCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterAvailableCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterConfirmingCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterArrivedCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterWorkingCommand { get; }
    public ReactiveCommand<Unit, Unit> FilterNoConnectionCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDetailsCommand { get; }


    // ================= КОНСТРУКТОР =================
    public MedBrigadesViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;

        // Привязка команд фильтрации к общему методу с передачей нужного статуса
        FilterAllCommand = ReactiveCommand.Create(() => ApplyFilter(null));
        FilterAvailableCommand = ReactiveCommand.Create(() => ApplyFilter(BrigadeStatus.Available));
        FilterConfirmingCommand = ReactiveCommand.Create(() => ApplyFilter(BrigadeStatus.Confirming));
        FilterArrivedCommand = ReactiveCommand.Create(() => ApplyFilter(BrigadeStatus.Arrived));
        FilterWorkingCommand = ReactiveCommand.Create(() => ApplyFilter(BrigadeStatus.Working));
        FilterNoConnectionCommand = ReactiveCommand.Create(() => ApplyFilter(BrigadeStatus.NoConnection));

        // Асинхронные команды управления
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);
        OpenDetailsCommand = ReactiveCommand.Create(() => { /* Логика открытия карточки */ });

        // Первичный запуск загрузки данных
        _ = LoadDataAsync();
    }


    // ================= МЕТОДЫ ЛОГИКИ И АНАЛИЗА ДАННЫХ =================

    /// <summary>
    /// Асинхронная загрузка данных (из локального файла с заготовкой под API)
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            string jsonRawData = string.Empty;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "brigades.json");

            if (File.Exists(filePath))
            {
                jsonRawData = await File.ReadAllTextAsync(filePath);
            }
            else
            {
                jsonRawData = "[]";
            }

            // РЕЗЕРВ ДЛЯ СЕРВЕРА: jsonRawData = await _apiClient.GetBrigadesJsonAsync();

            var parsedBrigades = JsonSerializer.Deserialize<MedicalBrigade[]>(jsonRawData);

            if (parsedBrigades != null)
            {
                // Сохраняем эталонный неизменяемый список для фильтрации
                _allSourceBrigades = parsedBrigades.ToList();

                // Рассчитываем глобальные счетчики на основе всех данных
                RecalculateCounters();

                // По умолчанию показываем все бригады
                ApplyFilter(null);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки бригад: {ex.Message}");
            _allSourceBrigades.Clear();
            Brigades = new ObservableCollection<MedicalBrigade>();
            ResetCounters();
        }
    }

    /// <summary>
    /// Применение фильтра к таблице на основе выбранного статуса
    /// </summary>
    private void ApplyFilter(BrigadeStatus? targetStatus)
    {
        if (targetStatus == null)
        {            
            Brigades = new ObservableCollection<MedicalBrigade>(_allSourceBrigades);
        }
        else
        {         
            var filtered = _allSourceBrigades.Where(b => b.Status == targetStatus);
            Brigades = new ObservableCollection<MedicalBrigade>(filtered);
        }
    }

    /// <summary>
    /// Пересчет индикаторов-тайлов 
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
