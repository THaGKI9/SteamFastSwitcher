using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SteamFastSwitcher
{
    public static class Config
    {
        public static bool AutoStart;
        public static string SteamExecutionPath;
        public static bool? MinimizedToTrayIcon = null;

        private const string configFileName = "safs.config.ini";
        private const string defaultSectionName = "SteamFastSwitcher";

        public static void Save()
        {
            SetBooleanValue("AutoStart", AutoStart);
            SetValue("SteamExecutionPath", SteamExecutionPath);
            SetNullableBooleanValue("MinimizedToTrayIcon", MinimizedToTrayIcon);
        }

        public static void Reload()
        {
            Config.MinimizedToTrayIcon = GetNullableBooleanValue("MinimizedToTrayIcon");
            Config.AutoStart = GetBooleanValue("AutoStart");
            Config.SteamExecutionPath = GetConfigValue("SteamExecutionPath");
        }

        /// <summary>
        /// Copies a string into the specified section of an initialization file.
        /// </summary>
        /// <returns>
        /// If the function successfully copies the string to the initialization file, the return value is nonzero.
        /// If the function fails, or if it flushes the cached version of the most recently accessed initialization file, the return value is zero. 
        /// </returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// Retrieves a string from the specified section in an initialization file.
        /// </summary>
        /// <returns>
        /// The return value is the number of characters copied to the buffer, not including the terminating null character.
        /// </returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private static string GetAbsoluteFilePath()
        {
            string basePath = System.IO.Directory.GetCurrentDirectory();
            return Path.Combine(basePath, configFileName);
        }

        private static void SetValue(string key, string value)
        {
            WritePrivateProfileString(defaultSectionName, key, value, GetAbsoluteFilePath());
        }

        private static void SetBooleanValue(string key, bool value)
        {
            SetValue(key, value.ToString());
        }

        private static void SetNullableBooleanValue(string key, bool? value)
        {
            if (value != null) SetValue(key, value.ToString());
        }

        private static string GetConfigValue(string key)
        {
            const int bufferSize = 1024;
            var sb = new StringBuilder(bufferSize);
            GetPrivateProfileString(defaultSectionName, key, null, sb, bufferSize, GetAbsoluteFilePath());
            return sb.ToString();
        }

        private static bool GetBooleanValue(string key)
        {
            const int bufferSize = 1024;
            var sb = new StringBuilder(bufferSize);
            GetPrivateProfileString(defaultSectionName, key, null, sb, bufferSize, GetAbsoluteFilePath());
            return sb.ToString() == "True";
        }

        private static bool? GetNullableBooleanValue(string key)
        {
            const int bufferSize = 1024;
            var sb = new StringBuilder(bufferSize);
            GetPrivateProfileString(defaultSectionName, key, null, sb, bufferSize, GetAbsoluteFilePath());

            var value = sb.ToString();
            if (value.Length == 0) return null;
            else return value == "True";
        }
    }
}
