using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace WeAreOneTrackInfo
{
    [XmlRoot("weareone")]
    [Serializable]
    public class Response
    {
        public Response() { this.radio = new List<Radio>(); }
        [XmlElement("radio")]
        public List<Radio> radio { get; set; }

        
    }
    public class Radio
    {
        [XmlElement("name")]
        public String name { get; set; }
        [XmlElement("moderator")]
        public String moderator { get; set; }
        [XmlElement("show")]
        public String show { get; set; }
        [XmlElement("style")]
        public String style { get; set; }
        [XmlElement("starttime")]
        public int starttime { get; set; }
        [XmlElement("endtime")]
        public int endtime { get; set; }
        [XmlElement("link")]
        public String link { get; set; }
        [XmlElement("picture")]
        public String picture { get; set; }
        [XmlElement("artist")]
        public String artist { get; set; }
        [XmlElement("song")]
        public String song { get; set; }
        [XmlElement("release")]
        public String release { get; set; }
        [XmlElement("listener")]
        public int listener { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}
