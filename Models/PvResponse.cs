using System;
namespace nigo.Models
{
    public class PvResponse
    {
        public Outputs outputs { get; set; }
    }

    public class Outputs
    {
        public List<double> ac_monthly { get; set; }
        public double ac_annual { get; set; }
    }

}

