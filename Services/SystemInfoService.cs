using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAMedicalCallClient.Services
{
    public static class SystemInfoService
    {

        public static string GetMajorVersion()
        {
            var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(
                              System.Reflection.Assembly.GetExecutingAssembly().Location
                                ).FileVersion;
            return fileVersion?.ToString() ?? "2025.11";
        }

        public static string GetBuildVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "2025.11.19.1111";
        }

        public static string GetPlatformInfo()
        {
            return OperatingSystem.IsWindows() ? "Windows" :
                   OperatingSystem.IsLinux() ? "Linux" :
                   OperatingSystem.IsMacOS() ? "macOS" :
                   OperatingSystem.IsAndroid() ? "Android" :
                   OperatingSystem.IsIOS() ? "iOS" :
                   "Unknown Platform";
        }

    }
}
