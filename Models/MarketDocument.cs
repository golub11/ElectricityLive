using System.Xml.Serialization;

namespace nigo.Models
{
	[XmlRoot(ElementName = "Publication_MarketDocument", Namespace = "urn:iec62325.351:tc57wg16:451-3:publicationdocument:7:0")]
	public class PublicationMarketDocument : XmlDocument
	{

		[XmlElement(ElementName = "revisionNumber")]
		public int RevisionNumber { get; set; }

		[XmlElement(ElementName = "type")]
		public string Type { get; set; }

		[XmlElement(ElementName = "TimeSeries")]
		public List<TimeSeries> TimeSeries { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
		

		[XmlText]
		public string Text { get; set; }


	}
}
