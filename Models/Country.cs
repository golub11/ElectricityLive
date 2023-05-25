using System;
namespace nigo.Models
{
	public class Country
	{
		internal String name;
        //internal TimeInterval timeInterval;
		internal EnergyCategory energy;

        public Country(string name, EnergyCategory energy)
        {
            this.name = name;
            this.energy = energy;
        }


    }
}

