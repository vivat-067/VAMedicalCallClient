// Реактивное программирование
using System.Reactive;
using System.Reactive.Disposables.Fluent;

// ReactiveUI 24+ и Avalonia 12.1
using ReactiveUI;
using ReactiveUI.Reactive;
using ReactiveUI.Avalonia.Reactive;

// Пространства имён проекта
using VAMedicalCallClient.ViewModels;

namespace VAMedicalCallClient.Views;

public partial class CallEditorWindow : ReactiveWindow<CallEditorViewModel>
{
    public CallEditorWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel is not { } vm) return;

            vm.CloseDialogInteraction
                .RegisterHandler(interaction =>
                {
                    // Передаем результат (сохраненный MedicalCall или null) в ShowDialog<MedicalCall?>
                    Close(interaction.Input);
                    interaction.SetOutput(Unit.Default);
                })
                .DisposeWith(disposables);
        });
    }
}
