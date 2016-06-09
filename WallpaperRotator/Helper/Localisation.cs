using System;
using System.Threading;
using System.Windows;

namespace WallpaperRotator.Helper
{
    public static class Localisation
    {
        /// <summary>
        /// get the current resource dictionary
        /// </summary>
        public static ResourceDictionary CurrentResource
        {
            get
            {
                ResourceDictionary dict = new ResourceDictionary();
                switch (Thread.CurrentThread.CurrentCulture.ToString())
                {
                    case "de-DE":
                        dict.Source = new Uri("..\\Resources\\Strings.de-DE.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("..\\Resources\\Strings.xaml", UriKind.Relative);
                        break;
                }
                return dict;
            }
        }

    }
}
