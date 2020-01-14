using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace SteamFastSwitcher
{
    public class Steam
    {
        static string GetSteamFolder(string steamExecPath) => Path.GetDirectoryName(steamExecPath);

        static RegistryKey GetSteamRegistryKey()
        {
            const string keyName = @"Software\Valve\Steam";

            var key = Registry.CurrentUser.OpenSubKey(keyName, true);
            return key ?? Registry.CurrentUser.CreateSubKey(keyName);
        }

        /// <summary>
        /// Get Steam install folder path from registry
        /// </summary>
        /// <returns></returns>
        public static string GetSteamExecution()
        {
            using (var key = GetSteamRegistryKey())
            {
                return Path.GetFullPath(key == null ? "" : key.GetValue("SteamExe", defaultValue: "").ToString());
            }
        }

        /// <summary>
        /// Read accounts information from Steam config
        /// </summary>
        /// <param name="steamExecPath"></param>
        /// <returns>array of existing accounts</returns>
        public static string[] ReadAccounts(string steamExecPath)
        {
            var accountIDs = new List<string>();
            var steamFolderPath = GetSteamFolder(steamExecPath);
            var configPath = Path.Combine(steamFolderPath, "config", "loginusers.vdf");

            using (var reader = File.OpenText(configPath))
            {
                const string lineBeginning = "\t\t\"AccountName\"\t\t\"";
                do
                {
                    var line = reader.ReadLine();
                    if (line.StartsWith(lineBeginning))
                    {
                        var accountID = line.Substring(lineBeginning.Length, line.Length - lineBeginning.Length - 1);
                        accountIDs.Add(accountID);
                    }
                }
                while (!reader.EndOfStream);

                return accountIDs.ToArray();
            }
        }

        /// <summary>
        /// Set Steam auto login account in system registry
        /// </summary>
        /// <param name="accountID"></param>
        public static void SetAutoLoginAccount(string accountID)
        {
            using (var key = GetSteamRegistryKey())
            {
                key.SetValue("AutoLoginUser", accountID);
                key.DeleteValue("LastGameNameUsed", false);
            }
        }

        /// <summary>
        /// Launch Steam
        /// </summary>
        /// <param name="steamExecPath"></param>
        public static Process Start(string steamExecPath)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = steamExecPath,
                WorkingDirectory = steamExecPath
            });
        }

        /// <summary>
        /// Shutdown Steam gracefully
        /// </summary>
        /// <param name="steamExecPath"></param>
        public static void Shutdown(string steamExecPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = steamExecPath,
                Arguments = "-shutdown",
                WorkingDirectory = steamExecPath,
                CreateNoWindow = true,
            }).WaitForExit();
        }
        
        /// <summary>
        /// Check if Steam is running
        /// </summary>
        /// <param name="steamExecPath"></param>
        /// <returns></returns>
        public static bool IsSteamRunning(string steamExecPath)
        {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(steamExecPath)).Length > 0;
        }
    }

}
