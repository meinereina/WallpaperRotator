using System;
using System.IO;
using System.Xml.Serialization;

namespace WallpaperRotator.Helper
{
    /// <summary>
    /// convert types
    /// </summary>
    public static class TypeConverter
    {
        #region Primitive Typen
        /// <summary>
        /// convert a string to bool
        /// </summary>
        /// <param name="value">string value</param>
        /// <returns>bool value</returns>
        public static bool ToBool(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            try
            {
                return Convert.ToBoolean(value);
            }
            catch
            {
                switch (value.ToLower())
                {
                    case "1":
                    case "t":
                    case "true":
                        return true;
                    case "0":
                    case "f":
                    case "false":
                        return false;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// convert a string to int
        /// </summary>
        /// <param name="value">string value</param>
        /// <returns>int value</returns>
        public static int ToInt(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return -1;

            try
            {
                return (int)Convert.ToInt64(value);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// convert a string to long
        /// </summary>
        /// <param name="value">string value</param>
        /// <returns>long value</returns>
        public static long ToLong(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return -1;

            try
            {
                return (int)Convert.ToInt64(value);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// convert a string to double
        /// </summary>
        /// <param name="value">string value</param>
        /// <returns>double value</returns>
        public static double ToDouble(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return -1;

            string doubleString = value.Replace(",", ".");

            try
            {
                return double.Parse(doubleString, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return -1;
            }
        }
        #endregion

        #region Serialisieren
        /// <summary>
        /// serialize object to string
        /// </summary>
        /// <typeparam name="T">object to serialize</typeparam>
        /// <param name="toSerialize">objext as string</param>
        /// <returns></returns>
        public static string SerializeObject(this object toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }
        /// <summary>
        /// deserialize object to string
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="value">object as string</param>
        /// <returns>deserialized object</returns>
        public static T DeSerializeObject<T>(this string value)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            try
            {
                using (TextReader textReader = new StringReader(value))
                {
                    return (T)(object)xmlSerializer.Deserialize(textReader);
                }
            }
            catch
            {
                return default(T);
            }
        }
        #endregion
    }
}
