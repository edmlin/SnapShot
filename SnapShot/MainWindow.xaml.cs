using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms=System.Windows.Forms;
using System.Configuration;
using System.Reflection;
using System.ComponentModel;
using System.IO;

namespace SnapShot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private GlobalKeyboardHook _globalKeyboardHook;
        private LowLevelKeyboardListener _listener;
        System.Windows.Forms.NotifyIcon m_notifyIcon;
        string outputFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        public string OutputFolder
        {
            get
            {
                return outputFolder;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    outputFolder = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputFolder"));
                }
            }
        }
        public long Quality { get; set; } = 80L;
        Hotkey hotkey = new Hotkey();
        bool reallyClose = false;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void tbHotKey_GotFocus(object sender, RoutedEventArgs e)
        {
            hotkey.SettingHotkey = true;
        }
        public string HotkeyString { get { return hotkey.HotkeyString; } }
        void SetHotkey(IEnumerable<Key> keys)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("HotkeyString"));
        }

        private void tbHotKey_LostFocus(object sender, RoutedEventArgs e)
        {
            hotkey.SettingHotkey = false;
        }

        WinForms.ContextMenu CreateTrayContextMenu()
        {
            var menu = new WinForms.ContextMenu();
            menu.MenuItems.Add("Open Output Folder", miOpenFolder_Click);
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Exit", btExit_Click);
            return menu;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
            m_notifyIcon.BalloonTipTitle = "SnapShot";
            m_notifyIcon.Text = "SnapShot";
            m_notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            m_notifyIcon.ContextMenu = CreateTrayContextMenu();
            m_notifyIcon.MouseClick += new WinForms.MouseEventHandler(m_notifyIcon_MouseClick);
            m_notifyIcon.Visible = false;
            OutputFolder = ConfigurationManager.AppSettings["OutputFolder"] ?? System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            hotkey.OnHotkeySet += (o, ev) => PropertyChanged(this, new PropertyChangedEventArgs("HotkeyString"));
            hotkey.OnHotkey += (o, ev) => ScreenShot.Take(System.IO.Path.Combine(OutputFolder, DateTime.Now.ToString("yyyyMMddHHmmss")), Quality);
            long quality;
            if (long.TryParse(ConfigurationManager.AppSettings["Quality"], out quality))
            {
                Quality = quality;
                PropertyChanged(this, new PropertyChangedEventArgs("Quality"));
            }
            if (ConfigurationManager.AppSettings["Hotkey"]!=null)
            {
                try
                {
                    hotkey.SetHotkey(ConfigurationManager.AppSettings["Hotkey"]);
                }
                catch
                {
                    hotkey.SetHotkey(new List<Key>() { Key.Snapshot });
                }
            }
            else
            {
                hotkey.SetHotkey(new List<Key>() { Key.Snapshot });
            }
        }

        private void m_notifyIcon_MouseClick(object sender, WinForms.MouseEventArgs e)
        {
            if (e.Button == WinForms.MouseButtons.Left)
            {
                Show();
                WindowState = m_storedWindowState;
                Activate();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (reallyClose)
            {
                m_notifyIcon.Dispose();
                m_notifyIcon = null;
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            hotkey.SettingHotkey = false;
        }

        private void tbHotKey_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            hotkey.SettingHotkey = true;
        }


        private void tbHotKey_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            hotkey.SettingHotkey = false;
        }

        void SetAppSetting(System.Configuration.Configuration config, string key,string value)
        {
            var settings = config.AppSettings.Settings;
            if (settings[key] == null)
            {
                settings.Add(key, value);
            }
            else
            {
                settings[key].Value = value;
            }
        }
        private void btSAVE_Click(object sender, RoutedEventArgs e)
        {
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(
                ConfigurationUserLevel.None);
            SetAppSetting(config, "OutputFolder", OutputFolder);
            SetAppSetting(config, "Quality", Quality.ToString());
            SetAppSetting(config, "Hotkey", hotkey.HotkeyString);
            config.Save(ConfigurationSaveMode.Modified);
        }


        private void btExit_Click(object sender, EventArgs e)
        {
            reallyClose = true;
            Close();
        }

        private WindowState m_storedWindowState = WindowState.Normal;

        public event PropertyChangedEventHandler PropertyChanged;

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            else
                m_storedWindowState = WindowState;
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }
        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CheckTrayIcon();
        }

        void SelectFolder()
        {
            var dlg = new WinForms.FolderBrowserDialog();
            dlg.SelectedPath = OutputFolder;
            var result = dlg.ShowDialog(null);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                OutputFolder = dlg.SelectedPath;
            }
        }

        private void tbFolder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectFolder();
        }

        private void btOpen_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(tbFolder.Text);
        }

        private void miOpenFolder_Click(object sender, EventArgs e)
        {
            Process.Start(tbFolder.Text);
        }

    }
}
