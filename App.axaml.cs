using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

using VAMedicalCallClient.ViewModels;
using VAMedicalCallClient.Views;

namespace VAMedicalCallClient
{
    public partial class App : Application
    {

        private IServiceProvider? serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {

            var services = new ServiceCollection();

            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<AboutWindowViewModel>();
            services.AddTransient<AboutWindow>();

            serviceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {                    
                    DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }



        //Get Service for access in other places
        public static T? GetService<T>() where T : class
        {
            return (Current as App)?.serviceProvider?.GetRequiredService<T>();
        }



    }
}
