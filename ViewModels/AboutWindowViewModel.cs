// Базовые типы и коллекции .NET
using System;
using System.Threading.Tasks;

// Реактивное программирование (System.Reactive)
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

// ReactiveUI 24+ и генераторы исходного кода
using ReactiveUI;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;


namespace VAMedicalCallClient.ViewModels
{
    public class AboutWindowViewModel : ViewModelBase
    {

        //Interactions 
        public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

        //Bound Properties               
        public string MajorVersion { get; }

        public string BuildVersion { get; }

        public string DeveloperEmail { get; } = "vivat-067@mail.ru";

        //Bound commands
        public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; }


        public AboutWindowViewModel()
        {
            string majorVersionInfo = Services.SystemInfoService.GetMajorVersion();
            string buildVersionInfo = Services.SystemInfoService.GetBuildVersion();
            string platformInfo = Services.SystemInfoService.GetPlatformInfo();

            MajorVersion = majorVersionInfo.Substring(0, 7);
            BuildVersion = $"({platformInfo[0]}:{buildVersionInfo[..^2]})";

            CloseWindowCommand = ReactiveCommand.CreateFromTask(CloseWindow);
        }

        private async Task CloseWindow()
        {
            await CloseWindowInteraction.Handle(Unit.Default);
        }

    }
}
