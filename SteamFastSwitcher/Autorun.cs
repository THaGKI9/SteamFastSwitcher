using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamFastSwitcher
{
    public class Autorun
    { 
        const string AutoRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        
        /// <summary>
        /// Add a execuatable file to autorun list. 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="execuationPath"></param>
        public static void Add(string appName, string execuationPath)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(AutoRunKey, true))
            {
                if (Registry.CurrentUser.OpenSubKey(AutoRunKey, true).GetValue(appName) == null) 
                    key.SetValue(appName, execuationPath);
            }
        }

        /// <summary>
        /// Remove autorun item with name appName.
        /// </summary>
        /// <param name="appName"></param>
        public static void Remove(string appName)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(AutoRunKey, true))
            {
                if (Registry.CurrentUser.OpenSubKey(AutoRunKey, true).GetValue(appName) != null)  key.DeleteValue(appName);
            }
        }

        /// <summary>
        /// Return whether autorun item with name appName exists
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static bool IsStartupItem(string appName)
        {
            return Registry.CurrentUser.OpenSubKey(AutoRunKey, true).GetValue(appName) == null;
        }
    }
}
