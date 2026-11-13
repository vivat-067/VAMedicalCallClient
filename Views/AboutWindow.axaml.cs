// Базовые типы и реактивное программирование (.NET / System.Reactive)
using System;
using System.Reactive;                        
using System.Reactive.Disposables.Fluent;     

// ReactiveUI 24+ и интеграция с Avalonia 12.1
using ReactiveUI;                             
using ReactiveUI.Reactive;                    
using ReactiveUI.Avalonia.Reactive;           // Базовые классы ReactiveWindow<T> и ReactiveUserControl<T>

using VAMedicalCallClient.ViewModels;

namespace VAMedicalCallClient.Views;

public partial class AboutWindow : ReactiveWindow<AboutWindowViewModel>
{
    public AboutWindow()
    {
        ViewModel = App.GetService<AboutWindowViewModel>();

        InitializeComponent();
        InitializeInteractionHandlers();
    }

    private void InitializeInteractionHandlers()
    {
        // Handle the close interaction
        this.WhenActivated(disposables =>
        {
            if (ViewModel is not { } vm) return;

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

