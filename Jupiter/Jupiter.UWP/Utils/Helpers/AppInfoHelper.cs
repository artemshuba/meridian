using System;
using Windows.ApplicationModel;

namespace Jupiter.Utils.Helpers
{
    /// <summary>
    /// Application info helper class
    /// </summary>
    public static class AppInfoHelper
    {
        /// <summary>
        /// Get app version
        /// </summary>
        /// <returns></returns>
        public static Version GetAppVersion()
        {
            var package = Package.Current;
            var packageId = package.Id;
            return new Version(packageId.Version.Major, packageId.Version.Minor, packageId.Version.Build, packageId.Version.Revision);
        }

        /// <summary>
        /// Get app version string
        /// </summary>
        public static string GetAppVersionString()
        {
            var version = GetAppVersion();

            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
