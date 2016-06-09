using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using WallpaperRotator.Core;
using WallpaperRotator.Helper;

namespace WallpaperRotator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// timeintervalls in seconds
        /// </summary>
        private int[] updateIntervals = new int[] { 1800, 3600, 7200, 10800, 18000 };

        /// <summary>
        /// current time intervall -> index of array 
        /// </summary>
        private int currentIntervall = 2;

        /// <summary>
        /// timer to manager updates
        /// </summary>
        private Timer timer = null;

        /// <summary>
        /// datetime of the last update
        /// </summary>
        private DateTime lastChange = DateTime.Now;

        /// <summary>
        /// Unsplash Helper instance
        /// </summary>
        private Unsplash unsplash = null;

        /// <summary>
        /// strech wallpaper over all screens
        /// </summary>
        private bool strechImage = false;

        /// <summary>
        /// programm settings
        /// </summary>
        private Settings settings { get { return ((App)System.Windows.Application.Current).Settings; } }

        /// <summary>
        /// true if window will be close
        /// </summary>
        private bool isClose = false;

        /// <summary>
        /// constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // overwrite resources
            this.Resources = new ResourceDictionary();
            this.Resources.MergedDictionaries.Add(Helper.Localisation.CurrentResource);

            // restore window position
            var point = this.settings.Get<Point>("windowPosition");
            if (point != null)
            {
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                this.Left = point.X;
                this.Top = point.Y;
            }
            else
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            // instance unsplash helper
            this.unsplash = new Unsplash();

            // set download image size
            unsplash.ImageSize = new Size(SystemParameters.MaximumWindowTrackWidth, SystemParameters.MaximumWindowTrackHeight);

            RegistryKey autostartKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            this.checkboxStartWithWindows.IsChecked = (autostartKey.GetValue("Random Wallpaper Changer", null) != null);

            this.strechImage = this.settings.Get<bool>("strechImage", false);
            this.checkboxStrechImage.IsChecked = this.strechImage;
            this.checkboxStrechImage_checkedStateChange(this.checkboxStrechImage, new RoutedEventArgs());

            this.comboboxCategories.SelectedIndex = (this.settings.Get<int>("category", 0) + 1);
            this.categories_SelectionChanged(this.comboboxCategories, null);

            this.currentIntervall = this.settings.Get<int>("updateIntervall", 2);
            this.comboboxUpdateIntervall.SelectedIndex = this.currentIntervall;

            // register events
            this.comboboxCategories.SelectionChanged += categories_SelectionChanged;
            this.comboboxUpdateIntervall.SelectionChanged += updateIntervall_SelectionChanged;
            this.nextImage.Click += nextImage_Click;
            this.checkboxStrechImage.Unchecked += checkboxStrechImage_checkedStateChange;
            this.checkboxStrechImage.Checked += checkboxStrechImage_checkedStateChange;
            this.checkboxStartWithWindows.Unchecked += checkboxStartWithWindows_checkedStateChange;
            this.checkboxStartWithWindows.Checked += checkboxStartWithWindows_checkedStateChange;
            this.Closing += mainWindow_Closing;
            this.Loaded += mainWindow_Loaded;

            // initialize timer 
            this.timer = new Timer();
            this.timer.Interval = 10 * 1000; // 10 seconds
            this.timer.Tick += timer_Tick;
            this.timer.Enabled = true;
            this.timer.Start();

            // System Tray Icon 
            System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();

            System.Windows.Forms.NotifyIcon icon = new System.Windows.Forms.NotifyIcon();
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/WallpaperRotator;component/Resources/icon.ico")).Stream;
            notifyIcon.Icon = new System.Drawing.Icon(iconStream);
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += itemShow_Click;

            System.Windows.Forms.ToolStripMenuItem itemShow = new System.Windows.Forms.ToolStripMenuItem();
            itemShow.Text = (string)this.Resources["showHide"];
            itemShow.Click += itemShow_Click;

            System.Windows.Forms.ToolStripMenuItem itemNextImage = new System.Windows.Forms.ToolStripMenuItem();
            itemNextImage.Text = (string)this.Resources["nextImage"];
            itemNextImage.Click += itemNextImage_Click;

            System.Windows.Forms.ToolStripMenuItem itemExit = new System.Windows.Forms.ToolStripMenuItem();
            itemExit.Text = (string)this.Resources["close"];
            itemExit.Click += itemExit_Click;

            System.Windows.Forms.ContextMenuStrip contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                itemShow,
                itemNextImage,
                new System.Windows.Forms.ToolStripSeparator(),
                itemExit
            });
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            // clear appdata directory
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WallpaperRotator");
            if (System.IO.Directory.Exists(path))
            {
                foreach (System.IO.FileInfo file in (new DirectoryInfo(path)).GetFiles())
                {
                    if (file.Extension.ToLower() == ".jpg")
                    {
                        file.Delete();
                    }
                }
            }

            // update background
            this.nextImage_Click(this.nextImage, new RoutedEventArgs());
        }

        // close application
        private void itemExit_Click(object sender, EventArgs e)
        {
            this.isClose = true;
            App.Current.Shutdown();
        }

        // next image
        private void itemNextImage_Click(object sender, EventArgs e)
        {
            // update background
            this.nextImage_Click(this.nextImage, new RoutedEventArgs());
        }

        // systray double click or click to show window
        private void itemShow_Click(object sender, EventArgs e)
        {
            if (this.IsVisible)
            {
                this.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Visible;
            }
        }

        // main window will close
        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.isClose)
            {
                e.Cancel = true;
                this.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                // save window position
                this.settings.Set("windowPosition", this.PointToScreen(new Point(0, 0)));
            }
        }
        
        // wind did loaded
        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
        
        // check state changed 
        private void checkboxStrechImage_checkedStateChange(object sender, RoutedEventArgs e)
        {
            this.strechImage = (bool)(sender as System.Windows.Controls.CheckBox).IsChecked;
            this.settings.Set("strechImage", this.strechImage);

            if (this.strechImage)
            {
                unsplash.ImageSize = new Size(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            }
            else
            {
                unsplash.ImageSize = new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            }
        }

        // change start with windows settings
        private void checkboxStartWithWindows_checkedStateChange(object sender, RoutedEventArgs e)
        {
            if ((bool)this.checkboxStartWithWindows.IsChecked)
            {
                try
                {
                    RegistryKey autostartKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    autostartKey.SetValue("Random Wallpaper Changer", string.Format("{0} -m", System.Reflection.Assembly.GetExecutingAssembly().Location));
                    autostartKey.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("registry write error: [{0}]", ex.Message);
                }
            }
            else
            {
                try
                {
                    RegistryKey autostartKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (autostartKey.GetValue("Random Wallpaper Changer", null) != null)
                    {
                        autostartKey.DeleteValue("Random Wallpaper Changer");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("registry write error: [{0}]", ex.Message);
                }
            }
        }

        // categories selection changed
        private void categories_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int index = (sender as System.Windows.Controls.ComboBox).SelectedIndex - 1;
            this.unsplash.Category = (Unsplash.Categories)index;
            this.settings.Set("category", index);
        }

        // update intervall change
        private void updateIntervall_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int index = (sender as System.Windows.Controls.ComboBox).SelectedIndex;
            this.currentIntervall = index;
            this.settings.Set("updateIntervall", index);
        }

        // click on noxt image button
        private async void nextImage_Click(object sender, RoutedEventArgs e)
        {
            this.progress.Visibility = Visibility.Visible;
            this.nextImage.IsEnabled = false;

            FileInfo file = await unsplash.Next();
            if (file != null && file.Exists)
            {
                this.lastChange = DateTime.Now;
                Wallpaper.SetWallpaper(file.FullName, Wallpaper.BackgroundStyle.Fill);
                this.progress.Visibility = Visibility.Collapsed;
                this.nextImage.IsEnabled = true;
            }
        }

        // timer tick
        private void timer_Tick(object sender, EventArgs e)
        {
            TimeSpan timeSpan = DateTime.Now - this.lastChange;
            if (timeSpan.TotalSeconds >= this.updateIntervals[this.currentIntervall])
            {
                this.nextImage_Click(this.nextImage, new RoutedEventArgs());
            }
        }
    }
}
