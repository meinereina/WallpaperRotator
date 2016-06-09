using System;
using System.Windows;
using WallpaperRotator.Helper;

namespace WallpaperRotator
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private Settings settings = null;
        /// <summary>
        /// App settings
        /// </summary>
        public Settings Settings
        {
            get
            {
                if (this.settings == null)
                    this.settings = new Settings(string.Format("{0}\\WallpaperRotator\\settings.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));

                return this.settings;
            }
        }
    }
}
