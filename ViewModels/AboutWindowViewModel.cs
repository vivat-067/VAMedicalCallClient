using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

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
            string buldVersionInfo = Services.SystemInfoService.GetBuildVersion();
            string platformInfo = Services.SystemInfoService.GetPlatformInfo();

            MajorVersion = majorVersionInfo.Substring(0, 7);
            BuildVersion = $"({platformInfo[0]}:{buldVersionInfo[..^2]})";

            CloseWindowCommand = ReactiveCommand.CreateFromTask(CloseWindow);
        }

        private async Task CloseWindow()
        {
            await CloseWindowInteraction.Handle(Unit.Default);
        }

    }
}
