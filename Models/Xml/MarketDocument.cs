using System.Xml.Serialization;

namespace nigo.Models
{
	[XmlRoot(ElementName = "Publication_MarketDocument")]
	public class PublicationMarketDocument : XmlDocument
	{
        public const string BaseNamespace = "urn:iec62325.351:tc57wg16:451-3:publicationdocument:";

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
					avgPriceAmount += p.Price;
				}
			//}
			return avgPriceAmount / TimeSeries[0].Period.Point.Count;
		}

		[XmlText]
		public string Text { get; set; }

        public DateTime GetMaxDate()
        {
            if (TimeSeries == null || TimeSeries.Count == 0)
                return DateTime.MinValue;

            DateTime maxDate = DateTime.MinValue;
            foreach (var timeSeries in TimeSeries)
            {
                if (timeSeries.Period?.Point == null)
                    continue;

                foreach (var point in timeSeries.Period.Point)
                {
                    if (point.Date > maxDate)
                        maxDate = (DateTime)point.Date;
                }
            }

            return maxDate;
        }

    }
}
