using System;
using System.IO;
using System.Xml;

namespace WallpaperRotator.Helper
{
    public class Settings
    {
        /// <summary>
        /// filename of settings file
        /// </summary>
        private FileInfo fileInfo = null;
        public FileInfo FileInfo { get { return this.fileInfo; } }

        /// <summary>
        /// xml document cache 
        /// </summary>
        private XmlDocument documentCache = null;

        /// <summary>
        /// xml document with all settings
        /// </summary>
        private XmlDocument document
        {
            get
            {
                if (this.documentCache == null)
                {
                    this.documentCache = new XmlDocument();
                    this.documentCache.Load(this.fileInfo.FullName);
                }
                return this.documentCache;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="filename">filename of the settings file</param>
        public Settings(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                this.fileInfo = new FileInfo(filename);
                if (!this.fileInfo.Exists)
                {
                    // if directory dosn't exists -> create it
                    if (Helper.Directory.CreateDirectory(new DirectoryInfo(this.fileInfo.DirectoryName)))
                    {
                        XmlDocument document = new XmlDocument();
                        XmlNode root = document.CreateElement("settings");
                        document.AppendChild(root);
                        document.Save(this.fileInfo.FullName);
                    }
                }
            }
        }

        /// <summary>
        /// get the settings with key
        /// </summary>
        /// <typeparam name="T">type of value</typeparam>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public T Get<T>(string key)
        {
            return this.Get<T>(key, default(T));
        }

        /// <summary>
        /// get the settings with key
        /// if value dosn't exist then take the default value
        /// </summary>
        /// <typeparam name="T">type of value</typeparam>
        /// <param name="key">key</param>
        /// <param name="defaultValue">default value if setting not exists</param>
        /// <returns>value</returns>
        public T Get<T>(string key, T defaultValue)
        {
            XmlNode node = this.getXMLNode(key);
            if (node != null)
            {
                Type type = Type.GetType(node.Attributes.GetNamedItem("type").Value);
                string value = node.Attributes.GetNamedItem("value").Value;

                if (typeof(T) == typeof(string))
                    return (T)(object)value;

                if (typeof(T) == typeof(bool))
                    return (T)(object)value.ToBool();

                if (typeof(T) == typeof(int))
                    return (T)(object)value.ToInt();

                if (typeof(T) == typeof(double))
                    return (T)(object)value.ToDouble();

                try
                {
                    return value.DeSerializeObject<T>();
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Sset a setting with key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Set(string key, object value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Type type = value.GetType();
                string valueString = string.Empty;

                if (type == typeof(string))
                    valueString = value.ToString();

                if (type == typeof(bool))
                    valueString = value.ToString();

                if (type == typeof(int))
                    valueString = value.ToString();

                if (type == typeof(double))
                    valueString = value.ToString();

                if (string.IsNullOrEmpty(valueString))
                    valueString = value.SerializeObject();

                XmlNode node = this.getXMLNode(key, true);
                if (node != null)
                {
                    node.Attributes.GetNamedItem("value").InnerText = valueString;
                    node.Attributes.GetNamedItem("type").InnerText = type.ToString();
                    this.document.Save(this.fileInfo.FullName);
                }
            }
        }

        #region helper
        /// <summary>
        /// get the xml node 
        /// </summary>
        /// <param name="key">name of the node</param>
        /// <returns>xml node with name</returns>
        private XmlNode getXMLNode(string key, bool createNew = false)
        {
            foreach (XmlNode node in this.document.SelectSingleNode("/settings").ChildNodes)
            {
                if (node.Attributes.GetNamedItem("name").Value.ToLower() == key.ToLower())
                    return node;
            }

            if (createNew)
            {
                XmlNode node = this.document.CreateElement("setting");

                XmlAttribute attribute = this.document.CreateAttribute("name");
                attribute.InnerText = key;
                node.Attributes.Append(attribute);

                node.Attributes.Append(this.document.CreateAttribute("value"));
                node.Attributes.Append(this.document.CreateAttribute("type"));

                this.document.SelectSingleNode("/settings").AppendChild(node);

                return node;
            }
            return null;
        }

        #endregion
    }
}
