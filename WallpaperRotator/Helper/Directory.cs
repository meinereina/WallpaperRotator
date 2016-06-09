using System.Diagnostics;
using System.IO;

namespace WallpaperRotator.Helper
{
    public static class Directory
    {
        /// <summary>
        /// create a dictionary
        /// </summary>
        /// <param name="directoryInfo">directory that will created</param>
        /// <returns>true if directory creates successfully</returns>
        public static bool CreateDirectory(DirectoryInfo directoryInfo)
        {
            string directory = string.Empty;
            string[] path = directoryInfo.FullName.Split('\\');

            for (int i = 0; i < path.Length; i++)
            {
                directory += string.Format("{0}\\", path[i]);
                if (!System.IO.Directory.Exists(directory))
                {
                    try
                    {
                        // create dictionary if not exists
                        System.IO.Directory.CreateDirectory(directory);
                    }
                    catch (IOException ex)
                    {
                        Debug.WriteLine("create directory error: [{0}]", ex.Message);
                        return false;
                    }
                }
            }
            return System.IO.Directory.Exists(directory);
        }
    }
}
