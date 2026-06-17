using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Reactive;

using System.Reactive.Disposables.Fluent;
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