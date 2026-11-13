// Базовые типы (.NET)
using System;                             // Работа с Type, Activator и StringComparison

// Avalonia UI
using Avalonia.Controls;                  // Базовый класс Control и TextBlock
using Avalonia.Controls.Templates;        // Интерфейс IDataTemplate
using Avalonia.Layout;                    // Выравнивание HorizontalAlignment и VerticalAlignment
using Avalonia.Media;                     // Кисти Brushes

// ReactiveUI 24+
using ReactiveUI.Reactive;                // Базовый класс ReactiveObject для проверки типа в Match()

namespace VAMedicalCallClient;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null) return null;

        // Получаем имя типа ViewModel
        var typeName = data.GetType().FullName;
        if (typeName == null) return null;

        // Извлекаем имя базового класса, если использован динамический прокси
        if (typeName.Contains("Castle.Proxies") || typeName.Contains("Proxy"))
        {
            typeName = data.GetType().BaseType?.FullName ?? typeName;
        }

        // Превращаем VAMedicalCallClient.ViewModels.NameViewModel в VAMedicalCallClient.Views.NameView
        var viewName = typeName.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(viewName);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        // Диагностическая заглушка, если представление не найдено
        return new TextBlock
        {
            Text = $"Ошибка: Не удалось найти представление {viewName} для {typeName}",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.Red
        };
    }

    public bool Match(object? data) => data is ReactiveObject;
}
