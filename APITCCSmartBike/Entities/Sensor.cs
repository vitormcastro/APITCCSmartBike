using APITCCSmartBike.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Entities
{
    public class Sensor
    {
        
        public string Id { get; set; }
        
        public double BatteryLevel { get; set; }

        public CoordinateModel Location { get; set; }

        public bool Locked { get; set; }

        public float Speed { get; set; }
    }
}
