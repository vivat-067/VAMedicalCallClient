// Базовые типы и реактивное программирование (.NET / System.Reactive)
using System;
using System.Reactive.Disposables.Fluent;     
using System.Reactive.Linq;                   // Для работы с Interaction

// Avalonia UI
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

// ReactiveUI 24+ и интеграция с Avalonia 12.1
using ReactiveUI;                             
using ReactiveUI.Reactive;                    
using ReactiveUI.Avalonia.Reactive;           // Базовый класс ReactiveUserControl<T>

// Пространства имён проекта
using VAMedicalCallClient.Models;             
using VAMedicalCallClient.ViewModels;

namespace VAMedicalCallClient.Views;


public partial class MedCallsLogView : ReactiveUserControl<MedCallsLogViewModel>
{
    public MedCallsLogView()
    {
        InitializeComponent();

        // Регистрация обработчика диалога редактирования/создания заявки
        this.WhenActivated(disposables =>
        {
            if (ViewModel is not { } vm) return;

            vm.ShowCallEditorInteraction
                .RegisterHandler(async interaction =>
                {
                    var dialog = new CallEditorWindow
                    {
                        ViewModel = interaction.Input
                    };

                    // Получаем родительское главное окно для модального показа диалога
                    var ownerWindow = TopLevel.GetTopLevel(this) as Window;
                    var result = ownerWindow != null
                        ? await dialog.ShowDialog<MedicalCall?>(ownerWindow)
                        : null;

                    interaction.SetOutput(result);
                })
                .DisposeWith(disposables);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Триггер загрузки только тогда, когда DataGrid прикрепился к окну
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (ViewModel is { } viewModel)
        {
            viewModel.LoadCallsCommand.Execute().Subscribe();
        }
    }
}
