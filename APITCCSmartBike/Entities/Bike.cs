using APITCCSmartBike.Models;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace APITCCSmartBike.Entities
{
    public class Bike
    {
        public string Id { get; set; }

        public string Modelo { get; set; }

        public string Fabricante { get; set; }

        public int VelMax { get; set; }

        public string Autonomia { get; set; }

        public double Peso { get; set; }

        public double PesoMax { get; set; }

        public int NivelAux { get; set; }

        public CoordinateModel Coordinates { get; set; }

        public float Speed { get; set; }

        public double BatteryLevel { get; set; }

        public bool Locked { get; set; }
    }
}
