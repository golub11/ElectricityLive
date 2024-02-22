using System;
namespace nigo.Models
{
	public class PvParams
	{

		public double lat { get; set; }
        public double lon { get; set; }
        public double systemCapacity { get; set; }
        public double azimuth { get; set; }
        public double tilt { get; set; }
        public int arrayType { get; set; }
        public int moduleType { get; set; } 
        public double losses { get; set; }
        public string dataset { get; set; }

	}
}

