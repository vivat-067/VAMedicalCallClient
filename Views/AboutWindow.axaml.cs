using Avalonia.Controls; // Используем стандартный Window
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using VAMedicalCallClient.ViewModels;

namespace VAMedicalCallClient.Views;

public partial class AboutWindow : ReactiveWindow<AboutWindowViewModel>
{
    public AboutWindow()
    {
        ViewModel = App.GetService<AboutWindowViewModel>()!;

        InitializeComponent();
        InitializeInteractionHandlers();
    }

    private void InitializeInteractionHandlers()
    {
        if (ViewModel?.CloseWindowInteraction == null)
        {
            Debug.WriteLine("Warning: ViewModel or CloseWindowInteraction is null in AboutWindow");
            return;
        }

        // Handle the close interaction
        this.WhenActivated(disposables =>
        {
            ViewModel.CloseWindowInteraction
                    .RegisterHandler(interaction =>
                    {
                        Close();
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);
        });

    }
}

