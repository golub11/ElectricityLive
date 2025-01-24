using System;
namespace nigo.Models
{
	public class DayAhead
	{

        public string country { get; set; }
        public List<Point> points{ get; set; }
        
        public DayAhead(
            string country,
            List<Point> points
        )
        {
            this.country = country;
            this.points = points;
        }

    }
}

