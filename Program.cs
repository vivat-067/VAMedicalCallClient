//--------------------------------------------------------------------
//            МедВызов: регистрация заявок вызова СМП
// -------------------------------------------------------------------
// Демо-версия ПРОТОТИПА МЕДИЦИНСКОЙ информационной системы
// Рабочее место диспетчера СМП
// Раздел заявок вызовов СМП, тестовые данные получаемые по REST API от сервера
// Раздел мониторинга состояния и загрузки бригад СМП, тестовые данные загружаемые из json файла
// Клиент .NET С# Avalonia Reactive UI MVVM DI
// Vit Vatkov   vivat-067@mail.ru


using System;
using Avalonia;
using ReactiveUI.Avalonia;

namespace VAMedicalCallClient
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI(builder => { });
    }
}
