using System.Xml.Serialization;

namespace nigo.Models
{
    [XmlRoot("ErrorTypeDocument")]
    public class ErrorTypeDocument : XmlDocument
    {
        [XmlElement(ElementName = "error")]
        public string Error { get; set; }

        [XmlElement(ElementName = "message")]
        public string Message { get; set; } 
    }
}
