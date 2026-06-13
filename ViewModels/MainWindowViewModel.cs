using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace VAMedicalCallClient.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    public RoutingState Router { get; } = new();

    private readonly ObservableAsPropertyHelper<string> _currentModuleTitle;
    public string CurrentModuleTitle => _currentModuleTitle.Value;

    private readonly ObservableAsPropertyHelper<string> _currentModuleSubTitle;
    public string CurrentModuleSubTitle => _currentModuleSubTitle.Value;

    private bool _isPaneOpen;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }

    public ReactiveCommand<Unit, bool> TogglePane { get; }

    public ReactiveCommand<Unit, Unit> NavigateToCalls { get; }
    public ReactiveCommand<Unit, Unit> NavigateToBrigades { get; }

    public ReactiveCommand<Unit, Unit> ExitApp { get; }

    public Interaction<Unit, Unit> ShutdownInteraction { get; }

    public MainWindowViewModel()
    {
        ShutdownInteraction = new Interaction<Unit, Unit>();

        TogglePane = ReactiveCommand.Create(() => IsPaneOpen = !IsPaneOpen);

        NavigateToCalls = ReactiveCommand.Create(() =>
        {
            Router.NavigateAndReset.Execute(new MedCallsLogViewModel(this)).Subscribe();
        });

        NavigateToBrigades = ReactiveCommand.Create(() =>
        {
            Router.NavigateAndReset.Execute(new MedBrigadesViewModel(this)).Subscribe();
        });

        _currentModuleTitle = Router.Navigate
           .Select(vm => vm is IModuleViewModel module ? module.ModuleTitle : "Информационная система")
           .ToProperty(this, x => x.CurrentModuleTitle, "Информационная система");

        _currentModuleSubTitle = Router.Navigate
            .Select(vm => vm is IModuleViewModel module ? module.ModuleSubTitle : string.Empty)
            .ToProperty(this, x => x.CurrentModuleSubTitle, string.Empty);


        ExitApp = ReactiveCommand.CreateFromTask(async () =>
        {
            await ShutdownInteraction.Handle(Unit.Default);
        });

        // Стартовый модуль
        Router.NavigateAndReset.Execute(new MedCallsLogViewModel(this)).Subscribe();

    }
}
