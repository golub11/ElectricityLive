using System;
namespace nigo.Models
{
	public class DayAhead
	{
        public TimeInterval timeInterval;

        public string country { get; set; }
        public List<Point> points{ get; set; }
        public DateTime dateFrom { get; set; } 
        public DateTime dateTo { get; set; }
        public double dailyPrice { get; set; }
        public string freqInMins { get; set; }

        public DayAhead(
            TimeInterval timeInterval,
            string country,
            List<Point> points,
            double dailyPrice,
            string freqInMins
        )
        {
            this.country = country;
            this.dateFrom = timeInterval.from;
            this.dateTo = timeInterval.to;
            this.points = points;
            this.dailyPrice = dailyPrice;
            this.freqInMins = freqInMins;
        }

    }
}

