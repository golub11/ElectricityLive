using System.Xml.Serialization;

namespace nigo.Models
{
	[XmlRoot(ElementName = "Publication_MarketDocument", Namespace = "urn:iec62325.351:tc57wg16:451-3:publicationdocument:7:0")]
	public class PublicationMarketDocument : XmlDocument
	{
		public string Country { get; set; }

		[XmlElement(ElementName = "revisionNumber")]
		public int RevisionNumber { get; set; }

		[XmlElement(ElementName = "type")]
		public string Type { get; set; }

		[XmlElement(ElementName = "TimeSeries")]
		public List<TimeSeries> TimeSeries { get; set; }


        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }

		public double getAvgPriceAmount()
		{
			double avgPriceAmount = 0.0;
			//foreach (TimeSeries t in TimeSeries) { 
				foreach (Point p in TimeSeries[0].Period.Point)
				{
					avgPriceAmount += p.PriceAmount;
				}
			//}
			return avgPriceAmount / TimeSeries[0].Period.Point.Count;
		}

		[XmlText]
		public string Text { get; set; }


	}
}
