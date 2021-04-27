using Windows.Graphics.Display;
using Windows.Security.Cryptography;
using Windows.System.Profile;

namespace Jupiter.Utils.Helpers
{
    /// <summary>
    /// Helper class to get device information
    /// </summary>
    public static class DeviceHelper
    {
        private static ResolutionScale _resolutionScale = ResolutionScale.Invalid;

        /// <summary>
        /// Resolution scale
        /// </summary>
        //public static ResolutionScale ResolutionScale
        //{
        //    get
        //    {
        //        if (_resolutionScale == ResolutionScale.Invalid)
        //            _resolutionScale = DisplayInformation.GetForCurrentView().ResolutionScale;

        //        return _resolutionScale;
        //    }
        //}

        /// <summary>
        /// Returns device id
        /// </summary>
        public static string GetDeviceId()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;

            return CryptographicBuffer.EncodeToHexString(hardwareId);
        }

        /// <summary>
        /// returns OS version
        /// </summary>
        public static string GetOsVersion()
        {
            // get the system version number
            string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (v & 0x000000000000FFFFL);
            return $"{v1}.{v2}.{v3}.{v4}";
        }

        /// <summary>
        /// Returns true if current device is a mobile phone
        /// </summary>
        public static bool IsMobile()
        {
            return AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";
        }
    }
}