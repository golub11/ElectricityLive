using System.Xml.Serialization;

namespace nigo.Models
{
	[XmlRoot(ElementName = "Period")]
	public class Period
	{


		[XmlElement(ElementName = "resolution")]
		public string Resolution { get; set; }

		[XmlElement(ElementName = "Point")]
		public List<Point> Point { get; set; }
	}
}