using System.Xml.Serialization;

namespace nigo.Models
{
	[XmlRoot(ElementName = "Point")]
	public class Point
	{

		[XmlElement(ElementName = "position")]
		public int Position { get; set; }

		[XmlElement(ElementName = "price.amount")]
		public double PriceAmount { get; set; }
	}
}