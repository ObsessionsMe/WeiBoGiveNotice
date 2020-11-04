using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace WeiBoGiveNotice
{
    public class CfgMgr
    {
        //public static Configuration config = ConfigurationManager.OpenExeConfiguration(getConfigFileMap());

        private static string GetConfigFilePath() {
            //使用exe目录下时间
            var files = Directory.GetFiles(Application.StartupPath, "*.exe.config", SearchOption.TopDirectoryOnly);
            return files[0];
        }

        private static XmlDocument GetConfigXmlDocument()
        {
            XmlDocument xmlDocument;
            xmlDocument = new XmlDocument();
            xmlDocument.Load(GetConfigFilePath());
            return xmlDocument;
        }


        public static string GetValue(string AppKey)
        {
            //return config.AppSettings.Settings[key].Value;
            var xDoc = GetConfigXmlDocument();
            XmlNode xNode;
            XmlElement xElem1;
            xNode = xDoc.SelectSingleNode("//appSettings");
            xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            return xElem1.GetAttribute("value");
        }

        public static void SaveValue(string AppKey, string AppValue)
        {
            var xDoc = GetConfigXmlDocument();
            XmlNode xNode;
            XmlElement xElem1;
            XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");

            xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            if (xElem1 != null) xElem1.SetAttribute("value", AppValue);
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", AppKey);
                xElem2.SetAttribute("value", AppValue);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(GetConfigFilePath());
            //config.AppSettings.Settings[key].Value = value;
            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
