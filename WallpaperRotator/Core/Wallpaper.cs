using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace WallpaperRotator.Core
{
    /// <summary>
    /// manage wallpaper actions 
    /// </summary>
    public static class Wallpaper
    {
        #region Enums
        /// <summary>
        /// Background Styles
        /// </summary>
        public enum BackgroundStyle : int
        {
            Tile = -1,
            Center = 0,
            Stretch = 2,
            Fill = 10,
            Fit = 6,
            Span = 22
        }
        #endregion

        #region WIN-API
        /* 
         * Windows API Call to get and set Wallpaper informations
         */
        private static readonly UInt32 SPI_SETDESKWALLPAPER = 0x14;
        private static readonly UInt32 SPIF_UPDATEINIFILE = 0x01;
        private static readonly UInt32 SPIF_SENDWININICHANGE = 0x02;
        private static readonly UInt32 SPI_GETDESKWALLPAPER = 0x73;
        private static readonly int MAX_PATH = 260;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);
        #endregion

        /// <summary>
        /// Get the current Wallpaper
        /// </summary>
        /// <returns>Path to the current wallpaper</returns>
        public static string GetWallpaper()
        {
            String wallpaper = new String('\0', MAX_PATH);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, (UInt32)wallpaper.Length, wallpaper, 0);
            return wallpaper.Substring(0, wallpaper.IndexOf('\0')); ;
        }

        /// <summary>
        /// returns the current wallpaper style
        /// </summary>
        public static BackgroundStyle GetWallpaperStyle()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
            int style = Convert.ToInt32(registryKey.GetValue("WallpaperStyle", "1"));
            int tile = Convert.ToInt32(registryKey.GetValue("TileWallpaper", "0"));

            if (tile == 1)
            {
                return BackgroundStyle.Tile;
            }
            else
            {
                return (BackgroundStyle)style;
            }
        }

        /// <summary>
        /// set a new wallpaper 
        /// </summary>
        /// <param name="filename">path to file</param>
        /// <param name="style">wallpaper style</param>
        public static void SetWallpaper(string filename, BackgroundStyle style)
        {
            // write image style to registry 
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
            if (style == BackgroundStyle.Tile)
            {
                registryKey.SetValue("WallpaperStyle", "0");
                registryKey.SetValue("TileWallpaper", "1");
            }
            else
            {
                registryKey.SetValue("WallpaperStyle", ((int)style).ToString());
                registryKey.SetValue("TileWallpaper", "0");
            }

            // save current image as bitmap image
            string tmpFilename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WallpaperRotator\\tmp.bmp";
            BitmapImage image = new BitmapImage(new Uri(filename));
            MemoryStream memStream = new MemoryStream();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(memStream);
            File.WriteAllBytes(tmpFilename, memStream.GetBuffer());
            memStream.Close();

            if (File.Exists(tmpFilename))
            {
                // change wallpaper
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tmpFilename, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
        }
    }
}
