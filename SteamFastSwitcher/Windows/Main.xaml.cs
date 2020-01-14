using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using ContextMenu = System.Windows.Forms.ContextMenu;
using System;

namespace SteamFastSwitcher
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        readonly NotifyIcon nIcon = new NotifyIcon();
        string[] accounts;

        public Main()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.nIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.Activate();
            };
            this.nIcon.Icon = System.Drawing.Icon.FromHandle(Properties.Resources.AppIcon.Handle);
            this.nIcon.Text = this.Title;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1] == "-minimized") this.Hide();

            Config.Reload();
            if (string.IsNullOrEmpty(Config.SteamExecutionPath))
            {
                // Try to get steam
                Config.SteamExecutionPath = Steam.GetSteamExecution();
                if (string.IsNullOrEmpty(Config.SteamExecutionPath))
                {
                    // Fail to get steam location. Let user choose themselves
                    MessageBox.Show("Cannot find Steam.exe. Please locate steam executable file.", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    ButtonSetting_Click(sender, e);
                }
                else
                {
                    Config.Save();
                }
            }

            ReloadAccounts();
            this.nIcon.Visible = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Config.MinimizedToTrayIcon == null)
            {
                var result = MessageBox.Show("Would you minimized the window to tray icon? \n" +
                    "Click \"Yes\" to minimized,\n" +
                    "Click \"No\" to exit.", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Config.MinimizedToTrayIcon = true;
                        break;
                    case MessageBoxResult.No:
                        Config.MinimizedToTrayIcon = false;
                        break;
                }
            }

            if (Config.MinimizedToTrayIcon == true)
            {
                e.Cancel = true;
                this.Hide();
            }
            Config.Save();
        }

        private void ButtonReload_Click(object sender, RoutedEventArgs e)
        {
            ReloadAccounts();
            MessageBox.Show("Accounts has been reloaded from Steam", this.Title, MessageBoxButton.OK);
        }

        private void ListBoxAccounts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItem = (sender as ListBox).SelectedItem;
            if (selectedItem != null)
            {
                this.Hide();
                var account = selectedItem.ToString();
                RestartSteamAsGivenAccount(account);
            }
        }

        private void ButtonSetting_Click(object sender, RoutedEventArgs e)
        {
            new Setting().ShowDialog();
        }

        private void ButtonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please login to Steam with your new account. \n" +
                "Remember to check \"Remember Password\" when you login to Steam!\n" +
                "\n" +
                "After login into Steam, click \"Reload\" to reload account informations", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);

            RestartSteamAsGivenAccount("");
        }

        private void BeforeExit()
        {
            Config.Save();
            this.nIcon.Visible = false;
            this.Close();

            System.Environment.Exit(0);
        }

        private void RestartSteamAsGivenAccount(string account, int timeout = 10000)
        {
            while (Steam.IsSteamRunning(Config.SteamExecutionPath))
            {
                Steam.Shutdown(Config.SteamExecutionPath);
                Thread.Sleep(1000);
            }

            Steam.SetAutoLoginAccount(account);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            while (true)
            {
                var process = Steam.Start(Config.SteamExecutionPath);
                if (!process.WaitForExit(200)) break;
                Thread.Sleep(100);

                if (stopWatch.ElapsedMilliseconds > timeout)
                {
                    stopWatch.Stop();
                    this.Show();
                    MessageBox.Show("Start Steam timeout!", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
            }
        }

        private void ReloadAccounts()
        {
            this.accounts = Steam.ReadAccounts(Config.SteamExecutionPath);
            ListBoxAccounts.ItemsSource = this.accounts;
            this.RefreshMenu();
        }

        private void RefreshMenu()
        {
            var contextMenu = new ContextMenu();

            if (accounts.Length == 0)
            {
                contextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem
                {
                    Enabled = false,
                    Text = "- No available accounts -"
                });
            }
            else
            {
                foreach (var account in accounts)
                {
                    contextMenu.MenuItems.Add(account, (s, e) => RestartSteamAsGivenAccount(account));
                }
            }
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add("Reload", (s, e) => ButtonReload_Click(s, null));
            contextMenu.MenuItems.Add("Setting", (s, e) => ButtonSetting_Click(s, null));
            contextMenu.MenuItems.Add("Exit", (s, e) => BeforeExit());
            this.nIcon.ContextMenu = contextMenu;
        }
    }
}
