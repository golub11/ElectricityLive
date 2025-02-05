using System.Xml.Serialization;
using nigo.Models.Xml;

namespace nigo.Models
{
	[XmlRoot(ElementName = "Period")]
	public class Period
	{


		[XmlElement(ElementName = "resolution")]
		public string Resolution { get; set; }

		[XmlElement(ElementName = "Point")]
		public List<Point> Point { get; set; }

		[XmlElement(ElementName = "timeInterval")]
		public XmlTimeIterval xmlTimeIterval ;



	}
}