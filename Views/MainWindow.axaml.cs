// Базовые типы и реактивное программирование (.NET / System.Reactive)
using System;
using System.Reactive;                        
using System.Reactive.Disposables.Fluent;     

// ReactiveUI 24+ и интеграция с Avalonia 12.1
using ReactiveUI;                             
using ReactiveUI.Reactive;                    // Метод активации представления this.WhenActivated(...)
using ReactiveUI.Avalonia.Reactive;           // Базовые классы ReactiveWindow<T> и ReactiveUserControl<T>
using VAMedicalCallClient.ViewModels;

namespace VAMedicalCallClient.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel != null)
            {                
                ViewModel.ShutdownInteraction
                    .RegisterHandler(interaction =>
                    {
                        this.Close();
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);


                ViewModel.ShowAboutInteraction
                    .RegisterHandler(async interaction =>
                    {
                        await new AboutWindow().ShowDialog(this);
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);

            }


        });
    }
}