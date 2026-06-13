using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;

namespace VAMedicalCallClient;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null) return null;

        // Получаем имя типа ViewModel
        var typeName = data.GetType().FullName;
        if (typeName == null) return null;

        // ФИКС: Если ReactiveUI обернул класс в динамический прокси, извлекаем имя базового класса
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

        // Выводим диагностическую ошибку прямо на экран, если View не найдена
        return new TextBlock
        {
            Text = $"Ошибка: Не удалось найти представление {viewName} для {typeName}",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Foreground = Avalonia.Media.Brushes.Red
        };
    }

    public bool Match(object? data) => data is ReactiveObject;
}
