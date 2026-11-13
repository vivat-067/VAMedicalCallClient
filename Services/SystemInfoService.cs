using System;
using System.Diagnostics;
using System.Reflection;


namespace VAMedicalCallClient.Services
{
    public static class SystemInfoService
    {
        private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();

        public static string GetMajorVersion()
        {
            var fileVersionAttr = CurrentAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (!string.IsNullOrWhiteSpace(fileVersionAttr?.Version))            
                return fileVersionAttr.Version;            
            
            if (!string.IsNullOrEmpty(Environment.ProcessPath))            
                return FileVersionInfo.GetVersionInfo(Environment.ProcessPath).FileVersion ?? "2025.11";            

            return "2026.11";
        }

        public static string GetBuildVersion()
        {
            var version = CurrentAssembly.GetName().Version;
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
