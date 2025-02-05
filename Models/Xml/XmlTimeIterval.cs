using System;
using System.Xml.Serialization;

namespace nigo.Models.Xml
{
    [XmlRoot(ElementName = "timeInterval")]
    public class XmlTimeIterval
	{

        [XmlElement(ElementName = "start")]
        public string Start { get; set; }

        [XmlElement(ElementName = "end")]
        public string End{ get; set; }
        
        public TimeInterval ToTimeInterval()
        {
            return TimeInterval.FromUTCString(Start, End);
        }
	}
}


