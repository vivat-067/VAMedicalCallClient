// Базовые типы и DI (.NET)
using System;                                     // IServiceProvider
using Microsoft.Extensions.DependencyInjection;   // ServiceCollection, BuildServiceProvider

// Avalonia UI
using Avalonia;                                   // Application
using Avalonia.Controls.ApplicationLifetimes;     // IClassicDesktopStyleApplicationLifetime
using Avalonia.Markup.Xaml;                       // AvaloniaXamlLoader

// Пространства имён проекта
using VAMedicalCallClient.ViewModels;             // Классы ViewModel
using VAMedicalCallClient.Views;                  // MainWindow
using VAMedicalCallClient.Services;               // Интерфейсы и реализации сервисов


namespace VAMedicalCallClient
{
    public partial class App : Application
    {

        private IServiceProvider? _serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {

            var services = new ServiceCollection();

            // Сервисы
            services.AddSingleton<IMedicalCallApiService, MedicalCallApiService>();            
            services.AddSingleton<IMedicalBrigadeDataService, MedicalBrigadeDataService>();          

            // ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<AboutWindowViewModel>();

            services.AddTransient<MedCallsLogViewModel>();
            services.AddTransient<MedBrigadesViewModel>();            


            _serviceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {                    
                    DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }


        public static T? GetService<T>() where T : class
        {
            return (Current as App)?._serviceProvider?.GetRequiredService<T>();
        }


    }
}
