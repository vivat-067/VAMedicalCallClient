using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VAMedicalCallClient.ViewModels;

namespace VAMedicalCallClient.Views;

public partial class MedCallsLogView : UserControl
{
    public MedCallsLogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Триггерим загрузку только тогда, когда DataGrid прикрепился к окну
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is MedCallsLogViewModel viewModel)
        {
            viewModel.LoadCallsCommand.Execute().Subscribe();
        }
    }
}
