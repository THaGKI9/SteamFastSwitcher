using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace SteamFastSwitcher
{
    /// <summary>
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }

        private void WindowSetting_Loaded(object sender, RoutedEventArgs e)
        {
            Config.Reload();
            CheckBoxAutoStart.IsChecked = Config.AutoStart;
            CheckBoxHideWhenMinimized.IsChecked = Config.MinimizedToTrayIcon == true;
            TextBoxSteamExecPath.Text = Config.SteamExecutionPath;
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executable File (.exe)|*.exe",
                CheckFileExists = true,
                Multiselect = false,
                InitialDirectory = string.IsNullOrWhiteSpace(Config.SteamExecutionPath) ? "" : Path.GetDirectoryName(Config.SteamExecutionPath)
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            {
                TextBoxSteamExecPath.Text = openFileDialog.FileName;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Config.AutoStart = CheckBoxAutoStart.IsChecked == true;
            Config.MinimizedToTrayIcon = CheckBoxHideWhenMinimized.IsChecked == true;
            Config.SteamExecutionPath = TextBoxSteamExecPath.Text;

            var autoRunAppName = System.Windows.Forms.Application.ProductName;
            if (Config.AutoStart == true) Autorun.Add(System.Windows.Forms.Application.ProductName, System.Windows.Forms.Application.ExecutablePath.ToString() + " -minimized");
            else Autorun.Remove(autoRunAppName);

            Config.Save();
            this.Close();
        }
    }
}
