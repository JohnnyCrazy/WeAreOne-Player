using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Diagnostics;
namespace WeAreOneTrackInfo
{
    class SettingsManager
    {
        public static int VolumeSpeech { get; set; }
        public static int VolumeMain { get; set; }
        public static String Station { get; set; }


        public static void Save()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("weareone.xml");
            XmlNode node = doc.SelectSingleNode("settings");

            node.Attributes[0].Value = Convert.ToString(VolumeMain);
            node.Attributes[1].Value = Convert.ToString(VolumeSpeech);
            node.Attributes[2].Value = Station;

            doc.Save("weareone.xml");
        }

        private static void Create()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create("weareone.xml",settings))
            {
                writer.WriteStartElement("settings");
                writer.WriteAttributeString("VolumeMain", Convert.ToString(50));
                writer.WriteAttributeString("VolumeSpeech", Convert.ToString(50));
                writer.WriteAttributeString("Station", "TechnoBase");
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        public static void Load()
        {
            if (!File.Exists("weareone.xml"))
                Create();
            XmlDocument doc = new XmlDocument();
            doc.Load("weareone.xml");

            XmlNode node = doc.SelectSingleNode("settings");
            try
            {
                VolumeMain = Convert.ToInt32(node.Attributes[0].Value);
                VolumeSpeech = Convert.ToInt32(node.Attributes[1].Value);
                Station = node.Attributes[2].Value;
            }
            catch(Exception)
            {
                throw new Exception("Es gab ein Fehler beim Auslesen der Settings!");
            }
        }
    }
}
