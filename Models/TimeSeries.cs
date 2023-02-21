using System.Xml.Serialization;

namespace nigo.Models
{

	[XmlRoot(ElementName = "TimeSeries")]
	public class TimeSeries
	{

		[XmlElement(ElementName = "mRID")]
		public int MRID { get; set; }

		[XmlElement(ElementName = "businessType")]
		public string BusinessType { get; set; }

		[XmlElement(ElementName = "currency_Unit.name")]
		public string CurrencyUnitName { get; set; }

		[XmlElement(ElementName = "price_Measure_Unit.name")]
		public string PriceMeasureUnitName { get; set; }

		[XmlElement(ElementName = "curveType")]
		public string CurveType { get; set; }

		[XmlElement(ElementName = "Period")]
		public Period Period { get; set; }
	}

}