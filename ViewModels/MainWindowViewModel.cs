// Базовые типы (.NET)
using System;                             // Метод расширения .Subscribe()

// Реактивное программирование (System.Reactive)
using System.Reactive;                    // Тип Unit для команд и Interaction
using System.Reactive.Linq;               // Операторы Select и Handle

// ReactiveUI 24+
using ReactiveUI;                         // Метод расширения RaiseAndSetIfChanged
using ReactiveUI.Reactive;                // RoutingState, IScreen, ReactiveCommand, Interaction

namespace VAMedicalCallClient.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    public RoutingState Router { get; } = new();

    // [ИЗМЕНЕНО] Замена ObservableAsPropertyHelper на обычные свойства с RaiseAndSetIfChanged
    private string _currentModuleTitle = "Информационная система";
    public string CurrentModuleTitle
    {
        get => _currentModuleTitle;
        private set => this.RaiseAndSetIfChanged(ref _currentModuleTitle, value);
    }

    private string _currentModuleSubTitle = string.Empty;
    public string CurrentModuleSubTitle
    {
        get => _currentModuleSubTitle;
        private set => this.RaiseAndSetIfChanged(ref _currentModuleSubTitle, value);
    }

    private bool _isPaneOpen;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    public ReactiveCommand<Unit, bool> TogglePane { get; }
    public ReactiveCommand<Unit, Unit> NavigateToCalls { get; }
    public ReactiveCommand<Unit, Unit> NavigateToBrigades { get; }
    public ReactiveCommand<Unit, Unit> ShowAboutWindow { get; }
    public ReactiveCommand<Unit, Unit> ExitApp { get; }

    public Interaction<Unit, Unit> ShutdownInteraction { get; } = new();
    public Interaction<Unit, Unit> ShowAboutInteraction { get; } = new();

    public MainWindowViewModel()
    {
        TogglePane = ReactiveCommand.Create(() => IsPaneOpen = !IsPaneOpen);

        NavigateToCalls = ReactiveCommand.Create(() =>
        {
            Router.NavigateAndReset.Execute(new MedCallsLogViewModel(this)).Subscribe();
        });

        NavigateToBrigades = ReactiveCommand.Create(() =>
        {
            Router.NavigateAndReset.Execute(new MedBrigadesViewModel(this)).Subscribe();
        });

        // [ИЗМЕНЕНО] Прямая подписка на поток навигации вместо ToPropertyUnsafe
        Router.Navigate
            .Select(vm => vm is IModuleViewModel module ? module.ModuleTitle : "Информационная система")
            .Subscribe(title => CurrentModuleTitle = title);

        Router.Navigate
            .Select(vm => vm is IModuleViewModel module ? module.ModuleSubTitle : string.Empty)
            .Subscribe(subTitle => CurrentModuleSubTitle = subTitle);

        ShowAboutWindow = ReactiveCommand.CreateFromTask(async () =>
        {
            await ShowAboutInteraction.Handle(Unit.Default);
        });

        ExitApp = ReactiveCommand.CreateFromTask(async () =>
        {
            await ShutdownInteraction.Handle(Unit.Default);
        });

        // Стартовый модуль при открытии главного окна
        Router.NavigateAndReset.Execute(new MedCallsLogViewModel(this)).Subscribe();
    }
}
